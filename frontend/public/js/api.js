/** База API: из .env (VITE_API_URL), по умолчанию локальный gateway. */
const API_BASE_URL = String(import.meta.env?.VITE_API_URL ?? 'http://localhost:5000').replace(/\/$/, '');
/** Docker / cold start: запас по времени для локальной разработки */
const REQUEST_TIMEOUT_MS = 30000;

function toQueryString(filters = {}) {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
        if (value === undefined || value === null || value === '') return;
        if (Array.isArray(value)) {
            value.forEach((item) => {
                if (item !== undefined && item !== null && item !== '') {
                    params.append(key, String(item));
                }
            });
            return;
        }
        params.append(key, String(value));
    });
    return params.toString();
}

function createApiError(method, endpoint, status, message) {
    const error = new Error(`API ${method} ${endpoint} failed: ${status} - ${message}`);
    error.status = status;
    return error;
}

async function fetchAPI(endpoint, method = 'GET', body = null, headers = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    const token = localStorage.getItem('token');
    const requestHeaders = { ...headers };

    if (token) requestHeaders.Authorization = `Bearer ${token}`;

    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

    const options = {
        method,
        headers: requestHeaders,
        credentials: 'include',
        signal: controller.signal
    };

    if (body instanceof FormData) {
        options.body = body;
    } else if (body !== null && body !== undefined) {
        requestHeaders['Content-Type'] = 'application/json';
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(url, options);

        if (!response.ok) {
            let errorText = await response.text();
            if (errorText) {
                try {
                    const errorJson = JSON.parse(errorText);
                    if (Array.isArray(errorJson.errors)) {
                        errorText = errorJson.errors.join(', ');
                    } else if (errorJson.message) {
                        errorText = errorJson.message;
                    }
                } catch {
                    // Keep original text when response is not JSON.
                }
            }
            throw createApiError(method, endpoint, response.status, errorText || 'Unknown API error');
        }

        if (response.status === 204) return null;

        const text = await response.text();
        if (!text) return null;

        try {
            return JSON.parse(text);
        } catch {
            return text;
        }
    } catch (error) {
        if (error.name === 'AbortError') {
            throw createApiError(method, endpoint, 408, 'Request timeout');
        }
        if (!error.status) error.status = 0;
        throw error;
    } finally {
        window.clearTimeout(timeoutId);
    }
}

// =====================
// Project API
// =====================
export const ProjectAPI = {
  async getProjects(filters = {}) {
    const queryParams = toQueryString(filters);
    return fetchAPI(`/api/Projects?${queryParams}`);
  },

  async getProjectById(projectId) {
    try {
      return await fetchAPI(`/api/Projects/${projectId}?includeMilestones=true&includeAttachments=true`);
    } catch (error) {
      if (error.status === 404) {
        console.warn(`Project ${projectId} not found`);
        return null;
      }
      throw error;
    }
  },

  async addMember(projectId, login, role) {
    return fetchAPI(`/api/Projects/${projectId}/add-member`, 'PATCH', { login, role });
  },

  async removeMember(projectId, email) {
    return fetchAPI(`/api/Projects/${projectId}/remove-member`, 'PATCH', { email });
  },

  async getProjectMembers(projectId) {
    return fetchAPI(`/api/Projects/${projectId}/members`);
  },

  async createProject(projectData) {
    const requiredFields = ['title', 'description', 'category'];
    const missingFields = requiredFields.filter(field => !projectData[field]);
    
    if (missingFields.length > 0) {
      throw new Error(`Missing required fields: ${missingFields.join(', ')}`);
    }

    if (projectData.budgetMin && projectData.budgetMax && projectData.budgetMin > projectData.budgetMax) {
      throw new Error('Min budget cannot be greater than max budget');
    }

    return fetchAPI('/api/Projects', 'POST', projectData);
  },

  async updateProject(projectId, projectData) {
    return fetchAPI(`/api/Projects/${projectId}`, 'PUT', projectData);
  },

  async deleteProject(projectId) {
    return fetchAPI(`/api/Projects/${projectId}`, 'DELETE');
  },

  async archiveProject(projectId) {
    return fetchAPI(`/api/Projects/${projectId}/archive`, 'PATCH');
  },

  async publishProject(projectId, expiresAt) {
    return fetchAPI(`/api/Projects/${projectId}/publish`, 'PATCH', { expiresAt });
  },

  async completeProject(projectId) {
    return fetchAPI(`/api/Projects/${projectId}/complete`, 'PATCH');
  },

  async addAttachment(projectId, file) {
    const formData = new FormData();
    formData.append('file', file);
    return fetchAPI(`/api/Projects/${projectId}/add-attachment`, 'PATCH', formData, { isForm: true });
  },

  async deleteAttachment(projectId, attachmentId) {
    return fetchAPI(`/api/Projects/${projectId}/delete-attachment`, 'PATCH', { attachmentId });
  },

  async addMilestone(projectId, milestoneData) {
    const payload = {
        title: milestoneData.title,
        dueDate: milestoneData.dueDate
    };
    if (payload.dueDate && /^\d{4}-\d{2}-\d{2}$/.test(payload.dueDate)) {
        payload.dueDate = new Date(payload.dueDate).toISOString();
    }
    return fetchAPI(`/api/Projects/${projectId}/add-milestone`, 'PATCH', payload);
  },

  async deleteMilestone(projectId, milestoneId) {
    return fetchAPI(`/api/Projects/${projectId}/delete-milestone`, 'PATCH', { milestoneId });
  },

  async completeMilestone(projectId, { milestoneId }) {
    if (!milestoneId || typeof milestoneId !== 'string') {
        throw new Error('milestoneId должен быть строкой');
    }
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    if (!guidRegex.test(milestoneId)) {
        throw new Error('milestoneId должен быть валидным GUID');
    }
    return fetchAPI(`/api/Projects/${projectId}/complete-milestone`, 'PATCH', { MilestoneId: milestoneId });
  },

  async rescheduleMilestone(projectId, milestoneId, newDueDate) {
    return fetchAPI(`/api/Projects/${projectId}/reschedule-milestone`, 'PATCH', { milestoneId, newDueDate });
  },

  async addTags(projectId, tags) {
    return fetchAPI(`/api/Projects/${projectId}/add-tags`, 'PATCH', { tags });
  },

  async deleteTags(projectId, tags) {
    return fetchAPI(`/api/Projects/${projectId}/delete-tags`, 'PATCH', { tags });
  },
};

