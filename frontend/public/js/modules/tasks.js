import { TaskAPI, ProjectAPI, UserAPI, formatDate } from '/js/api.js';
import { showToast, getPriorityClass, getPriorityText, getTaskStatusClass, getTaskStatusText, getDaysLeft, getDaysLeftNumber } from '/js/modules/ui.js';

let currentTasks = [];
let currentFilters = {};
let allProjects = [];
let taskOwnershipFilter = 'all'; // 'all', 'assigned', 'reported'
let currentUserId = null;


export async function initTasksPage() {
  try {
    console.log('Initializing tasks page...');
    
    // Получаем ID текущего пользователя - ДОБАВЛЕНО ДЕТАЛЬНОЕ ЛОГИРОВАНИЕ
    try {
      const user = await UserAPI.getProfile();
      console.log('User profile response:', user); // ✅ Логируем весь ответ
      
      // Пробуем разные возможные пути к ID пользователя
      currentUserId = user.id || user.userId || user.userID || user.Id;
      console.log('Extracted currentUserId:', currentUserId);
      
      if (!currentUserId) {
        console.warn('No user ID found in profile response. Available keys:', Object.keys(user));
      }
    } catch (error) {
      console.error('Failed to get current user profile:', error);
      currentUserId = null;
    }
    
    // Показываем загрузчик
    const loader = document.getElementById('tasksLoader');
    if (loader) loader.classList.remove('hidden');
    
    await loadProjectsForFilter();
    await applyFilters(); // Изменено: вместо loadTasks()
    
    // Инициализация всех обработчиков фильтрации
    setupAllFilters();
    setupExpandableFilters();
    updateFilterCounter();
    updateTaskCounters();
    
    console.log('Tasks page initialized. Current user ID:', currentUserId);
  } catch (error) {
    console.error('Failed to initialize tasks page:', error);
    showToast('Failed to initialize tasks page', 'error');
  } finally {
    // Скрываем загрузчик
    const loader = document.getElementById('tasksLoader');
    if (loader) loader.classList.add('hidden');
  }
}
function setupQuickFilters() {
  const quickStatusFilter = document.getElementById('quickStatusFilter');
  const quickPriorityFilter = document.getElementById('quickPriorityFilter');
  
  if (quickStatusFilter) {
    quickStatusFilter.addEventListener('change', function() {
      console.log('Quick status filter changed:', this.value);
      if (this.value) {
        // Очищаем checkbox статусы в расширенной панели
        document.querySelectorAll('.filter-status').forEach(cb => cb.checked = false);
        currentFilters.status = this.value;
      } else {
        delete currentFilters.status;
      }
      applyFilters();
    });
  }
  
  if (quickPriorityFilter) {
    quickPriorityFilter.addEventListener('change', function() {
      console.log('Quick priority filter changed:', this.value);
      if (this.value) {
        // Очищаем checkbox приоритеты в расширенной панели
        document.querySelectorAll('.filter-priority').forEach(cb => cb.checked = false);
        currentFilters.priority = this.value;
      } else {
        delete currentFilters.priority;
      }
      applyFilters();
    });
  }
}

function setupSearchFilter() {
  const searchFilter = document.getElementById('taskSearch'); // Исправлено с searchFilter на taskSearch
  
  if (searchFilter) {
    let searchTimeout;
    searchFilter.addEventListener('input', (e) => {
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => {
        if (e.target.value) {
          currentFilters.search = e.target.value;
        } else {
          delete currentFilters.search;
        }
        applyFilters();
      }, 500);
    });
  }
}


// ДОБАВЛЕНО: Новая функция для настройки всех фильтров
function setupAllFilters() {
  setupOwnershipFilter();
  setupQuickFilters();
  setupSearchFilter();
  setupTaskEventListeners();
  
  // Инициализируем начальное состояние кнопок владения
  updateOwnershipFilterUI();
}

// ДОБАВЛЕНО: Функция для обновления UI кнопок владения
function updateOwnershipFilterUI() {
  const ownershipButtons = document.querySelectorAll('.task-ownership-filter');
  const activeButton = document.querySelector(`.task-ownership-filter[data-value="${taskOwnershipFilter}"]`);
  
  ownershipButtons.forEach(btn => {
    btn.classList.remove('active', 'bg-blue-600', 'text-white');
    btn.classList.add('text-gray-700', 'hover:bg-gray-100');
  });
  
  if (activeButton) {
    activeButton.classList.add('active', 'bg-blue-600', 'text-white');
    activeButton.classList.remove('text-gray-700', 'hover:bg-gray-100');
  }
}

// ДОБАВЛЕНО: Обновленная функция для кнопок владения
function setupOwnershipFilter() {
  const ownershipButtons = document.querySelectorAll('.task-ownership-filter');
  
  ownershipButtons.forEach(button => {
    button.addEventListener('click', () => {
      console.log('Ownership filter clicked:', button.dataset.value);
      
      // Устанавливаем значение фильтра
      taskOwnershipFilter = button.dataset.value;
      
      // Обновляем UI
      updateOwnershipFilterUI();
      
      // Применяем фильтры
      applyFilters();
    });
  });
  
  // Устанавливаем начальное состояние
  updateOwnershipFilterUI();
}

async function loadTasks(filters = {}) {
  try {
    const loader = document.getElementById('tasksLoader');
    const noTasksRow = document.getElementById('noTasksRow');
    
    if (loader) loader.classList.remove('hidden');
    if (noTasksRow) noTasksRow.classList.add('hidden');

    // Преобразуем фильтры в формат, понятный API
    const apiFilters = {};
    
    // Статус
    if (filters.status) {
      apiFilters.status = filters.status;
    }
    
    // Приоритет
    if (filters.priority) {
      apiFilters.priority = filters.priority;
    }
    
    // Проект
    if (filters.projectId) {
      apiFilters.project = filters.projectId;
    }
    
    // Ключевое исправление: параметры для фильтрации по владению
    if (filters.OnlyMyTasks !== undefined) {
      apiFilters.OnlyMyTasks = filters.OnlyMyTasks;
    }
    
    if (filters.CurrentUserId) {
      apiFilters.CurrentUserId = filters.CurrentUserId;
    }
    
    // Назначенный пользователь
    if (filters.AssigneeId) {
      apiFilters.AssigneeId = filters.AssigneeId;
    }
    
    // Создатель задачи
    if (filters.ReporterId) {
      apiFilters.ReporterId = filters.ReporterId;
    }
    
    // Дата от
    if (filters.dueFrom) {
      apiFilters.dueFrom = new Date(filters.dueFrom).toISOString();
    }
    
    // Дата до
    if (filters.dueTo) {
      const dueToDate = new Date(filters.dueTo);
      dueToDate.setHours(23, 59, 59, 999);
      apiFilters.dueTo = dueToDate.toISOString();
    }
    
    // Минимальные часы
    if (filters.minEstimatedHours) {
      apiFilters.minEstimatedHours = parseFloat(filters.minEstimatedHours);
    }
    
    // Максимальные часы
    if (filters.maxEstimatedHours) {
      apiFilters.maxEstimatedHours = parseFloat(filters.maxEstimatedHours);
    }
    
    // Billable
    if (filters.isBillable) {
      apiFilters.isBillable = filters.isBillable === 'true';
    }
    
    // Просроченные
    if (filters.overdue) {
      apiFilters.overdue = true;
    }

    // Поиск
    if (filters.search) {
      apiFilters.search = filters.search;
    }

    console.log('Loading tasks with API filters:', apiFilters);
    
    const response = await TaskAPI.getTasks(apiFilters);
    currentTasks = response.items || response || [];
    
    // Обновляем UI
    renderTasks(currentTasks);
    updateTaskCounters();
    
    // Показываем/скрываем сообщение о отсутствии задач
    if (currentTasks.length === 0 && noTasksRow) {
      noTasksRow.classList.remove('hidden');
    }
    
    if (loader) loader.classList.add('hidden');
  } catch (error) {
    console.error('Failed to load tasks:', error);
    showToast('Failed to load tasks', 'error');
    
    const loader = document.getElementById('tasksLoader');
    const noTasksRow = document.getElementById('noTasksRow');
    if (loader) loader.classList.add('hidden');
    if (noTasksRow) noTasksRow.classList.remove('hidden');
  }
}

