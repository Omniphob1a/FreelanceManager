import { TaskAPI } from '../api.js';
import { formatDate, getDaysLeft, showToast } from './ui.js';

let currentTasks = [];
let currentFilters = {};

export async function initTaskManagement() {
    await loadTasks();
    setupTaskEventListeners();
    setupTaskFilters();
}

async function loadTasks(filters = {}) {
    try {
        const response = await TaskAPI.getTasks(filters);
        currentTasks = response.items || [];
        renderTasks(currentTasks);
    } catch (error) {
        console.error('Failed to load tasks:', error);
        showToast('Failed to load tasks', 'error');
    }
}

function renderTasks(tasks) {
    const todoContainer = document.getElementById('todoTasks');
    const inProgressContainer = document.getElementById('inProgressTasks');
    const completedContainer = document.getElementById('completedTasks');
    
    [todoContainer, inProgressContainer, completedContainer].forEach(container => {
        container.innerHTML = '';
    });

    tasks.forEach(task => {
        const taskElement = createTaskElement(task);
        switch(task.status) {
            case 0: // To Do
                todoContainer.appendChild(taskElement);
                break;
            case 1: // In Progress
                inProgressContainer.appendChild(taskElement);
                break;
            case 2: // Completed
                completedContainer.appendChild(taskElement);
                break;
        }
    });
}

