// modules/projects.js
import { ProjectAPI, getDaysLeft } from '../api.js';
import { getCurrentUser } from './auth.js';
import { showToast } from './ui.js';
import { openProjectModal } from './modalManager.js';
import { formatDate, getStatusClass, getStatusText, formatBudget } from './ui.js';

export let currentProjects = [];

// Заменяем существующую функцию
function calculateProjectProgress(project) {
    if (!project.milestones || !Array.isArray(project.milestones) || 
        project.milestones.length === 0) {
        return 0;
    }
    
    const completed = project.milestones.filter(m => m.isCompleted).length;
    return Math.round((completed / project.milestones.length) * 100);
}

export function updateProjectInUI(updatedProject) {
    console.log('Updating project in UI:', updatedProject.id, 'with status:', updatedProject.status);
    
    // 1. Обновляем в таблице проектов
    const tableRow = document.querySelector(`tr[data-id="${updatedProject.id}"]`);
    if (tableRow) {
        const statusCell = tableRow.querySelector('.project-status');
        if (statusCell) {
            statusCell.textContent = getStatusText(updatedProject.status);
            statusCell.className = `project-status px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(updatedProject.status)}`;
        }
    }
    
    // 2. Обновляем в карточках на дашборде
    const card = document.querySelector(`.project-card[data-id="${updatedProject.id}"]`);
    if (card) {
        const statusBadge = card.querySelector('.status-badge');
        if (statusBadge) {
            statusBadge.textContent = getStatusText(updatedProject.status);
            statusBadge.className = `status-badge ${getStatusClass(updatedProject.status)}`;
        }
    }
    
    // 3. Обновляем весь currentProjects массив
    const projectIndex = currentProjects.findIndex(p => p.id === updatedProject.id);
    if (projectIndex !== -1) {
        currentProjects[projectIndex] = { ...currentProjects[projectIndex], ...updatedProject };
    }
    
    // 4. Принудительно обновляем статистику дашборда - ИСПРАВЛЕНО: используем ES6 import
    try {
        // Динамический импорт функции обновления статистики
        import('./dashboardStats.js').then(({ updateDashboardStats }) => {
            if (typeof updateDashboardStats === 'function') {
                updateDashboardStats(currentProjects);
            }
        }).catch(error => {
            console.warn('Failed to update dashboard stats:', error);
        });
    } catch (error) {
        console.warn('Failed to update dashboard stats:', error);
    }
}

