const API_BASE_URL = 'http://localhost:5000';

// Общая функция для выполнения запросов
async function fetchAPI(endpoint, method = 'GET', body = null, headers = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    
    const token = localStorage.getItem('token');
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const options = { method, headers, credentials: 'include' };

    if (body instanceof FormData) {
        options.body = body;
    } else if (body) {
        headers['Content-Type'] = 'application/json';
        options.body = JSON.stringify(body);
    }
    
    // Детальное логирование запроса
    console.log('=== FETCH API DEBUG ===');
    console.log('URL:', url);
    console.log('Method:', method);
    console.log('Headers:', headers);
    console.log('Body:', body);
    console.log('Options:', options);
    
    try {
        const response = await fetch(url, options);
        
        console.log('Response status:', response.status);
        console.log('Response ok:', response.ok);
        console.log('Response headers:', Object.fromEntries(response.headers.entries()));

        if (!response.ok) {
            let errorText = await response.text();
            console.log('Error response text:', errorText);
            
            try {
                const errorJson = JSON.parse(errorText);
                console.log('Parsed error JSON:', errorJson);
                
                // Обрабатываем структурированные ошибки
                if (errorJson.errors && errorJson.errors.length > 0) {
                    errorText = errorJson.errors.join(', ');
                } else if (errorJson.message) {
                    errorText = errorJson.message;
                }
            } catch (parseError) {
                console.log('Failed to parse error as JSON:', parseError);
            }
            
            // Создаем кастомную ошибку с кодом статуса
            const error = new Error(`API ${method} ${endpoint} failed: ${response.status} - ${errorText}`);
            error.status = response.status;
            console.log('Throwing error:', error);
            throw error;
        }

        if (response.status === 204) {
            console.log('204 No Content response');
            return null;
        }
        
        const responseText = await response.text();
        console.log('Response text:', responseText);
        
        if (responseText) {
            try {
                const result = JSON.parse(responseText);
                console.log('Parsed response JSON:', result);
                return result;
            } catch (parseError) {
                console.log('Failed to parse response as JSON:', parseError);
                return responseText;
            }
        }
        
        console.log('Empty response body');
        return null;
        
    } catch (error) {
        console.error('=== FETCH ERROR ===');
        console.error('Network error:', error);
        console.error('Error stack:', error.stack);
        
        // Добавляем информацию о типе ошибки
        if (!error.status) error.status = 0;
        throw error;
    } finally {
        console.log('=== FETCH API DEBUG END ===');
    }
}

