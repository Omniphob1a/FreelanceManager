import { formatDate, getDaysLeft } from '../api.js';

export async function initTasksPage() {
    await loadTasks();
    
    document.getElementById('newTaskBtn')?.addEventListener('click', () => {
        alert('Create new task form will be implemented');
    });
}
// Загрузка задач
export async function loadTasks() {
    try {
        // Здесь будет реальный запрос к API
        const tasks = [
            {
                id: '1',
                title: 'Design homepage layout',
                description: 'Create wireframes and mockups for homepage',
                projectId: 'project-1',
                projectName: 'E-commerce Website',
                assignee: { id: 'user-1', name: 'Sarah Johnson' },
                dueDate: '2023-06-15',
                status: 'todo',
                priority: 'high',
                billable: true,
                estimatedHours: 8
            },
            // ... другие задачи
        ];
        
        renderTasks(tasks);
    } catch (error) {
        console.error('Failed to load tasks:', error);
    }
}

// Отображение задач
function renderTasks(tasks) {
    const todoContainer = document.getElementById('todoTasks');
    const inProgressContainer = document.getElementById('inProgressTasks');
    const completedContainer = document.getElementById('completedTasks');
    
    if (!todoContainer || !inProgressContainer || !completedContainer) return;
    
    // Очищаем контейнеры
    todoContainer.innerHTML = '';
    inProgressContainer.innerHTML = '';
    completedContainer.innerHTML = '';
    
    // Распределяем задачи по статусам
    tasks.forEach(task => {
        const taskElement = createTaskElement(task);
        
        switch(task.status) {
            case 'todo':
                todoContainer.appendChild(taskElement);
                break;
            case 'inprogress':
                inProgressContainer.appendChild(taskElement);
                break;
            case 'completed':
                completedContainer.appendChild(taskElement);
                break;
        }
    });
}

// Создание элемента задачи
function createTaskElement(task) {
    const element = document.createElement('div');
    element.className = 'bg-white rounded-lg border border-gray-200 p-4 cursor-pointer hover:shadow-md transition-shadow';
    element.dataset.id = task.id;
    
    element.innerHTML = `
        <div class="flex justify-between items-start">
            <div>
                <h4 class="font-medium text-gray-900">${task.title}</h4>
                <p class="text-sm text-gray-500 truncate">${task.description}</p>
            </div>
            <span class="px-2 py-1 text-xs font-medium rounded-full ${getPriorityClass(task.priority)}">
                ${task.priority}
            </span>
        </div>
        <div class="mt-3 flex items-center justify-between">
            <div class="text-xs text-gray-500">
                <i class="far fa-calendar-alt mr-1"></i> ${formatDate(task.dueDate)}
            </div>
            <div class="text-xs ${getDaysLeft(task.dueDate) < 3 ? 'text-red-500' : 'text-gray-500'}">
                <i class="far fa-clock mr-1"></i> ${getDaysLeft(task.dueDate)} days left
            </div>
        </div>
        <div class="mt-2 flex items-center">
            <div class="w-6 h-6 rounded-full bg-gray-300 flex items-center justify-center overflow-hidden mr-2">
                <img src="https://randomuser.me/api/portraits/women/44.jpg" alt="Assignee">
            </div>
            <span class="text-xs text-gray-500">${task.assignee.name}</span>
        </div>
    `;
    
    element.addEventListener('click', () => openTaskModal(task.id));
    
    return element;
}

// Открытие модального окна задачи
async function openTaskModal(taskId) {
    try {
        // Здесь будет запрос к API для получения деталей задачи
        const task = {
            id: taskId,
            title: 'Design homepage layout',
            description: 'Create wireframes and mockups for homepage with focus on user experience and conversion optimization',
            project: { id: 'project-1', name: 'E-commerce Website' },
            assignee: { id: 'user-1', name: 'Sarah Johnson' },
            reporter: { id: 'user-2', name: 'Michael Brown' },
            dueDate: '2023-06-15',
            status: 'todo',
            priority: 'high',
            billable: true,
            estimatedHours: 8,
            loggedHours: 2,
            createdAt: '2023-05-20',
            updatedAt: '2023-05-25'
        };
        
        renderTaskModal(task);
        document.getElementById('taskModal').classList.remove('hidden');
    } catch (error) {
        console.error('Failed to load task details:', error);
    }
}