// =====================
// Auth API
// =====================
export const AuthAPI = {
    async register(userData) { return fetchAPI('/api/Auth/register', 'POST', userData); },
    async login(credentials) { return fetchAPI('/api/Auth/login', 'POST', credentials); },
};

// =====================
// User API
// =====================

/** Нормализация ответа Users API (camelCase / PascalCase, единый id для фронта). */
export function normalizeUserDto(raw) {
    if (!raw || typeof raw !== 'object') return null;
    const u = raw;
    const idRaw = u.id ?? u.Id;
    if (idRaw === undefined || idRaw === null || idRaw === '') return null;
    const id = String(idRaw);
    const rolesRaw = u.roles ?? u.Roles;
    const roles = Array.isArray(rolesRaw) ? rolesRaw.map(String) : [];
    return {
        id,
        login: String(u.login ?? u.Login ?? ''),
        name: String(u.name ?? u.Name ?? ''),
        email: String(u.email ?? u.Email ?? ''),
        gender: Number(u.gender ?? u.Gender ?? 2),
        birthday: u.birthday ?? u.Birthday ?? null,
        createdAt: u.createdAt ?? u.CreatedAt ?? null,
        modifiedOn: u.modifiedOn ?? u.ModifiedOn ?? null,
        admin: Boolean(u.admin ?? u.Admin ?? false),
        roles,
        permissions: Array.isArray(u.permissions ?? u.Permissions)
            ? (u.permissions ?? u.Permissions).map(String)
            : [],
    };
}

export const UserAPI = {
    async getUsers() { return fetchAPI('/api/Users'); },
    async updateUser(userId, userData) { return fetchAPI(`/api/Users/${userId}`, 'PUT', userData); },
    async changePassword(userId, newPassword) {
        return fetchAPI(`/api/Users/${userId}/password`, 'PUT', { newPassword });
    },
    async getProfile() {
        const raw = await fetchAPI('/api/Users/get-my-profile', 'POST');
        return normalizeUserDto(raw);
    },
};

// =====================
// Task API
// =====================
export const TaskAPI = {
    async getTasks(filters = {}) {
        const queryParams = toQueryString(filters);
        return fetchAPI(`/api/ProjectTasks?${queryParams}`);
    },
    async createTask(taskData) { return fetchAPI('/api/ProjectTasks', 'POST', taskData); },
    async getTaskById(taskId, includes = []) {
        const queryParams = includes.length ? `?includes=${includes.join(',')}` : '';
        const task = await fetchAPI(`/api/ProjectTasks/${taskId}${queryParams}`);
        if (!task) {
            throw createApiError('GET', `/api/ProjectTasks/${taskId}${queryParams}`, 404, 'Task not found');
        }
        return task;
    },
    async updateTask(taskId, taskData) { return fetchAPI(`/api/ProjectTasks/${taskId}`, 'PUT', taskData); },
    async deleteTask(taskId) { return fetchAPI(`/api/ProjectTasks/${taskId}`, 'DELETE'); },
    async assignTask(taskId, assigneeId) { return fetchAPI(`/api/ProjectTasks/${taskId}/assign`, 'PATCH', { assigneeId }); },
    async unassignTask(taskId, assigneeId) { return fetchAPI(`/api/ProjectTasks/${taskId}/unassign`, 'PATCH', { assigneeId }); },
    async startTask(taskId) { return fetchAPI(`/api/ProjectTasks/${taskId}/start`, 'PATCH'); },
    async completeTask(taskId) { return fetchAPI(`/api/ProjectTasks/${taskId}/complete`, 'PATCH'); },
    async cancelTask(taskId, reason) { return fetchAPI(`/api/ProjectTasks/${taskId}/cancel`, 'PATCH', { reason }); },
    async addTimeEntry(taskId, timeEntryData) { return fetchAPI(`/api/ProjectTasks/${taskId}/time-entries`, 'POST', timeEntryData); },
    async addComment(taskId, commentData) { return fetchAPI(`/api/ProjectTasks/${taskId}/comments`, 'POST', commentData); },
    async getComments(taskId) { return fetchAPI(`/api/ProjectTasks/${taskId}/comments`); },
    async getProjectMembers(projectId) { return fetchAPI(`/api/ProjectTasks/${projectId}/projectMembers`); },
};

// =====================
// Notifications
// =====================
export const NotificationAPI = {
    async getNotifications() { return []; },
    async markAsRead(notificationId) { return true; },
    async markAllAsRead() { return true; },
};

// =====================
// Activity
// =====================
export const ActivityAPI = {
    async getRecentActivities() {
        return [
            { id: 1, type: 'completed', project: 'Website Redesign', timestamp: new Date(Date.now() - 2*3600000) },
            { id: 2, type: 'comment', user: 'John', project: 'Mobile App Development', timestamp: new Date(Date.now() - 5*3600000) }
        ];
    }
};

// =====================
// Утилиты
// =====================
export function formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount);
}
export function formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
}