function createTaskElement(task) {
    const element = document.createElement('div');
    element.className = 'bg-white rounded-lg border border-gray-200 p-4 cursor-pointer hover:shadow-md transition-shadow';
    element.dataset.id = task.id;
    
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
            <div class="${getDaysLeft(task.dueDate).includes('overdue') ? 'text-red-500' : 'text-gray-500'}">
                <i class="far fa-clock mr-1"></i> 
                ${getDaysLeft(task.dueDate)}
            </div>
        </div>
        ${task.assigneeName ? `
        <div class="mt-2 flex items-center">
            <div class="w-6 h-6 rounded-full bg-gray-300 flex items-center justify-center overflow-hidden mr-2">
                <img src="https://ui-avatars.com/api/?name=${task.assigneeName}&background=random" alt="${task.assigneeName}">
            </div>
            <span class="text-xs text-gray-500">${task.assigneeName}</span>
        </div>
        ` : ''}
    `;
    
    element.addEventListener('click', () => openTaskModal(task.id));
    return element;
}

function getPriorityClass(priority) {
    const classes = {
        0: 'bg-gray-100 text-gray-800',
        1: 'bg-blue-100 text-blue-800',
        2: 'bg-yellow-100 text-yellow-800',
        3: 'bg-red-100 text-red-800'
    };
    return classes[priority] || 'bg-gray-100 text-gray-800';
}

function getPriorityText(priority) {
    const texts = {
        0: 'Low',
        1: 'Medium',
        2: 'High',
        3: 'Urgent'
    };
    return texts[priority] || 'Unknown';
}

async function openTaskModal(taskId) {
    try {
        const task = await TaskAPI.getTaskById(taskId, ['timeEntries', 'comments']);
        renderTaskModal(task);
    } catch (error) {
        console.error('Failed to load task details:', error);
        showToast('Failed to load task details', 'error');
    }
}

function renderTaskModal(task) {
    const modal = document.getElementById('taskModal');
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
                                <span class="text-sm font-medium ${getStatusClass(task.status)} px-2 py-1 rounded-full">
                                    ${getStatusText(task.status)}
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
                    <div class="space-y-2">
                        ${task.timeEntries && task.timeEntries.length > 0 ? 
                            task.timeEntries.map(entry => `
                                <div class="flex justify-between items-center p-2 bg-gray-50 rounded">
                                    <div>
                                        <span class="text-sm font-medium">${formatDate(entry.startedAt)}</span>
                                        <span class="text-xs text-gray-500 ml-2">${entry.description || 'No description'}</span>
                                    </div>
                                    <span class="text-sm font-medium">${(entry.duration / 36000000000).toFixed(1)}h</span>
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
                    <div class="space-y-4">
                        ${task.comments && task.comments.length > 0 ? 
                            task.comments.map(comment => `
                                <div class="flex">
                                    <div class="flex-shrink-0 mr-3">
                                        <img class="h-8 w-8 rounded-full" src="https://ui-avatars.com/api/?name=${comment.authorName}&background=random" alt="${comment.authorName}">
                                    </div>
                                    <div class="bg-gray-50 p-3 rounded-lg flex-1">
                                        <div class="flex justify-between">
                                            <span class="text-sm font-medium">${comment.authorName}</span>
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

function getStatusClass(status) {
    const classes = {
        0: 'bg-gray-100 text-gray-800',
        1: 'bg-blue-100 text-blue-800',
        2: 'bg-green-100 text-green-800',
        3: 'bg-red-100 text-red-800'
    };
    return classes[status] || 'bg-gray-100 text-gray-800';
}

function getStatusText(status) {
    const texts = {
        0: 'To Do',
        1: 'In Progress',
        2: 'Completed',
        3: 'Cancelled'
    };
    return texts[status] || 'Unknown';
}

function setupTaskEventListeners() {
    document.getElementById('newTaskBtn')?.addEventListener('click', showCreateTaskModal);
}

function setupTaskFilters() {
    // Реализация фильтров будет добавлена позже
}

function setupTaskModalEventListeners(task) {
    const modal = document.getElementById('taskModal');
    
    // Закрытие модального окна
    modal.querySelector('.close-task-modal').addEventListener('click', () => {
        modal.classList.add('hidden');
    });
    
    // Обработчики для кнопок действий
    if (modal.querySelector('.assign-task-btn')) {
        modal.querySelector('.assign-task-btn').addEventListener('click', () => {
            showAssignTaskModal(task.id);
        });
    }
    
    if (modal.querySelector('.start-task-btn')) {
        modal.querySelector('.start-task-btn').addEventListener('click', () => {
            startTask(task.id);
        });
    }
    
    if (modal.querySelector('.complete-task-btn')) {
        modal.querySelector('.complete-task-btn').addEventListener('click', () => {
            completeTask(task.id);
        });
    }
    
    if (modal.querySelector('.cancel-task-btn')) {
        modal.querySelector('.cancel-task-btn').addEventListener('click', () => {
            showCancelTaskModal(task.id);
        });
    }
    
    if (modal.querySelector('.edit-task-btn')) {
        modal.querySelector('.edit-task-btn').addEventListener('click', () => {
            showEditTaskModal(task);
        });
    }
    
    if (modal.querySelector('.delete-task-btn')) {
        modal.querySelector('.delete-task-btn').addEventListener('click', () => {
            deleteTask(task.id);
        });
    }
    
    if (modal.querySelector('.add-time-entry-btn')) {
        modal.querySelector('.add-time-entry-btn').addEventListener('click', () => {
            showAddTimeEntryModal(task.id);
        });
    }
    
    if (modal.querySelector('.add-comment-btn')) {
        modal.querySelector('.add-comment-btn').addEventListener('click', () => {
            const commentText = modal.querySelector('.comment-input').value;
            if (commentText) {
                addComment(task.id, commentText);
            }
        });
    }
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

async function addComment(taskId, text) {
    try {
        const user = await UserAPI.getProfile();
        await TaskAPI.addComment(taskId, {
            authorId: user.id,
            text: text
        });
        showToast('Comment added successfully');
        // Обновляем модальное окно
        openTaskModal(taskId);
    } catch (error) {
        console.error('Failed to add comment:', error);
        showToast('Failed to add comment', 'error');
    }
}

// Остальные функции (showCreateTaskModal, showAssignTaskModal, и т.д.) будут реализованы позже