function renderTasks(tasks) {
  const tableBody = document.getElementById('tasksTableBody');
  
  if (!tableBody) return;
  
  tableBody.innerHTML = '';

  if (!tasks || tasks.length === 0) {
    tableBody.innerHTML = `
      <tr>
        <td colspan="7" class="px-6 py-8 text-center text-gray-500">
          No tasks found matching your criteria
        </td>
      </tr>
    `;
    return;
  }

  tasks.forEach(task => {
    const row = createTaskTableRow(task);
    tableBody.appendChild(row);
  });
}

function createTaskTableRow(task) {
  const row = document.createElement('tr');
  row.className = 'hover:bg-gray-50 transition-colors duration-150';
  row.dataset.id = task.id;
  
  const daysLeftNumber = getDaysLeftNumber(task.dueDate);
  const isOverdue = daysLeftNumber !== null && daysLeftNumber < 0;
  const isDueSoon = daysLeftNumber !== null && daysLeftNumber >= 0 && daysLeftNumber < 3;
  
  row.innerHTML = `
    <td class="px-6 py-4 whitespace-nowrap">
      <div class="flex items-center">
        <div class="flex-shrink-0 h-10 w-10 bg-blue-100 rounded-lg flex items-center justify-center mr-4">
          <i class="fas fa-tasks text-blue-600"></i>
        </div>
        <div>
          <div class="text-sm font-medium text-gray-900">${task.title || 'No title'}</div>
          <div class="text-sm text-gray-500 truncate max-w-xs">${task.description || 'No description'}</div>
        </div>
      </div>
    </td>
    <td class="px-6 py-4 whitespace-nowrap">
      <div class="text-sm text-gray-900">${task.projectName || 'No project'}</div>
    </td>
    <td class="px-6 py-4 whitespace-nowrap">
      <div class="flex items-center">
        <div class="flex-shrink-0 h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center overflow-hidden mr-3">
          <img src="https://ui-avatars.com/api/?name=${task.assigneeName || 'Unassigned'}&background=random" alt="${task.assigneeName || 'Unassigned'}">
        </div>
        <div class="text-sm text-gray-900">${task.assigneeName || 'Unassigned'}</div>
      </div>
    </td>
    <td class="px-6 py-4 whitespace-nowrap">
      <div class="text-sm ${isOverdue ? 'text-red-600 font-medium' : isDueSoon ? 'text-orange-600' : 'text-gray-500'}">
        ${task.dueDate ? formatDate(task.dueDate) : 'No due date'}
      </div>
      ${task.dueDate ? `<div class="text-xs ${isOverdue ? 'text-red-500' : isDueSoon ? 'text-orange-500' : 'text-gray-400'}">${getDaysLeft(task.dueDate)}</div>` : ''}
    </td>
    <td class="px-6 py-4 whitespace-nowrap">
      <span class="px-2.5 py-1 text-xs font-medium rounded-full ${getPriorityClass(task.priority)}">
        ${getPriorityText(task.priority)}
      </span>
    </td>
    <td class="px-6 py-4 whitespace-nowrap">
      <span class="px-2.5 py-1 text-xs font-medium rounded-full ${getTaskStatusClass(task.status)}">
        ${getTaskStatusText(task.status)}
      </span>
    </td>
    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
      <div class="flex space-x-2">
        <button class="view-task-btn text-blue-600 hover:text-blue-900 p-1 rounded" title="View Details">
          <i class="fas fa-eye"></i>
        </button>
        <button class="edit-task-btn text-gray-600 hover:text-gray-900 p-1 rounded" title="Edit">
          <i class="fas fa-edit"></i>
        </button>
        <button class="delete-task-btn text-red-600 hover:text-red-900 p-1 rounded" title="Delete">
          <i class="fas fa-trash"></i>
        </button>
      </div>
    </td>
  `;
  
  // Добавляем обработчики событий для кнопок действий
  row.querySelector('.view-task-btn').addEventListener('click', (e) => {
    e.stopPropagation();
    openTaskModal(task.id);
  });
  
  row.querySelector('.edit-task-btn').addEventListener('click', (e) => {
    e.stopPropagation();
    showEditTaskModal(task);
  });
  
  row.querySelector('.delete-task-btn').addEventListener('click', (e) => {
    e.stopPropagation();
    deleteTask(task.id);
  });
  
  // Клик по строке открывает детали задачи
  row.addEventListener('click', () => {
    openTaskModal(task.id);
  });
  
  return row;
}

function setupExpandableFilters() {
  const toggleBtn = document.getElementById('toggleFiltersBtn');
  const panel = document.getElementById('filtersPanel');
  const applyBtn = document.getElementById('applyFiltersBtn');
  const clearBtn = document.getElementById('clearFiltersBtn');

  if (!toggleBtn || !panel) {
    console.warn('Filter elements not found in DOM, skipping filter setup');
    return;
  }

  if (toggleBtn.dataset.filtersInit === 'true') {
    console.log('Expandable filters already initialized');
    return;
  }
  toggleBtn.dataset.filtersInit = 'true';

  console.log('Setting up task filters (improved open/close animation)...');

  // Помощник: после завершения transition по max-height синхронизируем inline-style и hidden
  panel.addEventListener('transitionend', (e) => {
    if (e.propertyName !== 'max-height') return;

    // если панель видима — убираем inline maxHeight чтобы позволить контенту менять высоту
    if (panel.classList.contains('visible')) {
      panel.style.maxHeight = '';
    } else {
      // если панель скрыта — ставим hidden и очистим inline-стили
      panel.classList.add('hidden');
      panel.style.maxHeight = '';
    }
  });

  const showPanel = () => {
    if (panel.classList.contains('visible')) return;

    // Убедимся, что hidden убран, добавим классы, затем анимируем maxHeight от 0 до scrollHeight
    panel.classList.remove('hidden');
    panel.classList.add('expanded', 'filter-panel-expanded');
    toggleBtn.setAttribute('aria-expanded', 'true');

    // Начальное значение — 0, затем в следующем кадре ставим реальную высоту.
    panel.style.maxHeight = '0px';
    // Нужен RAF чтобы браузер успел применить 0px перед переходом к реальной высоте
    requestAnimationFrame(() => {
      const h = panel.scrollHeight;
      panel.style.maxHeight = h + 'px';
      // включаем видимость (opacity/transform) — это даёт плавный fade+move
      panel.classList.add('visible');
    });

    console.log('Panel expanding (animated to content height)');
  };

  const hidePanel = () => {
    if (!panel.classList.contains('visible')) return;

    // Установим текущую фактическую высоту (чтобы transition сработал корректно),
    // затем в следующем кадре переведём в 0.
    panel.style.maxHeight = panel.scrollHeight + 'px';
    requestAnimationFrame(() => {
      panel.style.maxHeight = '0px';
      panel.classList.remove('visible');
      panel.classList.remove('filter-panel-expanded');
    });

    toggleBtn.setAttribute('aria-expanded', 'false');
    console.log('Panel collapsing (animated)');
  };

  // Toggle
  const toggleHandler = (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (panel.classList.contains('hidden') || !panel.classList.contains('visible')) {
      showPanel();
    } else {
      hidePanel();
    }
  };
  toggleBtn.addEventListener('click', toggleHandler);

  // Apply / Clear handlers (останавливаем propagation и закрываем панель после успешного выполнения)
  if (applyBtn) {
    applyBtn.addEventListener('click', async (e) => {
      e.preventDefault(); e.stopPropagation();
      console.log('[Filters] Apply clicked');
      const filters = getExpandableFilterValues();
      try {
        currentFilters = filters;
        applyFilters(); // Изменено: вместо loadTasks(filters)
        showToast('Filters applied', 'success');
        hidePanel();
        updateFilterCounter();
      } catch (error) {
        console.error('[Filters] Failed to apply filters:', error);
        showToast('Failed to apply filters', 'error');
      }
    });
  }

  if (clearBtn) {
    clearBtn.addEventListener('click', async (e) => {
      e.preventDefault(); e.stopPropagation();
      console.log('[Filters] Clear clicked');

      document.querySelectorAll('.filter-status').forEach(cb => cb.checked = false);
      document.querySelectorAll('.filter-priority').forEach(cb => cb.checked = false);

      const ids = ['filterDueFrom','filterDueTo','filterProject','filterMinEstimatedHours','filterMaxEstimatedHours','filterBillable','filterOverdue'];
      ids.forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        if (el.type === 'checkbox') el.checked = false;
        else el.value = '';
      });

      try {
        currentFilters = {};
        applyFilters(); // Изменено: вместо loadTasks({})
        showToast('Filters cleared', 'info');
        hidePanel();
        updateFilterCounter();
      } catch (error) {
        console.error('[Filters] Failed to clear filters:', error);
        showToast('Failed to clear filters', 'error');
      }
    });
  }

  // Закрытие при клике вне панели (только одна инициализация)
  if (!window._tasksFiltersOutsideClickInit) {
    window._tasksFiltersOutsideClickInit = true;
    document.addEventListener('click', (e) => {
      const tBtn = document.getElementById('toggleFiltersBtn');
      const pnl = document.getElementById('filtersPanel');
      if (!tBtn || !pnl) return;
      if (!tBtn.contains(e.target) && !pnl.contains(e.target)) {
        if (pnl.classList.contains('visible')) hidePanel();
      }
    }, true);
  }

  // Предотвращаем закрытие при клике внутри панели
  panel.addEventListener('click', (e) => e.stopPropagation());

  // Single-choice для статусов (чекбоксы ведут себя как радиокнопки)
  const statusEls = Array.from(document.querySelectorAll('.filter-status'));
  statusEls.forEach(el => {
    el.addEventListener('change', () => {
      if (el.checked) {
        statusEls.forEach(other => { if (other !== el) other.checked = false; });
      }
      updateFilterCounter();
    });
    el.addEventListener('keydown', (ev) => {
      if (ev.key === 'Enter' || ev.key === ' ') {
        ev.preventDefault();
        el.checked = !el.checked;
        el.dispatchEvent(new Event('change', { bubbles: true }));
      }
    });
  });

  // Остальные элементы: следим за изменениями для счетчика
  document.querySelectorAll('#filtersPanel input:not(.filter-status), #filtersPanel select').forEach(el => {
    el.addEventListener('change', updateFilterCounter);
    el.addEventListener('input', updateFilterCounter);
  });

  // Первичная синхронизация счетчика
  updateFilterCounter();
}