export async function loadProjects(params = {}) {
    try {
        const loader = document.getElementById('projectsLoader');
        const noProjectsRow = document.getElementById('noProjectsRow');

        if (loader) loader.classList.remove('hidden');
        if (noProjectsRow) noProjectsRow.classList.add('hidden');

        const user = await getCurrentUser();
        if (!user?.id) {
            console.warn('No authenticated user found');
            return [];
        }

        // Извлекаем параметры пагинации из params
        const { page, itemsPerPage, pageSize, ...filters } = params;

        // Нормализуем статус — если это массив (мультиселект), преобразуем в CSV,
        // т.к. бекенд у тебя, вероятно, принимает статус как "Draft,Active".
        let statusParam = filters.status;
        if (Array.isArray(statusParam)) {
            statusParam = statusParam.join(',');
        } else if (typeof statusParam === 'string') {
            // уже строка, оставляем как есть
        } else {
            statusParam = undefined;
        }

        const requestParams = {
            ownerId: user.id,
            sort: '-createdAt',
            includeMilestones: true, // Важно для прогресса
            includeAttachments: false,
            // Устанавливаем пагинацию динамически
            page: page || 1,
            pageSize: pageSize || itemsPerPage || 10, // Поддерживаем оба названия параметра
            ...filters
        };

        // Подменяем статус в requestParams на корректное (CSV) значение, если есть
        if (statusParam) {
            requestParams.status = statusParam;
        } else {
            // Если filters.status был массив, но пустой, удаляем параметр
            delete requestParams.status;
        }

        const response = await ProjectAPI.getProjects(requestParams);

        // Обрабатываем ответ в зависимости от того, пагинированный он или нет
        let projects;
        let totalCount = 0;

        if (response && response.items && Array.isArray(response.items)) {
            // Пагинированный ответ (PaginatedResult)
            projects = response.items;
            totalCount = response.pagination?.totalItems || projects.length;
        } else {
            // Обычный массив или старый формат
            projects = normalizeProjectsResponse(response);
            totalCount = projects.length;
        }

        currentProjects = projects;

        const projectsGrid = document.getElementById('projectsGrid');
        const dashboardProjects = document.getElementById('dashboardProjects');

        if (projectsGrid) {
            renderProjectsGrid(projects);
        }

        if (dashboardProjects) {
            renderDashboardProjects(projects);
        }

        if (document.getElementById('totalProjects')) {
            import('./dashboardStats.js').then(({ updateDashboardStats }) => {
                updateDashboardStats(projects);
            }).catch(error => {
                console.warn('Failed to update dashboard stats:', error);
            });
        }

        if (loader) loader.classList.add('hidden');
        if (projects.length === 0 && noProjectsRow) {
            noProjectsRow.classList.remove('hidden');
        }

        // Возвращаем результат в зависимости от типа запроса
        if (page !== undefined || pageSize !== undefined || itemsPerPage !== undefined) {
            // Если запрашивалась пагинация, возвращаем структуру как PaginatedResult
            return {
                items: projects,
                pagination: {
                    totalItems: totalCount,
                    itemsPerPage: pageSize || itemsPerPage || projects.length,
                    actualPage: page || 1,
                    totalPages: Math.ceil(totalCount / (pageSize || itemsPerPage || projects.length))
                }
            };
        } else {
            // Обратная совместимость - возвращаем просто массив
            return projects;
        }

    } catch (error) {
        console.error('Failed to load projects:', error);
        showToast(error.message || 'Failed to load projects', 'error');

        const loader = document.getElementById('projectsLoader');
        const noProjectsRow = document.getElementById('noProjectsRow');
        if (loader) loader.classList.add('hidden');
        if (noProjectsRow) noProjectsRow.classList.remove('hidden');

        return [];
    }
}
function getExpandableFilterValues() {
    const filters = {};

    // Status Filters (checkboxes с классом .filter-status)
    const statuses = Array.from(document.querySelectorAll('.filter-status'))
        .filter(cb => cb.checked)
        .map(cb => cb.value);

    if (statuses.length) {
        filters.status = statuses; // Массив строк, например ['Draft','Active']
    }

    // Date Range inputs
    const createdFromEl = document.getElementById('filterCreatedFrom');
    const createdToEl = document.getElementById('filterCreatedTo');
    const createdFrom = createdFromEl ? createdFromEl.value : '';
    const createdTo = createdToEl ? createdToEl.value : '';
    if (createdFrom) filters.createdFrom = createdFrom;
    if (createdTo) filters.createdTo = createdTo;

    // Category select
    const categoryEl = document.getElementById('filterCategory');
    const category = categoryEl ? categoryEl.value : '';
    if (category) filters.category = category;

    // Budget Range
    const minBudgetEl = document.getElementById('filterMinBudget');
    const maxBudgetEl = document.getElementById('filterMaxBudget');
    const minBudget = minBudgetEl ? minBudgetEl.value : '';
    const maxBudget = maxBudgetEl ? maxBudgetEl.value : '';
    if (minBudget) filters.minBudget = minBudget;
    if (maxBudget) filters.maxBudget = maxBudget;

    return filters;
}

