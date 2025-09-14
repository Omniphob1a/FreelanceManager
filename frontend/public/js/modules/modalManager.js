// modules/modalManager.js
import { ProjectAPI } from '../api.js';
import { showToast, formatCategory } from './ui.js';
import { loadPage } from './routing.js';
import { removeProjectFromUI, updateProjectInUI } from './projects.js';
import { formatDate, getStatusClass, getStatusText, formatBudget, getStatusHint, getFileIconClass, formatFileSize } from './ui.js';

let currentModal = null;
let currentProjectId = null;
let currentProjectData = null; // ДОБАВЛЕНО: для хранения текущих данных проекта
let flatpickrInstance = null;

export async function openProjectModal(projectId) {
  try {
    closeProjectModal();
    currentProjectId = projectId;

    const modalRoot = document.getElementById('modal-root');
    if (!modalRoot) return;

    const overlay = document.createElement('div');
    overlay.className = 'fixed inset-0 bg-black bg-opacity-50 z-40';
    overlay.onclick = closeProjectModal;

    const container = document.createElement('div');
    container.className = 'fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 bg-white rounded-xl shadow-2xl z-50 w-11/12 max-w-4xl max-h-[90vh] overflow-auto';
    container.onclick = e => e.stopPropagation();

    modalRoot.append(overlay, container);
    container.innerHTML = `<div class="text-center p-8">
      <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500 mx-auto"></div>
      <p class="mt-4 text-gray-600">Loading project details...</p>
    </div>`;

    const project = await ProjectAPI.getProjectById(projectId);

     if (!project) {
        showToast('Project not found', 'error');
        closeProjectModal();
        return;
     }

    currentProjectData = project; // ДОБАВЛЕНО: сохраняем данные проекта
    await renderProjectModal(project, container);

    currentModal = { overlay, container };
  } catch (err) {
    console.error('Failed to open project modal:', err);
    let errorMessage = 'Failed to load project details';
    if (err.status === 404) {
      errorMessage = 'Project not found';
    }
    showToast('Failed to load project details', 'error');
    closeProjectModal();
  }
}

export function closeProjectModal() {
  if (flatpickrInstance) {
    flatpickrInstance.destroy();
    flatpickrInstance = null;
  }
  
  if (currentModal) {
    currentModal.overlay.remove();
    currentModal.container.remove();
    currentModal = null;
    currentProjectId = null;
    currentProjectData = null; // ДОБАВЛЕНО: очищаем данные
  }
}

async function refreshProject() {
  if (!currentModal || !currentProjectId) return;
  
  try {
    console.log('Refreshing project:', currentProjectId);
    
    // Показываем индикатор загрузки
    const container = currentModal.container;
    const statusElements = container.querySelectorAll('#projectStatus, #projectStatusBadge');
    statusElements.forEach(el => {
      el.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Updating...';
    });
    
    const project = await ProjectAPI.getProjectById(currentProjectId);
    
    if (!project) {
      showToast('Project not found', 'error');
      closeProjectModal();
      return;
    }
    
    console.log('Project refreshed with status:', project.status);
    
    currentProjectData = project; // ДОБАВЛЕНО: обновляем сохраненные данные
    
    // Обновляем модальное окно
    await renderProjectModal(project, currentModal.container);
    
    // Обновляем проект в UI списков/карточек
    updateProjectInUI(project);
    
    // Принудительно обновляем статистику дашборда
    try {
      // Импортируем функцию обновления статистики
      const { loadProjects } = await import('./projects.js');
      const projects = await loadProjects();
      
      // Обновляем статистику если элементы существуют
      const { updateDashboardStats } = await import('./dashboardStats.js');
      updateDashboardStats(projects);
      
    } catch (error) {
      console.warn('Failed to update dashboard stats:', error);
    }
    
  } catch (error) {
    console.error('Failed to refresh project:', error);
    showToast('Failed to refresh project data', 'error');
    
    // Восстанавливаем статус элементы в случае ошибки
    const container = currentModal.container;
    const statusElements = container.querySelectorAll('#projectStatus, #projectStatusBadge');
    statusElements.forEach(el => {
      el.textContent = 'Error loading';
      el.className = 'px-3 py-1 rounded-full text-xs font-semibold bg-red-100 text-red-800';
    });
  }
}