function updateFilterCounter() {
  const filters = getExpandableFilterValues();
  const activeFilters = Object.keys(filters).filter(key => filters[key]).length;
  const counter = document.getElementById('filterCounter');
  
  if (counter) {
    if (activeFilters > 0) {
      counter.textContent = activeFilters;
      counter.classList.remove('hidden');
    } else {
      counter.classList.add('hidden');
    }
  }
}

async function loadProjectsForFilter() {
  try {
    const response = await ProjectAPI.getProjects({ pageSize: 100 });
    allProjects = response.items || response || [];
    
    const projectFilter = document.getElementById('filterProject');
    if (projectFilter) {
      // Сохраняем текущее значение
      const currentValue = projectFilter.value;
      
      // Очищаем и добавляем опции
      projectFilter.innerHTML = '<option value="">All projects</option>';
      
      allProjects.forEach(project => {
        const option = document.createElement('option');
        option.value = project.id;
        option.textContent = project.title || 'Untitled Project';
        projectFilter.appendChild(option);
      });
      
      // Восстанавливаем значение, если оно есть
      if (currentValue) {
        projectFilter.value = currentValue;
      }
    }
  } catch (error) {
    console.error('Failed to load projects for filter:', error);
  }
}

function getExpandableFilterValues() {
  const filters = {};

  // Status Filters
  const statuses = Array.from(document.querySelectorAll('.filter-status'))
    .filter(cb => cb.checked)
    .map(cb => cb.value);

  if (statuses.length) {
    filters.status = statuses.join(',');
  }

  // Priority Filters
  const priorities = Array.from(document.querySelectorAll('.filter-priority'))
    .filter(cb => cb.checked)
    .map(cb => cb.value);

  if (priorities.length) {
    filters.priority = priorities.join(',');
  }

  // Due Date Range
  const dueFromEl = document.getElementById('filterDueFrom');
  const dueToEl = document.getElementById('filterDueTo');
  const dueFrom = dueFromEl ? dueFromEl.value : '';
  const dueTo = dueToEl ? dueToEl.value : '';
  if (dueFrom) filters.dueFrom = dueFrom;
  if (dueTo) filters.dueTo = dueTo;

  // Project filter
  const projectEl = document.getElementById('filterProject');
  const project = projectEl ? projectEl.value : '';
  if (project) filters.projectId = project;

  // Estimated Hours Range
  const minEstimatedHoursEl = document.getElementById('filterMinEstimatedHours');
  const maxEstimatedHoursEl = document.getElementById('filterMaxEstimatedHours');
  const minEstimatedHours = minEstimatedHoursEl ? minEstimatedHoursEl.value : '';
  const maxEstimatedHours = maxEstimatedHoursEl ? maxEstimatedHoursEl.value : '';
  if (minEstimatedHours) filters.minEstimatedHours = minEstimatedHours;
  if (maxEstimatedHours) filters.maxEstimatedHours = maxEstimatedHours;

  // Billable filter
  const billableEl = document.getElementById('filterBillable');
  const billable = billableEl ? billableEl.value : '';
  if (billable) filters.isBillable = billable;

  // Overdue filter
  const overdueEl = document.getElementById('filterOverdue');
  if (overdueEl && overdueEl.checked) {
    filters.overdue = true;
  }

  return filters;
}

function createTaskElement(task) {
    const element = document.createElement('div');
    element.className = 'bg-white rounded-lg border border-gray-200 p-4 cursor-pointer hover:shadow-md transition-shadow';
    element.dataset.id = task.id;
    
    const daysLeftNumber = getDaysLeftNumber(task.dueDate);
    const isOverdue = daysLeftNumber !== null && daysLeftNumber < 0;
    const isDueSoon = daysLeftNumber !== null && daysLeftNumber >= 0 && daysLeftNumber < 3;
    
    element.innerHTML = `
        <div class="flex justify-between items-start mb-2">
            <h4 class="font-medium text-gray-900 text-sm">${task.title || 'No title'}</h4>
            <span class="px-2 py-1 text-xs font-medium rounded-full ${getPriorityClass(task.priority)}">
                ${getPriorityText(task.priority)}
            </span>
        </div>
        <p class="text-xs text-gray-500 mb-3">${task.description || 'No description'}</p>
        <div class="flex items-center justify-between text-xs text-gray-500">
            <div>
                <i class="far fa-calendar-alt mr-1"></i> 
                ${task.dueDate ? formatDate(task.dueDate) : 'No due date'}
            </div>
            <div class="${isOverdue || isDueSoon ? 'text-red-500' : 'text-gray-500'}">
                <i class="far fa-clock mr-1"></i> 
                ${getDaysLeft(task.dueDate)}
            </div>
        </div>
        ${task.assigneeEmail ? `
        <div class="mt-2 flex items-center">
            <div class="w-6 h-6 rounded-full bg-gray-300 flex items-center justify-center overflow-hidden mr-2">
                <img src="https://ui-avatars.com/api/?name=${task.assigneeEmail}&background=random" alt="${task.assigneeEmail}">
            </div>
            <span class="text-xs text-gray-500">${task.assigneeEmail}</span>
        </div>
        ` : ''}
    `;
    
    element.addEventListener('click', () => openTaskModal(task.id));
    return element;
}