function setupExpandableFilters() {
    const toggleBtn = document.getElementById('toggleFiltersBtn');
    const panel = document.getElementById('filtersPanel');
    const applyBtn = document.getElementById('applyFiltersBtn');
    const clearBtn = document.getElementById('clearFiltersBtn');

    if (!toggleBtn || !panel) {
        console.error('Filter elements not found in DOM');
        return;
    }

    // Защита от повторной инициализации отдельных обработчиков для этой панели
    if (toggleBtn.dataset.filtersInit === 'true') {
        console.log('Expandable filters already initialized');
        return;
    }
    toggleBtn.dataset.filtersInit = 'true';

    console.log('Setting up expandable filters...');

    // Явно инициализируем состояние: закрытая панель по умолчанию (если нужно скрыть)
    panel.classList.remove('expanded', 'filter-panel-expanded');
    // Если нужно, чтобы панель была видимой в DOM по-умолчанию, убери следующую строку
    panel.classList.remove('hidden');

    // Гарантируем, что кнопки не вызывают submit
    if (applyBtn) applyBtn.type = 'button';
    if (clearBtn) clearBtn.type = 'button';

    // Toggle handler
    const toggleHandler = (e) => {
        e.preventDefault();
        e.stopPropagation();

        const currentlyExpanded = panel.classList.contains('expanded');
        console.log('Toggle filters button clicked. Panel expanded (before):', currentlyExpanded);

        if (currentlyExpanded) {
            panel.classList.remove('expanded', 'filter-panel-expanded');
            toggleBtn.setAttribute('aria-expanded', 'false');
            console.log('Panel collapsed');
        } else {
            panel.classList.add('expanded', 'filter-panel-expanded');
            toggleBtn.setAttribute('aria-expanded', 'true');
            console.log('Panel expanded');
        }
    };

    toggleBtn.addEventListener('click', toggleHandler);

    // Обработчик применения фильтров — закрываем панель только после успешного применения
    if (applyBtn) {
        applyBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            e.stopPropagation();
            console.log('[Filters] Apply clicked');

            const filters = getExpandableFilterValues();
            console.log('[Filters] Collected:', filters);

            try {
                // Защитная проверка аутентификации (если loadProjects требует текущего пользователя)
                const user = await getCurrentUser();
                if (!user?.id) {
                    showToast('You must be logged in to apply filters', 'error');
                    return;
                }

                // Подгружаем первую страницу с новыми фильтрами
                const res = await loadProjectsPage(1, getCurrentPageSize(), filters);
                console.log('[Filters] loadProjectsPage returned:', res);

                showToast('Filters applied', 'success');

                // Закрываем панель только после успешного результата
                panel.classList.remove('expanded', 'filter-panel-expanded');
                toggleBtn.setAttribute('aria-expanded', 'false');

                // Обновляем счетчик активных фильтров
                updateFilterCounter();
            } catch (error) {
                console.error('[Filters] Failed to apply filters:', error);
                showToast('Failed to apply filters', 'error');
            }
        });
    }

    // Очистка фильтров
    if (clearBtn) {
        clearBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            e.stopPropagation();
            console.log('[Filters] Clear clicked');

            // Сброс значений UI
            document.querySelectorAll('.filter-status').forEach(cb => cb.checked = false);
            const createdFromInput = document.getElementById('filterCreatedFrom');
            const createdToInput = document.getElementById('filterCreatedTo');
            const categorySelect = document.getElementById('filterCategory');
            const minBudgetInput = document.getElementById('filterMinBudget');
            const maxBudgetInput = document.getElementById('filterMaxBudget');

            if (createdFromInput) createdFromInput.value = '';
            if (createdToInput) createdToInput.value = '';
            if (categorySelect) categorySelect.value = '';
            if (minBudgetInput) minBudgetInput.value = '';
            if (maxBudgetInput) maxBudgetInput.value = '';

            try {
                await loadProjectsPage(1, getCurrentPageSize(), {}); // без фильтров
                showToast('Filters cleared', 'info');

                // Закрываем панель после очистки
                panel.classList.remove('expanded', 'filter-panel-expanded');
                toggleBtn.setAttribute('aria-expanded', 'false');

                updateFilterCounter();
            } catch (error) {
                console.error('[Filters] Failed to clear filters:', error);
                showToast('Failed to clear filters', 'error');
            }
        });
    }

    // Закрываем панель при клике вне её блока — добавляем обработчик один раз глобально
    if (!window._filtersOutsideClickInit) {
        window._filtersOutsideClickInit = true;
        document.addEventListener('click', (e) => {
            const tBtn = document.getElementById('toggleFiltersBtn');
            const pnl = document.getElementById('filtersPanel');
            if (!tBtn || !pnl) return;

            if (!tBtn.contains(e.target) && !pnl.contains(e.target)) {
                if (pnl.classList.contains('expanded')) {
                    pnl.classList.remove('expanded', 'filter-panel-expanded');
                    tBtn.setAttribute('aria-expanded', 'false');
                    console.log('Panel collapsed (outside click)');
                }
            }
        }, true); // use capture to reduce race with other handlers
    }

    // Предотвращаем закрытие при клике внутри панели
    panel.addEventListener('click', (e) => e.stopPropagation());

    // Вешаем обновление счетчика на элементы фильтров
    document.querySelectorAll('#filtersPanel input, #filtersPanel select').forEach(el => {
        el.addEventListener('change', updateFilterCounter);
        el.addEventListener('input', updateFilterCounter);
    });

    // Первичная синхронизация счетчика
    updateFilterCounter();
}