async function renderProjectModal(project, container) {
  try {
    const res = await fetch('partials/project-detail.html');
    if (!res.ok) throw new Error('Template not found');
    const html = await res.text();
    container.innerHTML = html;

    // Fill project data
    container.querySelector('#projectTitle').textContent = project.title || 'Untitled Project';
    container.querySelector('#projectDescription').textContent = project.description || 'No description provided';
    container.querySelector('#projectIdText').textContent = project.id || 'N/A';
    container.querySelector('#projectCategory').textContent = formatCategory(project.category) || '-';
    // Status
    const statusEl = container.querySelector('#projectStatus');
    const statusBadge = container.querySelector('#projectStatusBadge');
    if (statusEl && statusBadge) {
      statusEl.textContent = getStatusText(project.status);
      statusEl.className = `px-3 py-1 rounded-full text-xs font-semibold ${getStatusClass(project.status)}`;
      statusBadge.textContent = getStatusText(project.status);
      statusBadge.className = `px-3 py-1 rounded-full text-xs font-semibold ${getStatusClass(project.status)}`;
    }

    // Budget
    container.querySelector('#projectBudget').textContent = formatBudget(project);

    // Dates
    container.querySelector('#projectCreated').textContent = project.createdAt ? formatDate(project.createdAt) : '-';
    
    const expiresEl = container.querySelector('#projectExpires');
    if (expiresEl) {
      if (project.expiresAt) {
        const expires = new Date(project.expiresAt);
        const diff = Math.ceil((expires - new Date()) / 86400000);
        let txt = formatDate(project.expiresAt);
        txt += diff > 0 ? ` (${diff} days left)` : diff === 0 ? ' (Today)' : ` (${Math.abs(diff)} days ago)`;
        expiresEl.textContent = txt;
      } else {
        expiresEl.textContent = '-';
      }
    }

    // Status hint
    container.querySelector('#statusHint').textContent = getStatusHint(project.status);

    // Tags count
    container.querySelector('#tagsCount').textContent = project.tags?.length || 0;

    // Setup all components
    setupTabs(container);
    setupActionButtons(container, project);
    setupAttachments(container, project);
    setupMilestones(container, project);
    setupTags(container, project);
    setupDescriptionEditor(container, project);
    setupTeamManagement(container, project);
    container.querySelector('.close-btn')?.addEventListener('click', closeProjectModal);
  } catch (err) {
    console.error('Error rendering project modal:', err);
    
    let errorMessage = err.message;
    if (err.status === 404) {
      errorMessage = 'Project not found or unavailable';
    }
    
    container.innerHTML = `<div class="bg-white p-6 max-w-md w-full rounded-lg">
      <h3 class="text-lg font-medium text-red-600 mb-2">Error</h3>
      <p class="text-gray-600 mb-4">${errorMessage}</p>
      <div class="flex justify-between">
        <button id="retryProjectLoad" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
          Try Again
        </button>
        <button id="closeErrorModal" class="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300">
          Close
        </button>
      </div>
    </div>`;
    
    // Обработчик для повторной попытки
    container.querySelector('#retryProjectLoad')?.addEventListener('click', async () => {
      try {
        const project = await ProjectAPI.getProjectById(currentProjectId);
        await renderProjectModal(project, container);
      } catch (retryError) {
        console.error('Retry failed:', retryError);
        showToast('Failed to load project, please try again later', 'error');
      }
    });
    
    container.querySelector('#closeErrorModal')?.addEventListener('click', closeProjectModal);
  }
}

function setupTabs(container) {
  const firstTab = container.querySelector('.tab-btn');
  if (firstTab) {
    const tabName = firstTab.dataset.tab || 'overview';
    switchTab(container, tabName);
  }

  container.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.preventDefault();
      const tabName = btn.dataset.tab || 'overview';
      switchTab(container, tabName);
    });
  });
}

