// modules/modalManager.js
import { ProjectAPI } from '../api.js';
import { showToast, formatCategory } from './ui.js';
import { loadPage } from './routing.js';
import { removeProjectFromUI, updateProjectInUI } from './projects.js';
import { formatDate, getStatusClass, getStatusText, formatBudget, getStatusHint, getFileIconClass, formatFileSize } from './ui.js';
import { localizeRuntimeText, applyLocalization } from './localization.js';

const t = (value) => localizeRuntimeText(value);

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
    container.className = 'fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 bg-white rounded-xl shadow-2xl z-50 w-11/12 max-w-6xl max-h-[99vh] overflow-hidden';
    container.onclick = e => e.stopPropagation();

    modalRoot.append(overlay, container);
    container.innerHTML = `<div class="text-center p-8">
      <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500 mx-auto"></div>
      <p class="mt-4 text-gray-600">${t('Loading project details...')}</p>
    </div>`;

    const project = await ProjectAPI.getProjectById(projectId);

     if (!project) {
        showToast(t('Project not found'), 'error');
        closeProjectModal();
        return;
     }

    currentProjectData = project; // ДОБАВЛЕНО: сохраняем данные проекта
    await renderProjectModal(project, container);

    currentModal = { overlay, container };
  } catch (err) {
    console.error('Failed to open project modal:', err);
    let errorMessage = t('Failed to load project details');
    if (err.status === 404) {
      errorMessage = t('Project not found');
    }
    showToast(errorMessage, 'error');
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
      el.innerHTML = `<i class="fas fa-spinner fa-spin"></i> ${t('Updating...')}`;
    });
    
    const project = await ProjectAPI.getProjectById(currentProjectId);
    
    if (!project) {
      showToast(t('Project not found'), 'error');
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
    showToast(t('Failed to refresh project data'), 'error');
    
    // Восстанавливаем статус элементы в случае ошибки
    const container = currentModal.container;
    const statusElements = container.querySelectorAll('#projectStatus, #projectStatusBadge');
    statusElements.forEach(el => {
      el.textContent = t('Error loading');
      el.className = 'px-3 py-1 rounded-full text-xs font-semibold bg-red-100 text-red-800';
    });
  }
}
function setupEditHandlers(container, project) {
  // Edit description
  const editDescBtn = container.querySelector('#editDescriptionBtn');
  const descForm = container.querySelector('#editDescriptionForm');
  const saveDescBtn = container.querySelector('#saveDescriptionBtn');
  const cancelDescBtn = container.querySelector('#cancelDescriptionBtn');
  const descInput = container.querySelector('#descriptionInput');

  if (editDescBtn && descForm) {
    editDescBtn.addEventListener('click', () => {
      descInput.value = project.description || '';
      descForm.classList.remove('hidden');
      container.querySelector('#projectDescription').classList.add('hidden');
      editDescBtn.classList.add('hidden');
    });

    cancelDescBtn.addEventListener('click', () => {
      descForm.classList.add('hidden');
      container.querySelector('#projectDescription').classList.remove('hidden');
      editDescBtn.classList.remove('hidden');
    });

    saveDescBtn.addEventListener('click', async () => {
      try {
        await ProjectAPI.updateProject(currentProjectId, { description: descInput.value.trim() });
        showToast(t('Description updated'));
        await refreshProject();
      } catch (error) {
        showToast(t('Failed to update description'), 'error');
      }
    });
  }

  // Edit budget
  const editBudgetBtn = container.querySelector('#editBudgetBtn');
  const budgetForm = container.querySelector('#editBudgetForm');
  const saveBudgetBtn = container.querySelector('#saveBudgetBtn');
  const cancelBudgetBtn = container.querySelector('#cancelBudgetBtn');
  const minBudgetInput = container.querySelector('#minBudgetInput');
  const maxBudgetInput = container.querySelector('#maxBudgetInput');
  const currencyInput = container.querySelector('#currencyInput');

  if (editBudgetBtn && budgetForm) {
    editBudgetBtn.addEventListener('click', () => {
      minBudgetInput.value = project.budgetMin || '';
      maxBudgetInput.value = project.budgetMax || '';
      currencyInput.value = project.currencyCode || 'USD';
      budgetForm.classList.remove('hidden');
      container.querySelector('#budgetDisplay').classList.add('hidden');
      editBudgetBtn.classList.add('hidden');
    });

    cancelBudgetBtn.addEventListener('click', () => {
      budgetForm.classList.add('hidden');
      container.querySelector('#budgetDisplay').classList.remove('hidden');
      editBudgetBtn.classList.remove('hidden');
    });

    saveBudgetBtn.addEventListener('click', async () => {
      const budgetData = {
        budgetMin: parseFloat(minBudgetInput.value) || null,
        budgetMax: parseFloat(maxBudgetInput.value) || null,
        currencyCode: currencyInput.value
      };

      if (budgetData.budgetMin && budgetData.budgetMax && budgetData.budgetMin > budgetData.budgetMax) {
        showToast(t('Min budget cannot be greater than max'), 'error');
        return;
      }

      try {
        await ProjectAPI.updateProject(currentProjectId, budgetData);
        showToast(t('Budget updated'));
        await refreshProject();
      } catch (error) {
        showToast(t('Failed to update budget'), 'error');
      }
    });
  }

  // Edit due date
  const editDueDateBtn = container.querySelector('#editDueDateBtn');
  const dueDateForm = container.querySelector('#editDueDateForm');
  const saveDueDateBtn = container.querySelector('#saveDueDateBtn');
  const cancelDueDateBtn = container.querySelector('#cancelDueDateBtn');
  const dueDateInput = container.querySelector('#dueDateInput');

  if (editDueDateBtn && dueDateForm) {
    editDueDateBtn.addEventListener('click', () => {
      dueDateInput.value = project.dueDate ? formatDateForInput(project.dueDate) : '';
      dueDateForm.classList.remove('hidden');
      container.querySelector('#dueDateDisplay').classList.add('hidden');
      editDueDateBtn.classList.add('hidden');

      if (flatpickrInstance) flatpickrInstance.destroy();
      flatpickrInstance = flatpickr(dueDateInput, {
        dateFormat: "Y-m-d",
        minDate: "today",
        static: true,
        position: 'auto'
      });
    });

    cancelDueDateBtn.addEventListener('click', () => {
      dueDateForm.classList.add('hidden');
      container.querySelector('#dueDateDisplay').classList.remove('hidden');
      editDueDateBtn.classList.remove('hidden');
      if (flatpickrInstance) {
        flatpickrInstance.destroy();
        flatpickrInstance = null;
      }
    });

    saveDueDateBtn.addEventListener('click', async () => {
      try {
        await ProjectAPI.updateProject(currentProjectId, { dueDate: dueDateInput.value || null });
        showToast(t('Due date updated'));
        await refreshProject();
      } catch (error) {
        showToast(t('Failed to update due date'), 'error');
      }
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
    container.querySelector('#projectTitle').textContent = project.title || t('Untitled Project');
    container.querySelector('#projectDescription').textContent = project.description || t('No description provided');
    
    // Calculate and display progress based on milestones
    const progress = calculateProjectProgress(project);
    container.querySelector('#projectProgressText').textContent = `${progress}%`;
    container.querySelector('#projectProgressBar').style.width = `${progress}%`;
    
    // Status
    const statusEl = container.querySelector('#projectStatus');
    const statusBadge = container.querySelector('#projectStatusBadge');
    if (statusEl && statusBadge) {
      statusEl.textContent = getStatusText(project.status);
      statusEl.className = `inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold ${getStatusClass(project.status)}`;
      statusBadge.textContent = getStatusText(project.status);
      statusBadge.className = `inline-flex px-3 py-1 rounded-full text-xs font-semibold ${getStatusClass(project.status)}`;
    }

    // Category, Budget, Dates (остается без изменений)
    container.querySelector('#projectCategory').textContent = formatCategory(project.category) || '-';
    container.querySelector('#projectBudget').textContent = formatBudget(project);
    container.querySelector('#projectCreated').textContent = project.createdAt ? formatDate(project.createdAt) : '-';
    
    const expiresEl = container.querySelector('#projectExpires');
    if (expiresEl) {
      if (project.expiresAt) {
        const expires = new Date(project.expiresAt);
        const diff = Math.ceil((expires - new Date()) / 86400000);
        let txt = formatDate(project.expiresAt);
        txt += diff > 0 ? ` (${diff} ${t('days left')})` : diff === 0 ? ` (${t('Today')})` : ` (${Math.abs(diff)} ${t('days ago')})`;
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
    
    // Initialize compact date pickers
    initializeCompactDatePickers(container);
    applyLocalization(container);
  } catch (err) {
    console.error('Error rendering project modal:', err);
    // ... остальная часть обработки ошибок
  }
}

function calculateProjectProgress(project) {
  if (!project.milestones || !Array.isArray(project.milestones) || project.milestones.length === 0) {
    return 0;
  }
  
  const completed = project.milestones.filter(m => m.isCompleted).length;
  return Math.round((completed / project.milestones.length) * 100);
}

function initializeCompactDatePickers(container) {
  if (typeof flatpickr === 'undefined') return;
  
  const dateInputs = container.querySelectorAll('.compact-date-input');
  dateInputs.forEach(input => {
    flatpickr(input, {
      dateFormat: "Y-m-d",
      minDate: "today",
      static: true,
      position: "auto",
      // Компактные настройки
      nextArrow: '<i class="fas fa-chevron-right"></i>',
      prevArrow: '<i class="fas fa-chevron-left"></i>'
    });
  });
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
            <h4 class="font-medium text-gray-800">${member.user?.name || t('Unknown User')}</h4>
            <p class="text-sm text-gray-500">${member.user?.login || t('No login')} • ${member.role}</p>
            ${member.user?.birthday ? `<p class="text-xs text-gray-400">${t('Birthday')}: ${formatDate(member.user.birthday)}</p>` : ''}
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
                if (confirm(`${t('Remove')} ${login} ${t('from project?')}`)) {
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

  // Edit button
  container.querySelector('#editProjectBtn')?.addEventListener('click', () => {
    closeProjectModal();
    window.location.hash = `project-form?id=${project.id}`;
  });

  // Publish button functionality
  const publishBtn = container.querySelector('#publishProjectBtn');
  const publishForm = container.querySelector('#publishFormContainer');
  const cancelPublishBtn = container.querySelector('#cancelPublishBtn');
  const confirmPublishBtn = container.querySelector('#confirmPublishBtn');

  if (publishBtn && publishForm) {
    publishBtn.addEventListener('click', (e) => {
      e.preventDefault();
      const isVisible = !publishForm.classList.contains('hidden');
      
      if (isVisible) {
        // Hide form
        publishForm.classList.add('hidden');
        publishBtn.innerHTML = `<i class="fas fa-paper-plane mr-2"></i> ${t('Publish Project')}`;
        publishBtn.classList.remove('bg-blue-700');
      } else {
        // Show form
        publishForm.classList.remove('hidden');
        publishBtn.innerHTML = `<i class="fas fa-times mr-2"></i> ${t('Cancel')}`;
        publishBtn.classList.add('bg-blue-700');
        
        // Initialize date picker
        const dateInput = container.querySelector('#publishDateInput');
        if (dateInput && typeof flatpickr !== 'undefined') {
          flatpickr(dateInput, {
            dateFormat: "Y-m-d",
            minDate: "today",
            static: true
          });
        }
      }
    });
  }

  if (cancelPublishBtn && publishForm) {
    cancelPublishBtn.addEventListener('click', () => {
      publishForm.classList.add('hidden');
      publishBtn.innerHTML = `<i class="fas fa-paper-plane mr-2"></i> ${t('Publish Project')}`;
      publishBtn.classList.remove('bg-blue-700');
    });
  }

  if (confirmPublishBtn) {
  confirmPublishBtn.addEventListener('click', async () => {
    const dateInput = container.querySelector('#publishDateInput');
    
    if (!dateInput.value) {
      showToast(t('Please select expiration date'), 'error');
      return;
    }

    const btn = confirmPublishBtn;
    const spinner = btn.querySelector('.fa-spinner');
    const text = btn.querySelector('.button-text');
    
    try {
      btn.disabled = true;
      spinner.classList.remove('hidden');
      text.textContent = t('Publishing...');
      
      await ProjectAPI.publishProject(project.id, dateInput.value);
      showToast(t('Project published successfully'));
      
      // Hide form and reset
      publishForm.classList.add('hidden');
      publishBtn.innerHTML = `<i class="fas fa-paper-plane mr-2"></i> ${t('Publish Project')}`;
      publishBtn.classList.remove('bg-blue-700');
      
      await refreshProject();
    } catch (error) {
      console.error('Publish error:', error);
      let errorMessage = t('Failed to publish project');
      if (error.status === 400) {
        errorMessage = t('Invalid request. Please check the expiration date.');
      } else if (error.status === 404) {
        errorMessage = t('Project not found');
      }
      showToast(errorMessage, 'error');
    } finally {
      btn.disabled = false;
      spinner.classList.add('hidden');
      text.textContent = t('Publish');
    }
  });
}

  // Остальные кнопки (delete, complete, archive) остаются без изменений
  container.querySelector('#deleteProjectBtn')?.addEventListener('click', async () => {
    if (confirm(t('Are you sure you want to delete this project?'))) {
      const btn = container.querySelector('#deleteProjectBtn');
      const originalText = btn.innerHTML;
      
      try {
        btn.innerHTML = `<i class="fas fa-spinner fa-spin mr-2"></i> ${t('Deleting...')}`;
        btn.disabled = true;
        
        await ProjectAPI.deleteProject(project.id);
        closeProjectModal();
        removeProjectFromUI(project.id);
        showToast(t('Project deleted successfully'));
      } catch (error) {
        console.error('Delete error:', error);
        showToast(t('Failed to delete project'), 'error');
        btn.innerHTML = originalText;
        btn.disabled = false;
      }
    }
  });

  container.querySelector('#completeProjectBtn')?.addEventListener('click', async () => {
    if (confirm(t('Mark this project as completed?'))) {
      const btn = container.querySelector('#completeProjectBtn');
      const originalText = btn.innerHTML;
      
      try {
        btn.innerHTML = `<i class="fas fa-spinner fa-spin mr-2"></i> ${t('Completing...')}`;
        btn.disabled = true;
        
        await ProjectAPI.completeProject(project.id);
        showToast(t('Project marked as completed'));
        await refreshProject();
        
      } catch (error) {
        console.error('Complete error:', error);
        let errorMessage = t('Failed to complete project');
        if (error.status === 400) {
          errorMessage = t('Invalid request. Project may not be in the correct status.');
        } else if (error.status === 404) {
          errorMessage = t('Project not found');
        }
        showToast(errorMessage, 'error');
      } finally {
        btn.innerHTML = originalText;
        btn.disabled = false;
      }
    }
  });

  container.querySelector('#archiveProjectBtn')?.addEventListener('click', async () => {
    if (confirm(t('Archive this project?'))) {
      const btn = container.querySelector('#archiveProjectBtn');
      const originalText = btn.innerHTML;
      
      try {
        btn.innerHTML = `<i class="fas fa-spinner fa-spin mr-2"></i> ${t('Archiving...')}`;
        btn.disabled = true;
        
        await ProjectAPI.archiveProject(project.id);
        showToast(t('Project archived'));
        await refreshProject();
      } catch (error) {
        console.error('Archive error:', error);
        showToast(t('Failed to archive project'), 'error');
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
        <h4 class="font-semibold text-gray-800 mb-4">${t('Publish Project')}</h4>
        <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">${t('Expiration Date')}*</label>
            <input type="text" id="publishDateInput" 
                   class="w-full px-4 py-2.5 border border-gray-300 rounded-lg">
        </div>
        <div class="flex justify-end space-x-3">
            <button id="cancelPublishBtn" 
                    class="px-4 py-2.5 text-gray-600 hover:bg-gray-100 rounded-lg">
                ${t('Cancel')}
            </button>
            <button id="confirmPublishBtn" 
                    class="px-4 py-2.5 bg-green-500 text-white rounded-lg hover:bg-green-600">
                <span class="button-text">${t('Publish')}</span>
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
        showToast(t('Please select expiration date'), 'error');
        return;
    }
    
    // Показываем загрузку
    btn.disabled = true;
    spinner.classList.remove('hidden');
    text.textContent = t('Publishing...');
    
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
        showToast(t('Project published successfully'));
        form.remove();
        await refreshProject();
    } catch (error) {
        console.error('Publish error:', error);
        let errorMessage = t('Failed to publish project');
        if (error.message.includes('Invalid date')) {
            errorMessage = t('Please select a valid date');
        } else if (error.message.includes('future')) {
            errorMessage = t('Expiration date must be in the future');
        } else if (error.status === 400) {
            errorMessage = t('Invalid request. Please check the expiration date.');
        } else if (error.status === 404) {
            errorMessage = t('Project not found');
        }
        showToast(errorMessage, 'error');
    } finally {
        // Возвращаем кнопку в исходное состояние
        btn.disabled = false;
        spinner.classList.add('hidden');
        text.textContent = t('Publish');
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
      showToast(t('Description cannot be empty'), 'error');
      return;
    }

    const originalText = saveDescBtn.innerHTML;
    
    try {
      saveDescBtn.innerHTML = `<i class="fas fa-spinner fa-spin mr-2"></i> ${t('Saving...')}`;
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
      
      showToast(t('Description updated successfully'));
      
    } catch (error) {
      console.error('Update description error:', error);
      let errorMessage = t('Failed to update description');
      
      if (error.message.includes('validation errors')) {
        errorMessage = t('Please fill all required fields correctly');
      } else if (error.status === 400) {
        errorMessage = t('Invalid data provided');
      } else if (error.status === 404) {
        errorMessage = t('Project not found');
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
    // Используем глобальный файловый инпут вместо поиска в контейнере
    const fileInput = document.getElementById('globalAttachmentFile');
    if (fileInput) {
      fileInput.click();
    }
  });

  // Обработчик изменения файла
  document.getElementById('globalAttachmentFile')?.addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    try {
      await ProjectAPI.addAttachment(currentProjectId, file);
      showToast(t('Attachment uploaded'));
      e.target.value = '';
      await refreshProject();
    } catch (error) {
      showToast(t('Failed to upload attachment'), 'error');
      e.target.value = '';
    }
  });
}

function renderAttachments(container, attachments) {
  const attachmentsContainer = container.querySelector('#attachmentsContainer');
  if (!attachmentsContainer) return;

  if (!attachments || attachments.length === 0) {
    attachmentsContainer.innerHTML = `<div class="text-center py-8 text-gray-500 col-span-full">${t('No attachments yet')}</div>`;
    return;
  }

  attachmentsContainer.innerHTML = attachments.map(attachment => `
    <div class="attachment-item bg-white rounded-lg border border-gray-200 hover:shadow-sm transition-shadow">
      <div class="flex items-center justify-between">
        <div class="flex items-center min-w-0 flex-1">
          <div class="mr-3 flex-shrink-0">
            <div class="w-10 h-10 rounded-lg bg-blue-100 flex items-center justify-center">
              <i class="fas ${getFileIconClass(attachment.fileName)} text-blue-600"></i>
            </div>
          </div>
          <div class="min-w-0 flex-1">
            <h4 class="font-medium text-gray-800 truncate text-sm">${attachment.fileName}</h4>
          </div>
        </div>
        <div class="flex space-x-2 ml-3 flex-shrink-0">
          <a href="${attachment.url}" target="_blank" class="p-2 text-blue-600 hover:bg-blue-100 rounded-full transition-colors">
            <i class="fas fa-download text-sm"></i>
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
      showToast(t('Please fill all required fields'), 'error');
      return;
    }

    try {
      await ProjectAPI.addMilestone(currentProjectId, { title, dueDate });
      showToast(t('Milestone added'));
      container.querySelector('#milestoneFormContainer').classList.add('hidden');
      await refreshProject();
    } catch (error) {
      showToast(t('Failed to add milestone'), 'error');
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
    milestonesContainer.innerHTML = `<div class="text-center py-4 text-gray-500">${t('No milestones added')}</div>`;
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
            <h4 class="font-semibold text-gray-800">${milestone.title || t('Untitled Milestone')}</h4>
            <div class="flex items-center mt-1">
              <span class="text-xs text-gray-500 mr-3">
                <i class="far fa-calendar mr-1"></i>
                ${milestone.isCompleted ? t('Completed') : t('Due')}: ${formatDate(milestone.dueDate)}
              </span>
              <span class="text-xs px-2 py-1 ${milestone.isCompleted ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'} rounded-full">
                ${milestone.isCompleted ? t('Completed') : t('Pending')}
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
        showToast(t('Milestone completed'));
        await refreshProject();
      } catch (error) {
        console.error('Error completing milestone:', error);
        
        let errorMessage = t('Failed to complete milestone');
        if (error.status === 404) {
          errorMessage = t('Milestone or project not found');
        } else if (error.status === 400) {
          errorMessage = t('Invalid request - milestone may already be completed');
        } else if (error.status === 403) {
          errorMessage = t('You do not have permission to complete this milestone');
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
      static: true,
      position: 'auto'
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
      showToast(t('Tag added'));
      tagInput.value = '';
      await refreshProject();
    } catch (error) {
      showToast(t('Failed to add tag'), 'error');
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
    tagsContainer.innerHTML = `<div class="text-sm text-gray-500 w-full">${t('No tags added')}</div>`;
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
      if (confirm(`${t('Remove tag')} "${btn.dataset.tag}"?`)) {
        try {
          await ProjectAPI.deleteTags(currentProjectId, [btn.dataset.tag]);
          showToast(t('Tag removed'));
          await refreshProject();
        } catch (error) {
          showToast(t('Failed to remove tag'), 'error');
        }
      }
    });
  });
}