// ДОБАВЛЕНО: Функция для получения текущего размера страницы
function getCurrentPageSize() {
   const selector = document.getElementById('itemsPerPageSelect');
   return selector ? parseInt(selector.value) : 10;
}
export function initProjectsPage() {
    // Загружаем проекты
    loadProjectsPage();
    
    // Инициализируем компоненты
    setupProjectsPageListeners();
    setupProjectsPageFilters();
    setupExpandableFilters();
    setupViewToggle();
    setupItemsPerPageSelector();
    setupClearFilters();
    
    // Обновляем счетчик фильтров
    updateFilterCounter();
    
    // Вешаем обработчики на изменение фильтров
    document.querySelectorAll('#filtersPanel input, #filtersPanel select').forEach(el => {
        el.addEventListener('change', updateFilterCounter);
    });
}
function normalizeProjectsResponse(response) {
    if (!response) return [];
    
    if (Array.isArray(response)) {
        return response;
    }
    
    if (response.data && Array.isArray(response.data)) {
        return response.data;
    }
    
    if (response.items && Array.isArray(response.items)) {
        return response.items;
    }
    
    console.warn('Unexpected API response format', response);
    return [];
}

function setupProjectsPageListeners() {
    document.getElementById('newProjectBtn')?.addEventListener('click', () => {
        window.location.hash = 'project-form';
    });
}