// Отображение модального окна задачи
function renderTaskModal(task) {
    const modal = document.getElementById('taskModal');
    if (!modal) return;
    
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
                    <h4 class="text-lg font-semibold text-gray-900 mb-2">${task.title}</h4>
                    <p class="text-gray-700">${task.description}</p>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                    <div>
                        <h5 class="font-medium text-gray-900 mb-3">Details</h5>
                        <div class="space-y-2">
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Project:</span>
                                <span class="text-sm font-medium">${task.project.name}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Assignee:</span>
                                <span class="text-sm font-medium">${task.assignee.name}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Reporter:</span>
                                <span class="text-sm font-medium">${task.reporter.name}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Priority:</span>
                                <span class="text-sm font-medium ${getPriorityClass(task.priority)} px-2 py-1 rounded-full">${task.priority}</span>
                            </div>
                        </div>
                    </div>
                    
                    <div>
                        <h5 class="font-medium text-gray-900 mb-3">Timing</h5>
                        <div class="space-y-2">
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Due Date:</span>
                                <span class="text-sm font-medium">${formatDate(task.dueDate)}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Created:</span>
                                <span class="text-sm font-medium">${formatDate(task.createdAt)}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Updated:</span>
                                <span class="text-sm font-medium">${formatDate(task.updatedAt)}</span>
                            </div>
                            <div class="flex">
                                <span class="text-sm text-gray-500 w-32">Time Tracking:</span>
                                <span class="text-sm font-medium">${task.loggedHours}h / ${task.estimatedHours}h</span>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="mb-6">
                    <h5 class="font-medium text-gray-900 mb-3">Actions</h5>
                    <div class="flex flex-wrap gap-2">
                        <button class="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700">
                            <i class="fas fa-edit mr-2"></i> Edit Task
                        </button>
                        <button class="px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-md hover:bg-green-700">
                            <i class="fas fa-check-circle mr-2"></i> Mark as Completed
                        </button>
                        <button class="px-4 py-2 bg-yellow-500 text-white text-sm font-medium rounded-md hover:bg-yellow-600">
                            <i class="fas fa-clock mr-2"></i> Log Time
                        </button>
                        <button class="px-4 py-2 bg-red-100 text-red-600 text-sm font-medium rounded-md hover:bg-red-200">
                            <i class="fas fa-trash-alt mr-2"></i> Delete
                        </button>
                    </div>
                </div>
                
                <div>
                    <h5 class="font-medium text-gray-900 mb-3">Comments</h5>
                    <div class="space-y-4">
                        <div class="flex">
                            <div class="flex-shrink-0 mr-3">
                                <img class="h-8 w-8 rounded-full" src="https://randomuser.me/api/portraits/men/32.jpg" alt="User">
                            </div>
                            <div class="bg-gray-50 p-3 rounded-lg flex-1">
                                <div class="flex justify-between">
                                    <span class="text-sm font-medium">Michael Brown</span>
                                    <span class="text-xs text-gray-500">2 hours ago</span>
                                </div>
                                <p class="text-sm mt-1">Can you add more examples of similar designs?</p>
                            </div>
                        </div>
                        <!-- More comments -->
                    </div>
                    
                    <div class="mt-4 flex">
                        <input type="text" placeholder="Add a comment..." class="flex-1 px-3 py-2 border border-gray-300 rounded-l-md focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <button class="px-4 py-2 bg-blue-600 text-white rounded-r-md hover:bg-blue-700">
                            <i class="fas fa-paper-plane"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Настройка кнопки закрытия
    const closeBtn = modal.querySelector('.close-task-modal');
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            document.getElementById('taskModal').classList.add('hidden');
        });
    }
}

// Вспомогательные функции
function getPriorityClass(priority) {
    const classes = {
        high: 'bg-red-100 text-red-800',
        medium: 'bg-yellow-100 text-yellow-800',
        low: 'bg-green-100 text-green-800'
    };
    return classes[priority] || 'bg-gray-100 text-gray-800';
}