function setupTeamManagement(container, project) {
    const addMemberBtn = container.querySelector('#addTeamMemberBtn');
    const addMemberForm = container.querySelector('#addMemberForm');
    const cancelAddMemberBtn = container.querySelector('#cancelAddMemberBtn');
    const saveMemberBtn = container.querySelector('#saveMemberBtn');
    const teamMembersContainer = container.querySelector('#teamMembersContainer');

    if (!addMemberBtn || !addMemberForm) return;

    // Обработчик кнопки добавления участника
    addMemberBtn.addEventListener('click', () => {
        addMemberForm.classList.remove('hidden');
    });

    // Обработчик отмены добавления
    cancelAddMemberBtn.addEventListener('click', () => {
        addMemberForm.classList.add('hidden');
    });

    // Обработчик сохранения участника
    saveMemberBtn.addEventListener('click', async () => {
        const loginInput = container.querySelector('#memberLoginInput');
        const roleSelect = container.querySelector('#memberRoleInput');
        const login = loginInput.value.trim();
        const role = roleSelect.value;

        if (!login) {
            showToast('Please enter login', 'error');
            return;
        }

        try {
            await ProjectAPI.addMember(project.id, login, role);
            showToast('Member added successfully');
            addMemberForm.classList.add('hidden');
            loginInput.value = '';
            await loadTeamMembers(container, project.id);
        } catch (error) {
            showToast(error.message, 'error');
        }
    });

    // Функция загрузки участников
    async function loadTeamMembers(container, projectId) {
        try {
            const members = await ProjectAPI.getProjectMembers(projectId);
            renderTeamMembers(container, members);
        } catch (error) {
            showToast('Failed to load team members', 'error');
        }
    }

    // Функция отображения участников
    function renderTeamMembers(container, members) {
        const teamMembersContainer = container.querySelector('#teamMembersContainer');
        if (!teamMembersContainer) return;

        if (!members || members.length === 0) {
            teamMembersContainer.innerHTML = '<div class="text-center py-4 text-gray-500">No team members added</div>';
            return;
        }

        teamMembersContainer.innerHTML = members.map(member => `
            <div class="team-member-item bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center justify-between">
                <div class="flex items-center">
                    <div class="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center mr-3">
                        <i class="fas fa-user text-gray-400"></i>
                    </div>
                    <div>
                        <h4 class="font-medium text-gray-800">${member.user?.name || 'Unknown User'}</h4>
                        <p class="text-sm text-gray-500">${member.user?.login || 'No login'} • ${member.role}</p>
                        ${member.user?.birthday ? `<p class="text-xs text-gray-400">Birthday: ${formatDate(member.user.birthday)}</p>` : ''}
                    </div>
                </div>
                <button class="remove-member-btn text-red-600 hover:text-red-800 p-2" data-login="${member.user?.login}">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `).join('');

        // Обработчики удаления участников
        teamMembersContainer.querySelectorAll('.remove-member-btn').forEach(btn => {
            btn.addEventListener('click', async () => {
                const login = btn.dataset.login;
                if (confirm(`Remove ${login} from project?`)) {
                    try {
                        await ProjectAPI.removeMember(project.id, login);
                        showToast('Member removed successfully');
                        await loadTeamMembers(container, project.id);
                    } catch (error) {
                        showToast(error.message, 'error');
                    }
                }
            });
        });
    }

    // Загружаем участников при открытии вкладки Team
    const teamTabBtn = container.querySelector('.tab-btn[data-tab="team"]');
    if (teamTabBtn) {
        teamTabBtn.addEventListener('click', () => {
            loadTeamMembers(container, project.id);
        });
    }
}

function switchTab(container, tabName) {
  container.querySelectorAll('.tab-content').forEach(tab => {
    tab.classList.remove('active');
    tab.classList.add('hidden');
  });

  container.querySelectorAll('.tab-btn').forEach(btn => {
    btn.classList.remove('active', 'text-blue-700', 'border-b-2', 'border-blue-700');
    btn.classList.add('text-gray-500');
  });

  const activeTab = container.querySelector(`#${tabName}Tab`);
  if (activeTab) {
    activeTab.classList.remove('hidden');
    activeTab.classList.add('active');
  }

  const activeBtn = container.querySelector(`.tab-btn[data-tab="${tabName}"]`);
  if (activeBtn) {
    activeBtn.classList.add('active', 'text-blue-700', 'border-b-2', 'border-blue-700');
    activeBtn.classList.remove('text-gray-500');
  }
}