function renderDashboardProjects(projects) {
    const container = document.getElementById('dashboardProjects');
    if (!container) return;
    
    container.innerHTML = projects.map(project => `
        <div class="bg-white rounded-lg shadow p-4 flex flex-col h-full">
            <div class="flex justify-between">
                <h4 class="font-medium truncate">${project.title}</h4>
                <span class="status-badge ${getStatusClass(project.status)}">
                    ${getStatusText(project.status)}
                </span>
            </div>
            
            <div class="mt-3 flex-1">
                <div class="text-xs text-gray-500 mb-1">Progress</div>
                <div class="w-full bg-gray-200 rounded-full h-1.5 mb-3">
                    <div class="${calculateProjectProgress(project) === 100 ? 'bg-green-500' : 'bg-blue-600'} h-1.5 rounded-full" 
                         style="width: ${calculateProjectProgress(project)}%"></div>
                </div>
            </div>
            
            <div class="flex justify-between text-xs">
                <span class="text-gray-500">
                    <i class="far fa-clock mr-1"></i>
                    ${getDaysLeft(project.expiresAt)}
                </span>
                <button class="text-blue-600 view-details-btn" data-id="${project.id}">
                    Details
                </button>
            </div>
        </div>
    `).join('');
    
    // Добавляем обработчики
    container.querySelectorAll('.view-details-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            openProjectModal(btn.dataset.id);
        });
    });
}
function setupProjectsPageFilters() {
    window.applyFilters = async () => {
        const filters = {};

        const statuses = Array.from(document.querySelectorAll('.filter-status'))
            .filter(cb => cb.checked)
            .map(cb => cb.value);
        if (statuses.length) filters.status = statuses.join(',');

        const category = document.getElementById('filterCategory').value;
        if (category) filters.category = category;

        const createdFrom = document.getElementById('filterCreatedFrom').value;
        if (createdFrom) filters.createdFrom = createdFrom;

        const createdTo = document.getElementById('filterCreatedTo').value;
        if (createdTo) filters.createdTo = createdTo;

        const minBudget = document.getElementById('filterMinBudget').value;
        if (minBudget) filters.minBudget = minBudget;

        const maxBudget = document.getElementById('filterMaxBudget').value;
        if (maxBudget) filters.maxBudget = maxBudget;

        await loadProjects(filters);
    };
}
function renderProjectsGrid(projects) {
    const projectsGrid = document.getElementById('projectsGrid');
    if (!projectsGrid) return;
    
    if (projects.length === 0) {
        projectsGrid.innerHTML = `
            <div class="col-span-full text-center py-8">
                <p class="text-gray-600">No projects found</p>
            </div>
        `;
        return;
    }
    
     projectsGrid.innerHTML = projects.map(project => `
        <div class="project-card bg-white rounded-lg shadow overflow-hidden transition-all duration-300" data-id="${project.id}">
            <div class="p-6">
                <div class="flex justify-between items-start mb-4">
                    <div>
                        <span class="status-badge ${getStatusClass(project.status)}">${getStatusText(project.status)}</span>
                    </div>
                    <!-- ... other elements ... -->
                </div>
                <h3 class="text-lg font-semibold text-gray-900 mb-2">${project.title || 'Untitled Project'}</h3>
                <p class="text-sm text-gray-500 mb-4 truncate">${project.description || 'No description'}</p>
                
                <!-- Progress bar -->
                <div class="mb-4">
                    <div class="flex justify-between text-xs text-gray-500 mb-1">
                        <span>Progress</span>
                        <span>${calculateProjectProgress(project)}%</span>
                    </div>
                    <div class="w-full bg-gray-200 rounded-full h-2">
                        <div class="${calculateProjectProgress(project) === 100 ? 'bg-green-500' : 'bg-blue-600'} h-2 rounded-full" 
                             style="width: ${calculateProjectProgress(project)}%"></div>
                    </div>
                </div>
                
                <!-- ... other elements ... -->
            </div>
            <div class="px-6 py-3 bg-gray-50 border-t border-gray-200 flex justify-between items-center">
                <div class="text-xs text-gray-500">
                    <i class="far fa-clock mr-1"></i> 
                    ${getDaysLeft(project.expiresAt)}
                </div>
                <button class="view-details-btn text-xs text-blue-600 hover:text-blue-800 font-medium">
                    View Details <i class="fas fa-chevron-right ml-1"></i>
                </button>
            </div>
        </div>
    `).join('');
    
    // Добавляем обработчик для кнопки просмотра деталей
    document.querySelectorAll('.view-details-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const projectId = btn.closest('.project-card').dataset.id;
            if (projectId) openProjectModal(projectId);
        });
    });
    
    document.querySelectorAll('.project-actions-toggle').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const menu = btn.nextElementSibling;
            document.querySelectorAll('.project-actions-menu').forEach(m => {
                if (m !== menu) m.classList.add('hidden');
            });
            menu.classList.toggle('hidden');
        });
    });
    
    document.addEventListener('click', () => {
        document.querySelectorAll('.project-actions-menu').forEach(menu => {
            menu.classList.add('hidden');
        });
    });
    
    document.querySelectorAll('.edit-project-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const projectId = btn.dataset.id;
            window.location.hash = `project-form?id=${projectId}`;
        });
    });
    
    document.querySelectorAll('.delete-project-btn').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.stopPropagation();
            const projectId = btn.dataset.id;
            if (confirm('Are you sure you want to delete this project?')) {
                try {
                    btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-1"></i> Deleting...';
                    await ProjectAPI.deleteProject(projectId);
                    showToast('Project deleted successfully');
                    removeProjectFromUI(projectId);
                } catch (error) {
                    showToast('Failed to delete project', 'error');
                    btn.innerHTML = 'Delete';
                }
            }
        });
    });
    
    document.querySelectorAll('.archive-project-btn').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.stopPropagation();
            const projectId = btn.dataset.id;
            if (confirm('Are you sure you want to archive this project?')) {
                try {
                    btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-1"></i> Archiving...';
                    await ProjectAPI.archiveProject(projectId);
                    showToast('Project archived');
                    
                    const updatedProject = await ProjectAPI.getProjectById(projectId);
                    updateProjectInUI(updatedProject);
                    
                    const menu = btn.closest('.project-actions-menu');
                    if (menu) menu.classList.add('hidden');
                } catch (error) {
                    showToast('Failed to archive project', 'error');
                } finally {
                    btn.innerHTML = 'Archive';
                }
            }
        });
    });
}