// Функции для работы с проектами
export const ProjectAPI = {
  // Получение списка проектов
  async getProjects(filters = {}) {
    const queryParams = new URLSearchParams(filters).toString();
    return fetchAPI(`/api/Projects?${queryParams}`);
  },

  // Получение проекта по ID
 async getProjectById(projectId) {
    try {
      return await fetchAPI(`/api/Projects/${projectId}?includeMilestones=true&includeAttachments=true`);
    } catch (error) {
      // Для 404 ошибок возвращаем null вместо исключения
      if (error.status === 404) {
        console.warn(`Project ${projectId} not found`);
        return null;
      }
      throw error;
    }
  },

  async addMember(projectId, email, role) {
    return fetchAPI(`/api/Projects/${projectId}/add-member`, 'PATCH', { email, role });
  },

// FILE: js/api.js — METHOD: ProjectAPI.removeMember
  async removeMember(projectId, email) {
    return fetchAPI(`/api/Projects/${projectId}/remove-member`, 'PATCH', { email });
  },
  
  async getProjectMembers(projectId) {
    return fetchAPI(`/api/Projects/${projectId}/members`);
  },

  // Создание проекта
  async createProject(projectData) {
    const requiredFields = ['title', 'description', 'ownerId', 'category'];
    const missingFields = requiredFields.filter(field => !projectData[field]);
    
    if (missingFields.length > 0) {
      throw new Error(`Missing required fields: ${missingFields.join(', ')}`);
    }

    if (projectData.budgetMin && projectData.budgetMax && 
        projectData.budgetMin > projectData.budgetMax) {
      throw new Error('Min budget cannot be greater than max budget');
    }

    return fetchAPI('/api/Projects', 'POST', projectData);
  },

  // Обновление проекта
  async updateProject(projectId, projectData) {
    return fetchAPI(`/api/Projects/${projectId}`, 'PUT', projectData);
  },

  // Удаление проекта
  async deleteProject(projectId) {
    return fetchAPI(`/api/Projects/${projectId}`, 'DELETE');
  },

  // Архивация проекта
  async archiveProject(projectId) {
    return fetchAPI(`/api/Projects/${projectId}/archive`, 'PATCH');
  },

  async publishProject(projectId, expiresAt) {
      console.log('Publishing project with data:', { expiresAt });
      // Отправляем expiresAt напрямую, а не как вложенный объект
      return fetchAPI(`/api/Projects/${projectId}/publish`, 'PATCH', {
          expiresAt: expiresAt
      });
  },

  // Завершение проекта - ИСПРАВЛЕНО: отправляем пустое тело или null
  async completeProject(projectId) {
    return fetchAPI(`/api/Projects/${projectId}/complete`, 'PATCH');
  },

  // Добавление вложения
  async addAttachment(projectId, file) {
    const formData = new FormData();
    formData.append('file', file);
    return fetchAPI(`/api/Projects/${projectId}/add-attachment`, 'PATCH', formData, {
      // не устанавливаем Content-Type — браузер сам добавит multipart boundary
      isForm: true,
    });
  },

  // Удаление вложения
  async deleteAttachment(projectId, attachmentId) {
    return fetchAPI(`/api/Projects/${projectId}/delete-attachment`, 'PATCH', { attachmentId });
  },

  // Добавление вехи
  async addMilestone(projectId, milestoneData) {
    // Форматируем данные согласно ожидаемой структуре на бэкенде
    const payload = {
        title: milestoneData.title,
        dueDate: milestoneData.dueDate
    };
    
    // Если дата в формате YYYY-MM-DD, преобразуем в ISO
    if (payload.dueDate && /^\d{4}-\d{2}-\d{2}$/.test(payload.dueDate)) {
        payload.dueDate = new Date(payload.dueDate).toISOString();
    }
    
    console.log('Adding milestone with payload:', payload);
    return fetchAPI(`/api/Projects/${projectId}/add-milestone`, 'PATCH', payload);
},

  // Удаление вехи
  async deleteMilestone(projectId, milestoneId) {
    return fetchAPI(`/api/Projects/${projectId}/delete-milestone`, 'PATCH', { attachmentId: milestoneId });
  },

  // Завершение вехи
  async completeMilestone(projectId, { milestoneId }) {
    // Проверяем, что milestoneId это валидный GUID
    if (!milestoneId || typeof milestoneId !== 'string') {
        throw new Error('milestoneId должен быть строкой');
    }
    
    // Проверяем формат GUID
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    if (!guidRegex.test(milestoneId)) {
        throw new Error('milestoneId должен быть валидным GUID');
    }
    
    console.log('Отправляем запрос с MilestoneId:', milestoneId);
    
    return fetchAPI(
      `/api/Projects/${projectId}/complete-milestone`,
      'PATCH',
      { MilestoneId: milestoneId }
    );
  },

  // Перенос срока вехи
  async rescheduleMilestone(projectId, milestoneId, newDueDate) {
    return fetchAPI(`/api/Projects/${projectId}/reschedule-milestone`, 'PATCH', {
      milestoneId,
      newDueDate,
    });
  },

  // Добавление тегов
  async addTags(projectId, tags) {
    return fetchAPI(`/api/Projects/${projectId}/add-tags`, 'PATCH', { tags });
  },

  // Удаление тегов
  async deleteTags(projectId, tags) {
    return fetchAPI(`/api/Projects/${projectId}/delete-tags`, 'PATCH', { tags });
  },
};