function setupActionButtons(container, project) {
  container.querySelector('.close-btn')?.addEventListener('click', closeProjectModal);
  container.querySelector('.close-modal-btn')?.addEventListener('click', closeProjectModal);

  // ИСПРАВЛЕНО: Кнопка Edit должна открывать форму редактирования с ID проекта
  container.querySelector('#editProjectBtn')?.addEventListener('click', () => {
    closeProjectModal();
    window.location.hash = `project-form?id=${project.id}`;
  });

  container.querySelector('#deleteProjectBtn')?.addEventListener('click', async () => {
    if (confirm('Are you sure you want to delete this project?')) {
      const btn = container.querySelector('#deleteProjectBtn');
      const originalText = btn.innerHTML;
      
      try {
        btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Deleting...';
        btn.disabled = true;
        
        await ProjectAPI.deleteProject(project.id);
        closeProjectModal();
        removeProjectFromUI(project.id);
        showToast('Project deleted successfully');
      } catch (error) {
        console.error('Delete error:', error);
        showToast('Failed to delete project', 'error');
        btn.innerHTML = originalText;
        btn.disabled = false;
      }
    }
  });

  container.querySelector('#publishProjectBtn')?.addEventListener('click', () => {
    showPublishForm(container, project);
  });

  container.querySelector('#completeProjectBtn')?.addEventListener('click', async () => {
    if (confirm('Mark this project as completed?')) {
      const btn = container.querySelector('#completeProjectBtn');
      const originalText = btn.innerHTML;
      
      try {
        btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Completing...';
        btn.disabled = true;
        
        await ProjectAPI.completeProject(project.id);
        showToast('Project marked as completed');
        await refreshProject();
        
      } catch (error) {
        console.error('Complete error:', error);
        let errorMessage = 'Failed to complete project';
        if (error.status === 400) {
          errorMessage = 'Invalid request. Project may not be in the correct status.';
        } else if (error.status === 404) {
          errorMessage = 'Project not found';
        }
        showToast(errorMessage, 'error');
      } finally {
        btn.innerHTML = originalText;
        btn.disabled = false;
      }
    }
  });

  container.querySelector('#archiveProjectBtn')?.addEventListener('click', async () => {
    if (confirm('Archive this project?')) {
      const btn = container.querySelector('#archiveProjectBtn');
      const originalText = btn.innerHTML;
      
      try {
        btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Archiving...';
        btn.disabled = true;
        
        await ProjectAPI.archiveProject(project.id);
        showToast('Project archived');
        await refreshProject();
      } catch (error) {
        console.error('Archive error:', error);
        showToast('Failed to archive project', 'error');
      } finally {
        btn.innerHTML = originalText;
        btn.disabled = false;
      }
    }
  });
}