function renderProjectsTable(projects) {
    const tableBody = document.getElementById('projectsTableBody');
    const loader = document.getElementById('projectsLoader');
    const noProjectsRow = document.getElementById('noProjectsRow');
    
    if (loader) loader.classList.add('hidden');
    
    if (projects.length === 0) {
        if (noProjectsRow) noProjectsRow.classList.remove('hidden');
        return;
    }
    
    if (noProjectsRow) noProjectsRow.classList.add('hidden');
    
    tableBody.innerHTML = projects.map(project => `
        <tr class="hover:bg-gray-50" data-id="${project.id}">
            <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                    <div class="flex-shrink-0 h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                        <i class="fas fa-project-diagram text-blue-600"></i>
                    </div>
                    <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">${project.title || 'Untitled Project'}</div>
                        <div class="text-sm text-gray-500">${project.category || 'No category'}</div>
                    </div>
                </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
                <span class="project-status px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(project.status)}">
                    ${getStatusText(project.status)}
                </span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                ${project.expiresAt ? formatDate(project.expiresAt) : 'No due date'}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                ${formatBudget(project)}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button data-id="${project.id}" class="edit-btn text-blue-600 hover:text-blue-900 mr-3">Edit</button>
                <button data-id="${project.id}" class="delete-btn text-red-600 hover:text-red-900">Delete</button>
            </td>
        </tr>
    `).join('');

    document.querySelectorAll('#projectsTableBody tr').forEach(row => {
        row.addEventListener('click', (e) => {
            // Проверяем, что клик не по кнопкам действий
            if (!e.target.closest('.edit-btn') && !e.target.closest('.delete-btn')) {
                const projectId = row.dataset.id;
                if (projectId) {
                    console.log('Opening project modal for:', projectId);
                    openProjectModal(projectId);
                }
            }
        });
    });
    
    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            window.location.hash = `project-form?id=${btn.dataset.id}`;
        });
    });
    
    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            if (confirm('Are you sure you want to delete this project?')) {
                try {
                    btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-1"></i> Deleting...';
                    await ProjectAPI.deleteProject(btn.dataset.id);
                    showToast('Project deleted successfully');
                    removeProjectFromUI(btn.dataset.id);
                } catch (error) {
                    showToast('Failed to delete project', 'error');
                    btn.innerHTML = 'Delete';
                }
            }
        });
    });
}
function updateProjectsCount(pagination, currentItems) {
    const countElement = document.getElementById('projectsCount');
    if (!countElement) return;
    
    if (pagination && pagination.totalItems > 0) {
        const start = ((pagination.actualPage - 1) * pagination.itemsPerPage) + 1;
        const end = Math.min(start + currentItems - 1, pagination.totalItems);
        countElement.innerHTML = `
            Showing <span class="font-semibold">${start}-${end}</span> 
            of <span class="font-semibold">${pagination.totalItems}</span> projects
        `;
    } else if (currentItems > 0) {
        countElement.textContent = `${currentItems} project${currentItems !== 1 ? 's' : ''}`;
    } else {
        countElement.textContent = 'No projects found';
    }
}

// Функция переключения вида
function setupViewToggle() {
    const gridViewBtn = document.getElementById('gridViewBtn');
    const listViewBtn = document.getElementById('listViewBtn');
    const projectsGrid = document.getElementById('projectsGrid');
    const projectsTable = document.getElementById('projectsTable');

    if (!gridViewBtn || !listViewBtn || !projectsGrid) {
        console.warn('View toggle elements not found');
        return;
    }

    // Инициализация
    gridViewBtn.classList.add('active');
    if (projectsTable) projectsTable.classList.add('hidden');

    // Удаляем старые обработчики
    gridViewBtn.replaceWith(gridViewBtn.cloneNode(true));
    listViewBtn.replaceWith(listViewBtn.cloneNode(true));
    
    // Получаем свежие ссылки
    const freshGridBtn = document.getElementById('gridViewBtn');
    const freshListBtn = document.getElementById('listViewBtn');
    
    freshGridBtn.classList.add('active');

    freshGridBtn.addEventListener('click', (e) => {
        e.preventDefault();
        console.log('Grid view clicked');
        if (!freshGridBtn.classList.contains('active')) {
            freshGridBtn.classList.add('active');
            freshListBtn.classList.remove('active');
            projectsGrid.classList.remove('hidden');
            if (projectsTable) projectsTable.classList.add('hidden');
            
            // Перерендерим сетку
            renderProjectsGrid(currentProjects);
        }
    });

    freshListBtn.addEventListener('click', (e) => {
        e.preventDefault();
        console.log('List view clicked');
        if (!freshListBtn.classList.contains('active')) {
            freshListBtn.classList.add('active');
            freshGridBtn.classList.remove('active');
            projectsGrid.classList.add('hidden');
            
            if (projectsTable) {
                projectsTable.classList.remove('hidden');
                // Форсируем рендеринг таблицы
                renderProjectsTable(currentProjects);
            }
        }
    });
}