// Функции для аутентификации
export const AuthAPI = {
    // Регистрация
    async register(userData) {
        return fetchAPI('/api/Auth/register', 'POST', userData);
    },
    
    // Логин
    async login(credentials) {
        return fetchAPI('/api/Auth/login', 'POST', credentials);
    },
    
};

// Функции для работы с пользователями
export const UserAPI = {
    // Получение списка пользователей
    async getUsers() {
        return fetchAPI('/api/Users');
    },
    
    // Обновление пользователя
    async updateUser(userId, userData) {
        return fetchAPI(`/api/Users/${userId}`, 'PUT', userData);
    },

    async getProfile() {
        return fetchAPI('/api/Users/get-my-profile', 'POST');
    }
     
};

// Вспомогательные функции
export function formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
    }).format(amount);
}

export function formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
}

export function getDaysLeft(endDate) {
    const today = new Date();
    const dueDate = new Date(endDate);
    const diffTime = dueDate - today;
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
}

// Добавим в конец файла
export const TaskAPI = {
    // Получение задач с фильтрами
    async getTasks(filters = {}) {
        const queryParams = new URLSearchParams();
        Object.entries(filters).forEach(([key, value]) => {
            if (value !== null && value !== undefined) {
                queryParams.append(key, value);
            }
        });
        return fetchAPI(`/api/ProjectTasks?${queryParams}`);
    },

    // Создание задачи
    async createTask(taskData) {
        return fetchAPI('/api/ProjectTasks', 'POST', taskData);
    },

    // Получение задачи по ID
    async getTaskById(taskId, includes = []) {
        const queryParams = includes.length > 0 
            ? `?includes=${includes.join(',')}`
            : '';
        return fetchAPI(`/api/ProjectTasks/${taskId}${queryParams}`);
    },

    // Обновление задачи
    async updateTask(taskId, taskData) {
        return fetchAPI(`/api/ProjectTasks/${taskId}`, 'PUT', taskData);
    },

    // Удаление задачи
    async deleteTask(taskId) {
        return fetchAPI(`/api/ProjectTasks/${taskId}`, 'DELETE');
    },

    // Назначение задачи
    async assignTask(taskId, assigneeId) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/assign`, 'PATCH', { assigneeId });
    },

    // Снятие назначения
    async unassignTask(taskId, assigneeId) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/unassign`, 'PATCH', { assigneeId });
    },

    // Начало работы над задачей
    async startTask(taskId) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/start`, 'PATCH');
    },

    // Завершение задачи
    async completeTask(taskId) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/complete`, 'PATCH');
    },

    // Отмена задачи
    async cancelTask(taskId, reason) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/cancel`, 'PATCH', { reason });
    },

    // Добавление записи времени
    async addTimeEntry(taskId, timeEntryData) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/time-entries`, 'POST', timeEntryData);
    },

    // Добавление комментария
    async addComment(taskId, commentData) {
        return fetchAPI(`/api/ProjectTasks/${taskId}/comments`, 'POST', commentData);
    }
};

export const NotificationAPI = {
    // Получение уведомлений
    async getNotifications() {
        // Заглушка - в реальности запрос к API
        return [];
    },
    
    // Пометить уведомление как прочитанное
    async markAsRead(notificationId) {
        // Заглушка - в реальности запрос к API
        return true;
    },
    
    // Пометить все уведомления как прочитанные
    async markAllAsRead() {
        // Заглушка - в реальности запрос к API
        return true;
    }
};

export const ActivityAPI = {
    async getRecentActivities() {
        // В реальном приложении здесь будет запрос к серверу
        return [
            {
                id: 1,
                type: 'completed',
                project: 'Website Redesign',
                timestamp: new Date(Date.now() - 2 * 3600000) // 2 часа назад
            },
            {
                id: 2,
                type: 'comment',
                user: 'John',
                project: 'Mobile App Development',
                timestamp: new Date(Date.now() - 5 * 3600000) // 5 часов назад
            }
        ];
    }
};