function showPublishForm(container, project) {
    const existingForm = container.querySelector('#publishFormContainer');
    if (existingForm) existingForm.remove();
    
    const form = document.createElement('div');
    form.id = 'publishFormContainer';
    form.className = 'bg-blue-50 rounded-xl p-5 mb-6 border border-blue-200';
    form.innerHTML = `
        <h4 class="font-semibold text-gray-800 mb-4">Publish Project</h4>
        <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">Expiration Date*</label>
            <input type="text" id="publishDateInput" 
                   class="w-full px-4 py-2.5 border border-gray-300 rounded-lg">
        </div>
        <div class="flex justify-end space-x-3">
            <button id="cancelPublishBtn" 
                    class="px-4 py-2.5 text-gray-600 hover:bg-gray-100 rounded-lg">
                Cancel
            </button>
            <button id="confirmPublishBtn" 
                    class="px-4 py-2.5 bg-green-500 text-white rounded-lg hover:bg-green-600">
                <span class="button-text">Publish</span>
                <i class="fas fa-spinner fa-spin ml-2 hidden"></i>
            </button>
        </div>
    `;
    
    container.querySelector('#overviewTab').appendChild(form);
    
    if (typeof flatpickr !== 'undefined') {
        flatpickr(form.querySelector('#publishDateInput'), {
            dateFormat: "Y-m-d",
            minDate: "today",
            static: true
        });
    }

    form.querySelector('#cancelPublishBtn').addEventListener('click', () => form.remove());
    
   form.querySelector('#confirmPublishBtn').addEventListener('click', async () => {
    const btn = form.querySelector('#confirmPublishBtn');
    const spinner = btn.querySelector('.fa-spinner');
    const text = btn.querySelector('.button-text');
    
    const date = form.querySelector('#publishDateInput').value;
    if (!date) {
        showToast('Please select expiration date', 'error');
        return;
    }
    
    // Показываем загрузку
    btn.disabled = true;
    spinner.classList.remove('hidden');
    text.textContent = 'Publishing...';
    
    try {
        // Проверяем, что дата валидна
        const selectedDate = new Date(date);
        if (isNaN(selectedDate.getTime())) {
            throw new Error('Invalid date selected');
        }
        
        // Проверяем, что дата в будущем
        if (selectedDate <= new Date()) {
            throw new Error('Expiration date must be in the future');
        }
        
        // Форматируем дату в ISO формат с временем
        const isoDate = selectedDate.toISOString();
        console.log('Publishing project with date:', isoDate);
        
        await ProjectAPI.publishProject(project.id, isoDate);
        showToast('Project published successfully');
        form.remove();
        await refreshProject();
    } catch (error) {
        console.error('Publish error:', error);
        let errorMessage = 'Failed to publish project';
        if (error.message.includes('Invalid date')) {
            errorMessage = 'Please select a valid date';
        } else if (error.message.includes('future')) {
            errorMessage = 'Expiration date must be in the future';
        } else if (error.status === 400) {
            errorMessage = 'Invalid request. Please check the expiration date.';
        } else if (error.status === 404) {
            errorMessage = 'Project not found';
        }
        showToast(errorMessage, 'error');
    } finally {
        // Возвращаем кнопку в исходное состояние
        btn.disabled = false;
        spinner.classList.add('hidden');
        text.textContent = 'Publish';
    }
});
}

// ИСПРАВЛЕНО: Функция обновления описания теперь отправляет все необходимые поля
function setupDescriptionEditor(container, project) {
  const editDescBtn = container.querySelector('#editDescriptionBtn');
  const descEditor = container.querySelector('#descriptionEditor');
  const cancelEditDesc = container.querySelector('#cancelEditDesc');
  const saveDescBtn = container.querySelector('#saveDescriptionBtn');
  const descTextarea = container.querySelector('#descriptionEditor textarea');

  if (!editDescBtn || !descEditor || !saveDescBtn || !cancelEditDesc) return;

  editDescBtn.addEventListener('click', () => {
    descEditor.classList.remove('hidden');
    editDescBtn.classList.add('hidden');
    descTextarea.value = project.description || '';
  });

  cancelEditDesc.addEventListener('click', () => {
    descEditor.classList.add('hidden');
    editDescBtn.classList.remove('hidden');
  });

  saveDescBtn.addEventListener('click', async () => {
    const newDesc = descTextarea.value.trim();
    if (!newDesc) {
      showToast('Description cannot be empty', 'error');
      return;
    }

    const originalText = saveDescBtn.innerHTML;
    
    try {
      saveDescBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Saving...';
      saveDescBtn.disabled = true;
      
      // ИСПРАВЛЕНО: Отправляем все обязательные поля, не только описание
      const updateData = {
        title: currentProjectData.title,
        description: newDesc,
        category: currentProjectData.category,
        currencyCode: currentProjectData.currencyCode || currentProjectData.currency || 'USD',
        budgetMin: currentProjectData.budgetMin,
        budgetMax: currentProjectData.budgetMax,
        tags: currentProjectData.tags || []
      };
      
      console.log('Updating project with data:', updateData);
      
      await ProjectAPI.updateProject(project.id, updateData);
      
      // Обновляем локальные данные
      currentProjectData.description = newDesc;
      
      // Обновляем отображение описания
      container.querySelector('#projectDescription').textContent = newDesc;
      
      // Скрываем редактор
      descEditor.classList.add('hidden');
      editDescBtn.classList.remove('hidden');
      
      showToast('Description updated successfully');
      
    } catch (error) {
      console.error('Update description error:', error);
      let errorMessage = 'Failed to update description';
      
      if (error.message.includes('validation errors')) {
        errorMessage = 'Please fill all required fields correctly';
      } else if (error.status === 400) {
        errorMessage = 'Invalid data provided';
      } else if (error.status === 404) {
        errorMessage = 'Project not found';
      }
      
      showToast(errorMessage, 'error');
    } finally {
      saveDescBtn.innerHTML = originalText;
      saveDescBtn.disabled = false;
    }
  });
}