async function openTaskModal(taskId) {
    try {
        console.log('Opening task modal for task ID:', taskId);
        
        // Загружаем основную информацию о задаче
        const task = await TaskAPI.getTaskById(taskId, ['timeEntries']);
        console.log('Task data received:', task);
        
        // Если задача имеет назначенного пользователя, загружаем его данные
        if (task.assigneeId) {
            try {
                // Загружаем участников проекта для получения информации о назначенном пользователе
                const members = await TaskAPI.getProjectMembers(task.projectId);
                let membersArray = [];
                
                if (Array.isArray(members)) {
                    membersArray = members;
                } else if (members && Array.isArray(members.items)) {
                    membersArray = members.items;
                } else if (members && Array.isArray(members.value)) {
                    membersArray = members.value;
                }
                
                // Находим назначенного пользователя - ИСПРАВЛЕНИЕ: сравниваем с user.id
                const assignedMember = membersArray.find(member => {
                    // Пропускаем участников без объекта user
                    if (!member.user) return false;
                    
                    // ИСПРАВЛЕНИЕ: Сравниваем с member.user.id вместо member.userId
                    const memberUserId = member.user.id;
                    return memberUserId === task.assigneeId;
                });
                
                if (assignedMember) {
                    const user = assignedMember.user;
                    task.assigneeName = user.name || user.userName || user.email || `User ${task.assigneeId}`;
                } else {
                    task.assigneeName = `User ${task.assigneeId}`;
                }
            } catch (error) {
                console.error('Failed to load assignee info:', error);
                task.assigneeName = `User ${task.assigneeId}`;
            }
        }
        
        // Загружаем комментарии через отдельный endpoint
        const comments = await TaskAPI.getComments(taskId);
        console.log('Comments with author data:', comments);
        
        // Добавляем комментарии к задаче
        task.comments = comments || [];
        
        renderTaskModal(task);
    } catch (error) {
        console.error('Failed to load task details:', error);
        showToast('Failed to load task details', 'error');
    }
}