// Функция для изменения количества элементов на странице
function setupItemsPerPageSelector() {
    const selector = document.getElementById('itemsPerPageSelect');
    if (!selector) return;
    
    selector.addEventListener('change', () => {
        const newPageSize = parseInt(selector.value);
        loadProjectsPage(1, newPageSize); // Перезагружаем с первой страницы
    });
}

// Функция для очистки фильтров
function setupClearFilters() {
    const createBtn = document.getElementById('createFirstProjectBtn');
    
    if (createBtn) {
        // Удаляем старые обработчики
        createBtn.replaceWith(createBtn.cloneNode(true));
        const freshCreateBtn = document.getElementById('createFirstProjectBtn');
        
        freshCreateBtn.addEventListener('click', () => {
            window.location.hash = 'project-form';
        });
    }
}

// Обновленная функция loadProjectsPage
export async function loadProjectsPage(page = 1, pageSize = 10, additionalFilters = {}) {
    try {
        console.log(`Loading projects page ${page} with pageSize ${pageSize}`);
        
        // Получаем текущие фильтры из UI
        const uiFilters = getExpandableFilterValues();
        
        // Объединяем фильтры из UI и дополнительные
        const filters = { ...uiFilters, ...additionalFilters };
        
        console.log('Applied filters:', filters);
        
        // Загружаем проекты с пагинацией
        const projectsResult = await loadProjects({ 
            page: page, 
            pageSize: pageSize,
            ...filters
        });
        
        // Обновляем счетчик проектов
        const projects = projectsResult.items || projectsResult;
        updateProjectsCount(projectsResult.pagination, projects.length);
        
        // Рендерим пагинацию
        if (projectsResult.pagination) {
            renderPagination(projectsResult.pagination, page);
        }
        
        // Показываем/скрываем сообщения в зависимости от результата
        const hasFilters = Object.keys(filters).length > 0;
        const noProjectsRow = document.getElementById('noProjectsRow');
        
        if (projects.length === 0 && hasFilters && noProjectsRow) {
            const noProjectsMsg = noProjectsRow.querySelector('p');
            if (noProjectsMsg) {
                noProjectsMsg.textContent = 'No projects match your current filters';
            }
        } else if (noProjectsRow) {
            const noProjectsMsg = noProjectsRow.querySelector('p');
            if (noProjectsMsg) {
                noProjectsMsg.textContent = 'Get started by creating your first project';
            }
        }
        
        // Применяем текущий вид
        const gridViewBtn = document.getElementById('gridViewBtn');
        if (gridViewBtn && gridViewBtn.classList.contains('active')) {
            renderProjectsGrid(projects);
        } else {
            renderProjectsTable(projects);
        }
        
        // Инициализируем компоненты только один раз
        if (!window.projectsPageSetup) {
            setupProjectsPageListeners();
            setupProjectsPageFilters();
            setupExpandableFilters(); 
            setupViewToggle();
            setupItemsPerPageSelector();
            setupClearFilters();
            window.projectsPageSetup = true;
            console.log('Projects page setup completed');
        }
        
        return projectsResult;
        
    } catch (error) {
        console.error('Failed to load projects page:', error);
        showToast('Failed to load projects', 'error');
    }
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
function renderPagination(pagination, currentPage) {
    const paginationContainer = document.getElementById('paginationContainer');
    if (!paginationContainer) return;
    
    const { totalPages, actualPage } = pagination;
    
    if (totalPages <= 1) {
        paginationContainer.innerHTML = '';
        return;
    }
    
    let paginationHTML = '<div class="pagination-container">';
    
    // Кнопка "Предыдущая"
    if (actualPage > 1) {
        paginationHTML += `
            <button class="pagination-btn" data-page="${actualPage - 1}">
                <i class="fas fa-chevron-left mr-1"></i> Previous
            </button>
        `;
    } else {
        paginationHTML += `
            <button class="pagination-btn" disabled>
                <i class="fas fa-chevron-left mr-1"></i> Previous
            </button>
        `;
    }
    
    // Номера страниц
    for (let i = 1; i <= totalPages; i++) {
        if (i === actualPage) {
            paginationHTML += `
                <button class="pagination-btn active" data-page="${i}">
                    ${i}
                </button>
            `;
        } else if (i === 1 || i === totalPages || (i >= actualPage - 2 && i <= actualPage + 2)) {
            paginationHTML += `
                <button class="pagination-btn" data-page="${i}">
                    ${i}
                </button>
            `;
        } else if (i === actualPage - 3 || i === actualPage + 3) {
            paginationHTML += `<span class="pagination-ellipsis">...</span>`;
        }
    }
    
    // Кнопка "Следующая"
    if (actualPage < totalPages) {
        paginationHTML += `
            <button class="pagination-btn" data-page="${actualPage + 1}">
                Next <i class="fas fa-chevron-right ml-1"></i>
            </button>
        `;
    } else {
        paginationHTML += `
            <button class="pagination-btn" disabled>
                Next <i class="fas fa-chevron-right ml-1"></i>
            </button>
        `;
    }
    
    paginationHTML += '</div>';
    
    paginationContainer.innerHTML = paginationHTML;
    
    // Добавляем обработчики для кнопок пагинации
    paginationContainer.querySelectorAll('.pagination-btn:not([disabled])').forEach(btn => {
        btn.addEventListener('click', () => {
            const page = parseInt(btn.dataset.page);
            const pageSize = parseInt(document.getElementById('itemsPerPageSelect')?.value) || 10;
            loadProjectsPage(page, pageSize);
        });
    });
}
// Функция для получения текущих фильтров
function getCurrentFilters() {
    const filters = {};
    
    const statusFilter = document.getElementById('statusFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const dateRangeFilter = document.getElementById('dateRangeFilter');
    
    if (statusFilter && statusFilter.value !== 'all') {
        filters.status = statusFilter.value;
    }
    
    if (categoryFilter && categoryFilter.value !== 'all') {
        filters.category = categoryFilter.value;
    }
    
    if (dateRangeFilter && dateRangeFilter.value !== 'all') {
        let createdLastDays;
        switch(dateRangeFilter.value) {
            case 'week': createdLastDays = 7; break;
            case 'month': createdLastDays = 30; break;
            case 'quarter': createdLastDays = 90; break;
        }
        if (createdLastDays) {
            filters.createdLastDays = createdLastDays;
        }
    }
    
    return filters;
}
export function removeProjectFromUI(projectId) {
    const tableRow = document.querySelector(`tr[data-id="${projectId}"]`);
    if (tableRow) tableRow.remove();
    
    const card = document.querySelector(`.project-card[data-id="${projectId}"]`);
    if (card) card.remove();
    
    const tableBody = document.getElementById('projectsTableBody');
    if (tableBody && tableBody.children.length === 0) {
        document.getElementById('noProjectsRow').classList.remove('hidden');
    }
}

window.updateProjectsUI = async function() {
    try {
        const user = await getCurrentUser();
        if (!user?.id) return;
        
        const params = { 
            ownerId: user.id, 
            sort: '-createdAt',
            includeMilestones: false,
            includeAttachments: false
        };
        
        const response = await ProjectAPI.getProjects(params);
        currentProjects = normalizeProjectsResponse(response);
        
        const projectsGrid = document.getElementById('projectsGrid');
        const projectsTableBody = document.getElementById('projectsTableBody');
        
        if (projectsGrid) renderProjectsGrid(currentProjects);
        if (projectsTableBody) renderProjectsTable(currentProjects);
    } catch (error) {
        console.error('UI update failed:', error);
    }
};