function setupAttachments(container, project) {
  renderAttachments(container, project.attachments || []);

  container.querySelector('#addAttachmentBtn')?.addEventListener('click', () => {
    document.getElementById('attachmentFile').click();
  });

  document.getElementById('attachmentFile')?.addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    try {
      await ProjectAPI.addAttachment(currentProjectId, file);
      showToast('Attachment uploaded');
      e.target.value = '';
      await refreshProject();
    } catch (error) {
      showToast('Failed to upload attachment', 'error');
      e.target.value = '';
    }
  });
}

function renderAttachments(container, attachments) {
  const attachmentsContainer = container.querySelector('#attachmentsContainer');
  if (!attachmentsContainer) return;

  if (!attachments || attachments.length === 0) {
    attachmentsContainer.innerHTML = '<div class="col-span-full text-center py-8 text-gray-500">No attachments added</div>';
    return;
  }

  attachmentsContainer.innerHTML = attachments.map(attachment => `
    <div class="attachment-item bg-white p-4 rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
      <div class="flex items-center">
        <div class="mr-3">
          <div class="w-12 h-12 rounded-lg bg-blue-100 flex items-center justify-center">
            <i class="fas ${getFileIconClass(attachment.fileName)} text-xl"></i>
          </div>
        </div>
        <div class="flex-grow min-w-0">
          <h4 class="font-medium text-gray-800 truncate">${attachment.fileName}</h4>
          <div class="flex items-center mt-1">
            <span class="text-xs text-gray-500">Size: unknown</span>
            <span class="mx-2 text-gray-300">•</span>
            <span class="text-xs text-gray-500">Added: unknown</span>
          </div>
        </div>
        <div class="flex space-x-2 ml-3">
          <a href="${attachment.url}" target="_blank" class="p-2 text-blue-600 hover:bg-blue-100 rounded-full">
            <i class="fas fa-download"></i>
          </a>
        </div>
      </div>
    </div>
  `).join('');
}

function setupMilestones(container, project) {
  renderMilestones(container, project.milestones || []);

  // Milestone form handlers
  container.querySelector('#addMilestoneBtn')?.addEventListener('click', () => {
    showAddMilestoneForm(container);
  });

  container.querySelector('#saveMilestoneBtn')?.addEventListener('click', async () => {
    const title = container.querySelector('#milestoneTitleInput').value.trim();
    const dueDate = container.querySelector('#milestoneDueDateInput').value;
    
    if (!title || !dueDate) {
      showToast('Please fill all required fields', 'error');
      return;
    }

    try {
      await ProjectAPI.addMilestone(currentProjectId, { title, dueDate });
      showToast('Milestone added');
      container.querySelector('#milestoneFormContainer').classList.add('hidden');
      await refreshProject();
    } catch (error) {
      showToast('Failed to add milestone', 'error');
    }
  });

  container.querySelector('#cancelMilestoneBtn')?.addEventListener('click', () => {
    container.querySelector('#milestoneFormContainer').classList.add('hidden');
  });
}