function renderTaskModal(task) {
    const modal = document.getElementById('taskModal');
    if (!modal) {
        console.error('Task modal element not found');
        return;
    }
    
    console.log('Rendering task modal with data:', task);
    
    // Убедимся, что свойства существуют
    if (!task.timeEntries) task.timeEntries = [];
    if (!task.comments) task.comments = [];
    
    // Форматируем время для time entries
    const formatTimeEntryDuration = (duration) => {
        if (!duration) return '0h';
        try {
            // Парсим строку формата "-04:00:00" или объекта с тиками
            if (typeof duration === 'string') {
                const [hours, minutes, seconds] = duration.substring(1).split(':').map(Number);
                return `${hours}.${Math.round(minutes/60*10)}h`;
            } else if (typeof duration === 'number') {
                // Если duration в тиках, конвертируем в часы
                return (duration / 36000000000).toFixed(1) + 'h';
            }
            return '0h';
        } catch {
            return '0h';
        }
    };

    // Форматируем даты для time entries
    const formatTimeEntryDate = (dateString) => {
        if (!dateString) return 'Unknown date';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString();
        } catch {
            return 'Invalid date';
        }
    };

    modal.innerHTML = `
        <div class="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-screen overflow-y-auto">
            <div class="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
                <h3 class="text-xl font-semibold text-gray-900">Task Details</h3>
                <button class="close-task-modal text-gray-400 hover:text-gray-500">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            
            <div class="p-6">
                <div class="mb-6">
                    <h4 class="text-lg font-semibold text-gray-900 mb-2">${task.title || 'No title'}</h4>
                    <p class="text-gray-700">${task.description || 'No description'}</p>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                    <div>
                        <h5 class="font-medium text-gray-900 mb-3">Details</h5>
                        <div class="space-y-2">
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Status:</span>
                                <span class="text-sm font-medium ${getTaskStatusClass(task.status)} px-2 py-1 rounded-full">
                                    ${getTaskStatusText(task.status)}
                                </span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Priority:</span>
                                <span class="text-sm font-medium ${getPriorityClass(task.priority)} px-2 py-1 rounded-full">
                                    ${getPriorityText(task.priority)}
                                </span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Due Date:</span>
                                <span class="text-sm font-medium">${task.dueDate ? formatDate(task.dueDate) : 'Not set'}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Estimated Hours:</span>
                                <span class="text-sm font-medium">${task.timeEstimatedTicks ? (task.timeEstimatedTicks / 36000000000).toFixed(1) : 'Not set'}</span>
                            </div>
                        </div>
                    </div>
                    
                    <div>
                        <h5 class="font-medium text-gray-900 mb-3">People</h5>
                        <div class="space-y-2">
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Assignee:</span>
                                <span class="text-sm font-medium">${task.assigneeName || 'Unassigned'}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Reporter:</span>
                                <span class="text-sm font-medium">${task.reporterName || 'Unknown'}</span>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="mb-6">
                    <h5 class="font-medium text-gray-900 mb-3">Actions</h5>
                    <div class="flex flex-wrap gap-2">
                        ${task.status === 0 ? `
                            <button class="assign-task-btn px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700">
                                <i class="fas fa-user-plus mr-2"></i> Assign
                            </button>
                            <button class="start-task-btn px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-md hover:bg-green-700">
                                <i class="fas fa-play mr-2"></i> Start
                            </button>
                        ` : ''}
                        ${task.status === 1 ? `
                            <button class="complete-task-btn px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-md hover:bg-green-700">
                                <i class="fas fa-check-circle mr-2"></i> Complete
                            </button>
                        ` : ''}
                        ${task.status !== 3 ? `
                            <button class="cancel-task-btn px-4 py-2 bg-red-100 text-red-600 text-sm font-medium rounded-md hover:bg-red-200">
                                <i class="fas fa-ban mr-2"></i> Cancel
                            </button>
                        ` : ''}
                        <button class="edit-task-btn px-4 py-2 bg-gray-600 text-white text-sm font-medium rounded-md hover:bg-gray-700">
                            <i class="fas fa-edit mr-2"></i> Edit
                        </button>
                        <button class="delete-task-btn px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-md hover:bg-red-700">
                            <i class="fas fa-trash-alt mr-2"></i> Delete
                        </button>
                    </div>
                </div>
                
                <div class="mb-6">
                    <h5 class="font-medium text-gray-900 mb-3">Time Entries</h5>
                    <div class="space-y-2" id="timeEntriesContainer">
                        ${task.timeEntries && task.timeEntries.length > 0 ? 
                            task.timeEntries.map(entry => `
                                <div class="flex justify-between items-center p-2 bg-gray-50 rounded">
                                    <div>
                                        <span class="text-sm font-medium">${formatTimeEntryDate(entry.startedAt)}</span>
                                        <span class="text-xs text-gray-500 ml-2">${entry.description || 'No description'}</span>
                                    </div>
                                    <span class="text-sm font-medium">${formatTimeEntryDuration(entry.duration)}</span>
                                </div>
                            `).join('') : 
                            '<p class="text-sm text-gray-500">No time entries</p>'
                        }
                    </div>
                    <button class="add-time-entry-btn mt-3 px-3 py-1 bg-blue-100 text-blue-600 text-sm rounded-md hover:bg-blue-200">
                        <i class="fas fa-plus mr-1"></i> Add Time Entry
                    </button>
                </div>
                
                <div>
                    <h5 class="font-medium text-gray-900 mb-3">Comments</h5>
                    <div class="space-y-4" id="commentsContainer">
                        ${task.comments && task.comments.length > 0 ? 
                            task.comments.map(comment => `
                                <div class="flex">
                                    <div class="flex-shrink-0 mr-3">
                                        <img class="h-8 w-8 rounded-full" src="https://ui-avatars.com/api/?name=${comment.author?.name || comment.author?.login || 'Unknown'}&background=random" alt="${comment.author?.name || 'Author'}">
                                    </div>
                                    <div class="bg-gray-50 p-3 rounded-lg flex-1">
                                        <div class="flex justify-between">
                                            <span class="text-sm font-medium">${comment.author?.name || comment.author?.login || 'Unknown User'}</span>
                                            <span class="text-xs text-gray-500">${formatDate(comment.createdAt)}</span>
                                        </div>
                                        <p class="text-sm mt-1">${comment.text}</p>
                                    </div>
                                </div>
                            `).join('') : 
                            '<p class="text-sm text-gray-500">No comments</p>'
                        }
                    </div>
                    
                    <div class="mt-4 flex">
                        <input type="text" placeholder="Add a comment..." class="comment-input flex-1 px-3 py-2 border border-gray-300 rounded-l-md focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <button class="add-comment-btn px-4 py-2 bg-blue-600 text-white rounded-r-md hover:bg-blue-700">
                            <i class="fas fa-paper-plane"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Добавляем обработчики событий
    setupTaskModalEventListeners(task);
    modal.classList.remove('hidden');
}

function setupTaskEventListeners() {
    const newTaskBtn = document.getElementById('newTaskBtn');
    if (newTaskBtn) {
        newTaskBtn.addEventListener('click', showCreateTaskModal);
    }
}

function setupTaskFilters() {
  // Добавляем обработчики для фильтров
  const searchFilter = document.getElementById('searchFilter');
  if (searchFilter) {
    // Добавляем обработчик для поиска с задержкой
    let searchTimeout;
    searchFilter.addEventListener('input', (e) => {
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => {
        applyFilters();
      }, 500);
    });
  }
}

function applyFilters() {
  console.log('Applying filters with ownership:', taskOwnershipFilter);
  
  // Копируем текущие фильтры
  let filters = { ...currentFilters };
  
  // Добавляем фильтры владения на основе taskOwnershipFilter
  if (currentUserId) {
    // Всегда добавляем CurrentUserId, если он доступен
    filters.CurrentUserId = currentUserId;
    
    switch (taskOwnershipFilter) {
      case 'all':
        // Показываем все свои задачи (assigned OR reported)
        filters.OnlyMyTasks = true;
        delete filters.AssigneeId;
        delete filters.ReporterId;
        break;
        
      case 'assigned':
        // Показываем только назначенные на текущего пользователя
        filters.OnlyMyTasks = false;
        filters.AssigneeId = currentUserId;
        delete filters.ReporterId;
        break;
        
      case 'reported':
        // Показываем только созданные текущим пользователем
        filters.OnlyMyTasks = false;
        filters.ReporterId = currentUserId;
        delete filters.AssigneeId;
        break;
    }
  } else {
    console.warn('No currentUserId available - ownership filters may not work');
  }
  
  // Загружаем задачи с обновленными фильтрами
  loadTasks(filters);
}

function setupTaskModalEventListeners(task) {
    const modal = document.getElementById('taskModal');
    if (!modal) return;
    
    // Закрытие модального окна
    const closeBtn = modal.querySelector('.close-task-modal');
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            modal.classList.add('hidden');
        });
    }
    
    // Обработчики для кнопок действий
    const actionButtons = [
        { selector: '.assign-task-btn', handler: () => showAssignTaskModal(task.id) },
        { selector: '.start-task-btn', handler: () => startTask(task.id) },
        { selector: '.complete-task-btn', handler: () => completeTask(task.id) },
        { selector: '.cancel-task-btn', handler: () => showCancelTaskModal(task.id) },
        { selector: '.edit-task-btn', handler: () => showEditTaskModal(task) },
        { selector: '.delete-task-btn', handler: () => deleteTask(task.id) },
        { selector: '.add-time-entry-btn', handler: () => showAddTimeEntryModal(task.id) },
        { selector: '.add-comment-btn', handler: () => addCommentToTask(task.id, modal) }
    ];
    
    actionButtons.forEach(({ selector, handler }) => {
        const button = modal.querySelector(selector);
        if (button) {
            button.addEventListener('click', handler);
        }
    });
}

async function startTask(taskId) {
    try {
        await TaskAPI.startTask(taskId);
        showToast('Task started successfully');
        await loadTasks(currentFilters);
        document.getElementById('taskModal').classList.add('hidden');
    } catch (error) {
        console.error('Failed to start task:', error);
        showToast('Failed to start task', 'error');
    }
}

async function completeTask(taskId) {
    try {
        await TaskAPI.completeTask(taskId);
        showToast('Task completed successfully');
        await loadTasks(currentFilters);
        document.getElementById('taskModal').classList.add('hidden');
    } catch (error) {
        console.error('Failed to complete task:', error);
        showToast('Failed to complete task', 'error');
    }
}

async function deleteTask(taskId) {
    if (!confirm('Are you sure you want to delete this task?')) return;
    
    try {
        await TaskAPI.deleteTask(taskId);
        showToast('Task deleted successfully');
        await loadTasks(currentFilters);
        document.getElementById('taskModal').classList.add('hidden');
    } catch (error) {
        console.error('Failed to delete task:', error);
        showToast('Failed to delete task', 'error');
    }
}

async function addCommentToTask(taskId, modal) {
    const commentInput = modal.querySelector('.comment-input');
    const commentText = commentInput.value.trim();
    
    if (!commentText) return;
    
    try {
        const user = await UserAPI.getProfile();
        await TaskAPI.addComment(taskId, {
            authorId: user.id,
            text: commentText
        });
        
        showToast('Comment added successfully');
        commentInput.value = '';
        
        // Перезагружаем комментарии после добавления
        const comments = await TaskAPI.getComments(taskId);
        
        // Находим контейнер комментариев и обновляем его
        const commentsContainer = modal.querySelector('#commentsContainer');
        
        if (comments && comments.length > 0) {
            // Очищаем контейнер и добавляем все комментарии
            commentsContainer.innerHTML = comments.map(comment => `
                <div class="flex">
                    <div class="flex-shrink-0 mr-3">
                        <img class="h-8 w-8 rounded-full" src="https://ui-avatars.com/api/?name=${comment.author?.name || comment.author?.login || 'Unknown'}&background=random" alt="${comment.author?.name || 'Author'}">
                    </div>
                    <div class="bg-gray-50 p-3 rounded-lg flex-1">
                        <div class="flex justify-between">
                            <span class="text-sm font-medium">${comment.author?.name || comment.author?.login || 'Unknown User'}</span>
                            <span class="text-xs text-gray-500">${formatDate(comment.createdAt)}</span>
                        </div>
                        <p class="text-sm mt-1">${comment.text}</p>
                    </div>
                </div>
            `).join('');
        } else {
            commentsContainer.innerHTML = '<p class="text-sm text-gray-500">No comments</p>';
        }
    } catch (error) {
        console.error('Failed to add comment:', error);
        showToast('Failed to add comment', 'error');
    }
}

function updateTaskCounters() {
    try {
        const totalTasks = currentTasks.length;
        const inProgressTasks = currentTasks.filter(task => task.status === 1).length;
        
        // Считаем задачи, которые скоро должны быть выполнены (менее 3 дней)
        const dueSoonTasks = currentTasks.filter(task => {
            const daysLeft = getDaysLeftNumber(task.dueDate);
            return daysLeft !== null && daysLeft >= 0 && daysLeft < 3;
        }).length;
        
        // Считаем просроченные задачи
        const overdueTasks = currentTasks.filter(task => {
            const daysLeft = getDaysLeftNumber(task.dueDate);
            return daysLeft !== null && daysLeft < 0;
        }).length;
        
        // Безопасное обновление счетчиков
        const updateElement = (id, value) => {
            const element = document.getElementById(id);
            if (element) element.textContent = value;
        };
        
        updateElement('totalTasksCount', totalTasks);
        updateElement('inProgressTasksCount', inProgressTasks);
        updateElement('dueSoonTasksCount', dueSoonTasks);
        updateElement('overdueTasksCount', overdueTasks);
        
        // Обновляем счетчик в подвале таблицы
        updateElement('tasksShowing', totalTasks);
        updateElement('tasksTotal', totalTasks);
        
    } catch (error) {
        console.error('Error updating task counters:', error);
    }
}

// Функции для модальных окон
async function showCreateTaskModal() {
    try {
        // Получаем проекты с обработкой пагинации
        const projectsResponse = await ProjectAPI.getProjects();
        const projects = projectsResponse.items || projectsResponse;
        
        // Проверяем, что projects - массив
        if (!Array.isArray(projects)) {
            console.error('Expected projects to be an array, got:', projects);
            showToast('Failed to load projects', 'error');
            return;
        }
        
        // Создаем модальное окно
        const modal = document.createElement('div');
        modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
        
        // Загружаем HTML формы задачи
        let formHtml;
        try {
            formHtml = await fetch('/partials/task-form.html').then(r => r.text());
        } catch (error) {
            console.error('Failed to load task form HTML:', error);
            formHtml = `
                <div class="bg-white rounded-lg shadow-lg p-6 max-w-3xl mx-auto">
                    <h2 class="text-2xl font-semibold text-gray-900 mb-6">Create New Task</h2>
                    <p class="text-red-500">Failed to load task form. Please refresh the page.</p>
                    <div class="flex justify-end mt-4">
                        <button class="cancel-btn px-4 py-2 bg-gray-200 text-gray-700 rounded-md">Cancel</button>
                    </div>
                </div>
            `;
        }
        
        modal.innerHTML = `<div class="modal-container">${formHtml}</div>`;
        document.body.appendChild(modal);
        
        // Заполняем выпадающий список проектов
        const projectSelect = modal.querySelector('#taskProject');
        if (projectSelect) {
            projects.forEach(project => {
                const option = document.createElement('option');
                option.value = project.id;
                option.textContent = project.title;
                projectSelect.appendChild(option);
            });
        }
        
        // Делаем список исполнителей неактивным до выбора проекта
        const assigneeSelect = modal.querySelector('#taskAssignee');
        if (assigneeSelect) {
            assigneeSelect.disabled = true;
        }
        
        // Обработчик изменения проекта
        if (projectSelect) {
            projectSelect.addEventListener('change', async (e) => {
                const projectId = e.target.value;
                const assigneeSelect = modal.querySelector('#taskAssignee');
                
                if (!projectId) {
                    if (assigneeSelect) {
                        assigneeSelect.disabled = true;
                        assigneeSelect.innerHTML = '<option value="">Select project first</option>';
                    }
                    return;
                }
                
                try {
                    // Загружаем участников проекта через правильный endpoint
                    const members = await ProjectAPI.getProjectMembers(projectId);
                    console.log('Members response:', members);
                    
                    if (assigneeSelect) {
                        assigneeSelect.disabled = false;
                        assigneeSelect.innerHTML = '';
                        
                        // Добавляем опцию "Unassigned"
                        const unassignedOption = document.createElement('option');
                        unassignedOption.value = '';
                        unassignedOption.textContent = 'Unassigned';
                        assigneeSelect.appendChild(unassignedOption);
                        
                        // Обрабатываем разные форматы ответа
                        let membersArray = [];
                        if (Array.isArray(members)) {
                            membersArray = members;
                        } else if (members && Array.isArray(members.items)) {
                            membersArray = members.items;
                        } else if (members && Array.isArray(members.value)) {
                            membersArray = members.value;
                        }
                        
                        // Добавляем участников проекта
                        if (membersArray.length > 0) {
                            membersArray.forEach(member => {
                                const option = document.createElement('option');
                                
                                // Получаем ID пользователя из разных возможных форматов
                                const memberId = member.userId || member.id || member.userID;
                                option.value = memberId;
                                
                                // Получаем информацию о пользователе
                                const user = member.user || member;
                                const userName = user.name || user.userName || user.fullName;
                                const userEmail = user.email || user.userEmail;
                                
                                // Формируем текст для отображения
                                let displayText = '';
                                if (userName && userEmail) {
                                    displayText = `${userName} (${userEmail})`;
                                } else if (userName) {
                                    displayText = userName;
                                } else if (userEmail) {
                                    displayText = userEmail;
                                } else {
                                    displayText = `User ${memberId}`;
                                }
                                
                                option.textContent = displayText;
                                assigneeSelect.appendChild(option);
                            });
                        } else {
                            const noMembersOption = document.createElement('option');
                            noMembersOption.value = '';
                            noMembersOption.textContent = 'No members in this project';
                            noMembersOption.disabled = true;
                            assigneeSelect.appendChild(noMembersOption);
                        }
                    }
                } catch (error) {
                    console.error('Failed to load project members:', error);
                    if (assigneeSelect) {
                        assigneeSelect.disabled = true;
                        assigneeSelect.innerHTML = '<option value="">Failed to load members</option>';
                    }
                    showToast('Failed to load project members', 'error');
                }
            });
        }
        
        // Инициализируем datepicker
        if (window.flatpickr && modal.querySelector('#taskDueDate')) {
            flatpickr(modal.querySelector('#taskDueDate'), {
                dateFormat: 'Y-m-d',
                minDate: 'today'
            });
        }
        
        // Обработчики событий
        const cancelBtn = modal.querySelector('.cancel-btn');
        if (cancelBtn) {
            cancelBtn.addEventListener('click', () => {
                document.body.removeChild(modal);
            });
        }
        
        const taskForm = modal.querySelector('#taskForm');
        if (taskForm) {
            taskForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                await createTaskFromForm(modal);
            });
        }
        
    } catch (error) {
        console.error('Failed to show create task modal:', error);
        showToast('Failed to load task form', 'error');
    }
}

async function createTaskFromForm(modal) {
    try {
        const formData = new FormData(modal.querySelector('#taskForm'));
        
        // Получаем projectId
        const projectId = formData.get('projectId');
        if (!projectId) {
            showToast('Please select a project', 'error');
            return;
        }

        // Форматируем данные согласно CreateProjectTaskRequest
        const taskData = {
            projectId: projectId, // Исправлено на projectId (с маленькой "d")
            title: formData.get('title'),
            description: formData.get('description'),
            estimateValue: parseFloat(formData.get('estimatedHours')) || 0,
            estimateUnit: 0, // 0 = Hours (соответствует EstimateUnit в DTO)
            dueDate: formData.get('dueDate') ? new Date(formData.get('dueDate')).toISOString() : null,
            isBillable: formData.get('isBillable') === 'on',
            amount: parseFloat(formData.get('amount')) || 0,
            currency: formData.get('currency') || 'USD',
            priority: parseInt(formData.get('priority')) || 1
        };

        console.log('Sending task data:', taskData);
        
        await TaskAPI.createTask(taskData);
        showToast('Task created successfully');
        document.body.removeChild(modal);
        await loadTasks(currentFilters);
    } catch (error) {
        console.error('Failed to create task:', error);
        // Более конкретное сообщение об ошибке
        if (error.message.includes('Project not found')) {
            showToast('Selected project was not found. Please choose another project.', 'error');
        } else {
            showToast('Failed to create task', 'error');
        }
    }
}

async function showEditTaskModal(task) {
    try {
        // Получаем проекты с обработкой пагинации
        const projectsResponse = await ProjectAPI.getProjects();
        const projects = projectsResponse.items || projectsResponse;
        
        // Проверяем, что projects - массив
        if (!Array.isArray(projects)) {
            console.error('Expected projects to be an array, got:', projects);
            showToast('Failed to load projects', 'error');
            return;
        }
        
        // Создаем модальное окно
        const modal = document.createElement('div');
        modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
        
        // Загружаем HTML формы задачи
        let formHtml;
        try {
            formHtml = await fetch('/partials/task-form.html').then(r => r.text());
        } catch (error) {
            console.error('Failed to load task form HTML:', error);
            formHtml = `
                <div class="bg-white rounded-lg shadow-lg p-6 max-w-3xl mx-auto">
                    <h2 class="text-2xl font-semibold text-gray-900 mb-6">Edit Task</h2>
                    <p class="text-red-500">Failed to load task form. Please refresh the page.</p>
                    <div class="flex justify-end mt-4">
                        <button class="cancel-btn px-4 py-2 bg-gray-200 text-gray-700 rounded-md">Cancel</button>
                    </div>
                </div>
            `;
        }
        
        modal.innerHTML = `<div class="modal-container">${formHtml}</div>`;
        document.body.appendChild(modal);
        
        // Заполняем форму данными задачи
        modal.querySelector('#taskFormTitle').textContent = 'Edit Task';
        modal.querySelector('#submitButtonText').textContent = 'Update Task';
        modal.querySelector('#taskId').value = task.id;
        modal.querySelector('#taskTitle').value = task.title || '';
        modal.querySelector('#taskDescription').value = task.description || '';
        
        // Заполняем выпадающий список проектов
        const projectSelect = modal.querySelector('#taskProject');
        projects.forEach(project => {
            const option = document.createElement('option');
            option.value = project.id;
            option.textContent = project.title;
            option.selected = project.id === task.projectId;
            projectSelect.appendChild(option);
        });
        
        // Загружаем участников выбранного проекта
        const assigneeSelect = modal.querySelector('#taskAssignee');
        assigneeSelect.disabled = true;
        const loadingOption = document.createElement('option');
        loadingOption.value = '';
        loadingOption.textContent = 'Loading members...';
        assigneeSelect.appendChild(loadingOption);
        
        if (task.projectId) {
            try {
                // Загружаем участников проекта через правильный endpoint
                const members = await ProjectAPI.getProjectMembers(task.projectId);
                console.log('Members response:', members);
                
                if (assigneeSelect) {
                    assigneeSelect.disabled = false;
                    assigneeSelect.innerHTML = '';
                    
                    // Добавляем опцию "Unassigned"
                    const unassignedOption = document.createElement('option');
                    unassignedOption.value = '';
                    unassignedOption.textContent = 'Unassigned';
                    unassignedOption.selected = !task.assigneeId;
                    assigneeSelect.appendChild(unassignedOption);
                    
                    // Обрабатываем разные форматы ответа
                    let membersArray = [];
                    if (Array.isArray(members)) {
                        membersArray = members;
                    } else if (members && Array.isArray(members.items)) {
                        membersArray = members.items;
                    } else if (members && Array.isArray(members.value)) {
                        membersArray = members.value;
                    }
                    
                    // Добавляем участников проекта
                    if (membersArray.length > 0) {
                        membersArray.forEach(member => {
                            const option = document.createElement('option');
                            
                            // Получаем ID пользователя из разных возможных форматов
                            const memberId = member.userId || member.id || member.userID;
                            option.value = memberId;
                            
                            // Получаем информацию о пользователе
                            const user = member.user || member;
                            const userName = user.name || user.userName || user.fullName;
                            const userEmail = user.email || user.userEmail;
                            
                            // Формируем текст для отображения
                            let displayText = '';
                            if (userName && userEmail) {
                                displayText = `${userName} (${userEmail})`;
                            } else if (userName) {
                                displayText = userName;
                            } else if (userEmail) {
                                displayText = userEmail;
                            } else {
                                displayText = `User ${memberId}`;
                            }
                            
                            option.textContent = displayText;
                            option.selected = memberId === task.assigneeId;
                            assigneeSelect.appendChild(option);
                        });
                    } else {
                        const noMembersOption = document.createElement('option');
                        noMembersOption.value = '';
                        noMembersOption.textContent = 'No members in this project';
                        noMembersOption.disabled = true;
                        assigneeSelect.appendChild(noMembersOption);
                    }
                }
            } catch (error) {
                console.error('Failed to load project members:', error);
                if (assigneeSelect) {
                    assigneeSelect.disabled = true;
                    assigneeSelect.innerHTML = '<option value="">Failed to load members</option>';
                }
            }
        }
        
        // Обработчик изменения проекта
        projectSelect.addEventListener('change', async (e) => {
            const projectId = e.target.value;
            
            if (!projectId) {
                assigneeSelect.disabled = true;
                assigneeSelect.innerHTML = '';
                const noProjectOption = document.createElement('option');
                noProjectOption.value = '';
                noProjectOption.textContent = 'Select project first';
                assigneeSelect.appendChild(noProjectOption);
                return;
            }
            
            try {
                // Загружаем участников проекта через правильный endpoint
                const members = await ProjectAPI.getProjectMembers(projectId);
                console.log('Members response:', members);
                
                assigneeSelect.disabled = false;
                assigneeSelect.innerHTML = '';
                
                // Добавляем опцию "Unassigned"
                const unassignedOption = document.createElement('option');
                unassignedOption.value = '';
                unassignedOption.textContent = 'Unassigned';
                assigneeSelect.appendChild(unassignedOption);
                
                // Обрабатываем разные форматы ответа
                let membersArray = [];
                if (Array.isArray(members)) {
                    membersArray = members;
                } else if (members && Array.isArray(members.items)) {
                    membersArray = members.items;
                } else if (members && Array.isArray(members.value)) {
                    membersArray = members.value;
                }
                
                // Добавляем участников проекта
                if (membersArray.length > 0) {
                    membersArray.forEach(member => {
                        const option = document.createElement('option');
                        
                        // Получаем ID пользователя из разных возможных форматов
                        const memberId = member.userId || member.id || member.userID;
                        option.value = memberId;
                        
                        // Получаем информацию о пользователе
                        const user = member.user || member;
                        const userName = user.name || user.userName || user.fullName;
                        const userEmail = user.email || user.userEmail;
                        
                        // Формируем текст для отображения
                        let displayText = '';
                        if (userName && userEmail) {
                            displayText = `${userName} (${userEmail})`;
                        } else if (userName) {
                            displayText = userName;
                        } else if (userEmail) {
                            displayText = userEmail;
                        } else {
                            displayText = `User ${memberId}`;
                        }
                        
                        option.textContent = displayText;
                        assigneeSelect.appendChild(option);
                    });
                } else {
                    const noMembersOption = document.createElement('option');
                    noMembersOption.value = '';
                    noMembersOption.textContent = 'No members in this project';
                    noMembersOption.disabled = true;
                    assigneeSelect.appendChild(noMembersOption);
                }
            } catch (error) {
                console.error('Failed to load project members:', error);
                assigneeSelect.disabled = true;
                assigneeSelect.innerHTML = '';
                const errorOption = document.createElement('option');
                errorOption.value = '';
                errorOption.textContent = 'Failed to load members';
                errorOption.disabled = true;
                assigneeSelect.appendChild(errorOption);
                showToast('Failed to load project members', 'error');
            }
        });
        
        // Заполняем остальные поля
        if (task.dueDate) {
            modal.querySelector('#taskDueDate').value = task.dueDate.split('T')[0];
        }
        
        modal.querySelector('#taskPriority').value = task.priority || 1;
        modal.querySelector('#taskEstimatedHours').value = task.timeEstimatedTicks ? (task.timeEstimatedTicks / 36000000000) : '';
        modal.querySelector('#taskBillable').checked = task.isBillable || false;
        
        if (task.isBillable) {
            modal.querySelector('#billingFields').classList.remove('hidden');
            modal.querySelector('#taskAmount').value = task.amount || '';
            modal.querySelector('#taskCurrency').value = task.currency || 'USD';
        }
        
        // Инициализируем datepicker
        if (window.flatpickr) {
            flatpickr(modal.querySelector('#taskDueDate'), {
                dateFormat: 'Y-m-d',
                minDate: 'today'
            });
        }
        
        // Обработчики событий
        modal.querySelector('.cancel-btn').addEventListener('click', () => {
            document.body.removeChild(modal);
        });
        
        modal.querySelector('#taskForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            await updateTaskFromForm(modal, task.id);
        });
        
    } catch (error) {
        console.error('Failed to show edit task modal:', error);
        showToast('Failed to load task form', 'error');
    }
}

async function updateTaskFromForm(modal, taskId) {
    try {
        const formData = new FormData(modal.querySelector('#taskForm'));
        
        // Форматируем данные согласно UpdateProjectTaskRequest
        const taskData = {
            title: formData.get('title'),
            description: formData.get('description'),
            estimateValue: parseFloat(formData.get('estimatedHours')) || 0,
            estimateUnit: 0, // 0 = Hours
            dueDate: formData.get('dueDate') ? new Date(formData.get('dueDate')).toISOString() : null,
            isBillable: formData.get('isBillable') === 'on',
            amount: parseFloat(formData.get('amount')) || 0,
            currency: formData.get('currency') || 'USD',
            priority: parseInt(formData.get('priority')) || 1
        };

        // Убираем пустые поля
        Object.keys(taskData).forEach(key => {
            if (taskData[key] === null || taskData[key] === undefined || taskData[key] === '') {
                delete taskData[key];
            }
        });

        console.log('Updating task with data:', taskData);
        
        await TaskAPI.updateTask(taskId, taskData);
        showToast('Task updated successfully');
        document.body.removeChild(modal);
        await loadTasks(currentFilters);
        
        // Закрываем также детальное модальное окно если оно открыто
        const taskModal = document.getElementById('taskModal');
        if (taskModal) {
            taskModal.classList.add('hidden');
        }
    } catch (error) {
        console.error('Failed to update task:', error);
        showToast('Failed to update task', 'error');
    }
}

async function showAssignTaskModal(taskId) {
    try {
        const task = await TaskAPI.getTaskById(taskId);
        
        if (!task.projectId) {
            showToast('Task must be associated with a project first', 'error');
            return;
        }
        
        // Загружаем участников проекта
        const members = await TaskAPI.getProjectMembers(task.projectId);
        console.log('Project members:', members);
        
        const modal = document.createElement('div');
        modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
        
        // Загружаем HTML форму назначения
        let formHtml;
        try {
            formHtml = await fetch('/partials/assign-task-modal.html').then(r => r.text());
        } catch (error) {
            console.error('Failed to load assign form HTML:', error);
            formHtml = `
                <div class="bg-white rounded-lg shadow-lg p-6 max-w-md mx-auto">
                    <h2 class="text-xl font-semibold text-gray-900 mb-4">Assign Task</h2>
                    <p class="text-red-500">Failed to load assign form. Please refresh the page.</p>
                    <div class="flex justify-end space-x-3 mt-4">
                        <button class="cancel-btn px-4 py-2 bg-gray-200 text-gray-700 rounded-md">Cancel</button>
                    </div>
                </div>
            `;
        }
        
        modal.innerHTML = `<div class="modal-container">${formHtml}</div>`;
        document.body.appendChild(modal);
        
        // Заполняем выпадающий список участниками проекта
        const assigneeSelect = modal.querySelector('#assigneeSelect');
        
        // Добавляем опцию "Unassigned"
        const unassignedOption = document.createElement('option');
        unassignedOption.value = '';
        unassignedOption.textContent = 'Unassigned';
        unassignedOption.selected = !task.assigneeId;
        assigneeSelect.appendChild(unassignedOption);
        
        // Обрабатываем разные форматы ответа
        let membersArray = [];
        if (Array.isArray(members)) {
            membersArray = members;
        } else if (members && Array.isArray(members.items)) {
            membersArray = members.items;
        } else if (members && Array.isArray(members.value)) {
            membersArray = members.value;
        }
        
        console.log('Processed members array:', membersArray);
        
        // Добавляем участников проекта с отображением имени и email - ИСПРАВЛЕНИЕ: используем user.id
        if (membersArray.length > 0) {
            membersArray.forEach(member => {
                // Пропускаем участников без объекта user
                if (!member.user) {
                    console.warn('Member without user object found:', member);
                    return;
                }
                
                const option = document.createElement('option');
                
                // ИСПРАВЛЕНИЕ: Используем member.user.id вместо member.userId
                const userId = member.user.id;
                option.value = userId;
                
                // Получаем информацию о пользователе
                const user = member.user;
                const userName = user.name || user.userName || user.fullName;
                const userEmail = user.email || user.userEmail;
                
                // Формируем текст для отображения
                let displayText = '';
                if (userName && userEmail) {
                    displayText = `${userName} (${userEmail})`;
                } else if (userName) {
                    displayText = userName;
                } else if (userEmail) {
                    displayText = userEmail;
                } else {
                    displayText = `User ${userId}`;
                }
                
                option.textContent = displayText;
                option.selected = userId === task.assigneeId;
                assigneeSelect.appendChild(option);
            });
        } else {
            const noMembersOption = document.createElement('option');
            noMembersOption.value = '';
            noMembersOption.textContent = 'No members in this project';
            noMembersOption.disabled = true;
            assigneeSelect.appendChild(noMembersOption);
        }
        
        // Обработчики событий
        modal.querySelector('.cancel-btn').addEventListener('click', () => {
            document.body.removeChild(modal);
        });
        
        modal.querySelector('#confirmAssignBtn').addEventListener('click', async () => {
            const assigneeId = assigneeSelect.value;
            try {
                if (assigneeId) {
                    await TaskAPI.assignTask(taskId, assigneeId);
                    showToast('Task assigned successfully');
                } else {
                    await TaskAPI.unassignTask(taskId, task.assigneeId);
                    showToast('Task unassigned successfully');
                }
                
                document.body.removeChild(modal);
                await loadTasks(currentFilters);
                
                // Обновляем детальное модальное окно если оно открыто
                const taskModal = document.getElementById('taskModal');
                if (taskModal && !taskModal.classList.contains('hidden')) {
                    openTaskModal(taskId);
                }
            } catch (error) {
                console.error('Failed to assign task:', error);
                let errorMessage = 'Failed to assign task';
                
                if (error.message.includes('Assignee must be a project member')) {
                    errorMessage = 'Selected user must be a member of the project first';
                } else if (error.status === 400) {
                    errorMessage = 'Invalid request. User may not be a project member.';
                }
                
                showToast(errorMessage, 'error');
            }
        });
        
    } catch (error) {
        console.error('Failed to show assign task modal:', error);
        showToast('Failed to load assign form', 'error');
    }
}

async function showAddTimeEntryModal(taskId) {
    try {
        const modal = document.createElement('div');
        modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
        
        // Загружаем HTML форму записи времени
        let formHtml;
        try {
            formHtml = await fetch('/partials/time-entry-modal.html').then(r => r.text());
        } catch (error) {
            console.error('Failed to load time entry form HTML:', error);
            formHtml = `
                <div class="bg-white rounded-lg shadow-lg p-6 max-w-md mx-auto">
                    <h2 class="text-xl font-semibold text-gray-900 mb-4">Log Time</h2>
                    <p class="text-red-500">Failed to load time entry form. Please refresh the page.</p>
                    <div class="flex justify-end space-x-3 mt-4">
                        <button class="cancel-btn px-4 py-2 bg-gray-200 text-gray-700 rounded-md">Cancel</button>
                    </div>
                </div>
            `;
        }
        
        modal.innerHTML = `<div class="modal-container">${formHtml}</div>`;
        document.body.appendChild(modal);
        
        // Обработчики событий
        modal.querySelector('.cancel-btn').addEventListener('click', () => {
            document.body.removeChild(modal);
        });
        
        modal.querySelector('#timeEntryForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const date = modal.querySelector('#timeEntryDate').value;
            const startTime = modal.querySelector('#timeEntryStart').value;
            const endTime = modal.querySelector('#timeEntryEnd').value;
            const description = modal.querySelector('#timeEntryDescription').value;
            const isBillable = modal.querySelector('#timeEntryBillable').checked;
            
            const startedAt = new Date(`${date}T${startTime}`);
            const endedAt = new Date(`${date}T${endTime}`);
            
            // Проверяем, что конечное время после начального
            if (endedAt <= startedAt) {
                showToast('End time must be after start time', 'error');
                return;
            }
            
            const duration = (endedAt - startedAt) / 3600000; // в часах
            
            try {
                await TaskAPI.addTimeEntry(taskId, {
                    startedAt: startedAt.toISOString(),
                    endedAt: endedAt.toISOString(),
                    description: description,
                    isBillable: isBillable,
                    amount: duration * 50, // Примерная ставка $50/час
                    currency: 'USD'
                });
                
                showToast('Time entry added successfully');
                document.body.removeChild(modal);
                
                // Обновляем детальное модальное окно если оно открыто
                const taskModal = document.getElementById('taskModal');
                if (taskModal && !taskModal.classList.contains('hidden')) {
                    openTaskModal(taskId);
                }
            } catch (error) {
                console.error('Failed to add time entry:', error);
                showToast('Failed to add time entry', 'error');
            }
        });
        
    } catch (error) {
        console.error('Failed to show time entry modal:', error);
        showToast('Failed to load time entry form', 'error');
    }
}

function showCancelTaskModal(taskId) {
    const reason = prompt('Please enter reason for cancellation:');
    if (reason === null) return;
    
    TaskAPI.cancelTask(taskId, reason)
        .then(() => {
            showToast('Task cancelled successfully');
            loadTasks(currentFilters);
            
            // Закрываем детальное модальное окно если оно открыто
            const taskModal = document.getElementById('taskModal');
            if (taskModal) {
                taskModal.classList.add('hidden');
            }
        })
        .catch(error => {
            console.error('Failed to cancel task:', error);
            showToast('Failed to cancel task', 'error');
        });
}