function renderMilestones(container, milestones) {
  const milestonesContainer = container.querySelector('#milestonesContainer');
  if (!milestonesContainer) return;

  if (!milestones || milestones.length === 0) {
    milestonesContainer.innerHTML = '<div class="text-center py-4 text-gray-500">No milestones added</div>';
    return;
  }

  milestonesContainer.innerHTML = milestones.map(milestone => {
    const milestoneId = milestone.id || milestone.Id || milestone.milestoneId || milestone.MilestoneId;
    
    return `
    <div class="milestone-item bg-white p-5 rounded-xl border border-gray-200 shadow-sm flex items-start">
      <div class="mr-4 mt-1">
        <div class="w-8 h-8 rounded-full ${milestone.isCompleted ? 'bg-green-100' : 'bg-blue-100'} flex items-center justify-center">
          <i class="fas ${milestone.isCompleted ? 'fa-check text-green-600' : 'fa-flag text-blue-600'}"></i>
        </div>
      </div>
      <div class="flex-grow">
        <div class="flex justify-between items-start">
          <div>
            <h4 class="font-semibold text-gray-800">${milestone.title || 'Untitled Milestone'}</h4>
            <div class="flex items-center mt-1">
              <span class="text-xs text-gray-500 mr-3">
                <i class="far fa-calendar mr-1"></i>
                ${milestone.isCompleted ? 'Completed' : 'Due'}: ${formatDate(milestone.dueDate)}
              </span>
              <span class="text-xs px-2 py-1 ${milestone.isCompleted ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'} rounded-full">
                ${milestone.isCompleted ? 'Completed' : 'Pending'}
              </span>
            </div>
          </div>
          ${!milestone.isCompleted ? `
            <button class="complete-milestone-btn p-2 text-green-600 hover:bg-green-100 rounded-full" data-id="${milestoneId}">
              <i class="fas fa-check-circle"></i>
            </button>
          ` : ''}
        </div>
      </div>
    </div>
  `;
  }).join('');

  milestonesContainer.querySelectorAll('.complete-milestone-btn').forEach(btn => {
    btn.addEventListener('click', async () => {
      const milestoneId = btn.dataset.id;
      
      try {
        await ProjectAPI.completeMilestone(currentProjectId, { milestoneId });
        showToast('Milestone completed');
        await refreshProject();
      } catch (error) {
        console.error('Error completing milestone:', error);
        
        let errorMessage = 'Failed to complete milestone';
        if (error.status === 404) {
          errorMessage = 'Milestone or project not found';
        } else if (error.status === 400) {
          errorMessage = 'Invalid request - milestone may already be completed';
        } else if (error.status === 403) {
          errorMessage = 'You do not have permission to complete this milestone';
        }
        
        showToast(errorMessage, 'error');
      }
    });
  });
}

function showAddMilestoneForm(container) {
  const milestoneForm = container.querySelector('#milestoneFormContainer');
  if (!milestoneForm) return;

  milestoneForm.classList.remove('hidden');
  container.querySelector('#milestoneTitleInput').value = '';
  container.querySelector('#milestoneDueDateInput').value = '';

  const dateInput = container.querySelector('#milestoneDueDateInput');
  if (dateInput) {
    if (flatpickrInstance) {
      flatpickrInstance.destroy();
    }
    
    flatpickrInstance = flatpickr(dateInput, {
      dateFormat: "Y-m-d",
      minDate: "today",
      static: true
    });
  }
}

function setupTags(container, project) {
  renderTags(container, project.tags || []);

  container.querySelector('#addTagBtn')?.addEventListener('click', async () => {
    const tagInput = container.querySelector('#newTagInput');
    const tag = tagInput.value.trim();
    if (!tag) return;

    try {
      await ProjectAPI.addTags(currentProjectId, [tag]);
      showToast('Tag added');
      tagInput.value = '';
      await refreshProject();
    } catch (error) {
      showToast('Failed to add tag', 'error');
    }
  });

  container.querySelector('#newTagInput')?.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      container.querySelector('#addTagBtn').click();
    }
  });
}

function renderTags(container, tags) {
  const tagsContainer = container.querySelector('#projectTags');
  if (!tagsContainer) return;

  if (!tags || tags.length === 0) {
    tagsContainer.innerHTML = '<div class="text-sm text-gray-500 w-full">No tags added</div>';
    return;
  }

  tagsContainer.innerHTML = tags.map(tag => `
    <div class="inline-flex items-center px-3 py-1.5 rounded-full text-xs bg-blue-100 text-blue-800 font-medium mr-2 mb-2">
      ${tag}
      <button class="ml-2 text-blue-600 hover:text-blue-800 delete-tag-btn" data-tag="${tag}">
        <i class="fas fa-times text-xs"></i>
      </button>
    </div>
  `).join('');

  tagsContainer.querySelectorAll('.delete-tag-btn').forEach(btn => {
    btn.addEventListener('click', async () => {
      if (confirm(`Remove tag "${btn.dataset.tag}"?`)) {
        try {
          await ProjectAPI.deleteTags(currentProjectId, [btn.dataset.tag]);
          showToast('Tag removed');
          await refreshProject();
        } catch (error) {
          showToast('Failed to remove tag', 'error');
        }
      }
    });
  });
}