import { formatDate } from '../api.js';
import { NotificationAPI } from '../api.js';


// Состояние уведомлений
let notifications = [];
export async function initNotifications() {
    await loadNotifications();
    updateNotificationBadge();
}
export async function initNotificationsPage() {
    const notifications = await loadNotifications();
    // Рендеринг уведомлений на странице
}
// Загрузка уведомлений
export async function loadNotifications() {
    try {
        // Заглушка - в реальности запрос к API
        notifications = [
            {
                id: '1',
                type: 'project',
                title: 'Project Update',
                message: 'Michael Brown approved the design mockups for the e-commerce project',
                read: false,
                timestamp: '2023-06-10T14:30:00Z',
                action: { type: 'project', id: 'project-1' }
            },
            {
                id: '2',
                type: 'system',
                title: 'System Notification',
                message: 'New milestone added to "Mobile Banking App" project',
                read: false,
                timestamp: '2023-06-10T10:15:00Z',
                action: { type: 'project', id: 'project-2' }
            },
            {
                id: '3',
                type: 'task',
                title: 'Task Assigned',
                message: 'You have been assigned a new task: "Design homepage layout"',
                read: true,
                timestamp: '2023-06-09T16:45:00Z',
                action: { type: 'task', id: 'task-1' }
            }
        ];
        
        renderNotifications();
        return notifications;
    } catch (error) {
        console.error('Failed to load notifications:', error);
        return [];
    }
}

// Отображение уведомлений
function renderNotifications() {
    const container = document.getElementById('notificationsList');
    if (!container) return;
    
    container.innerHTML = notifications.map(notification => `
        <div class="p-4 hover:bg-gray-50 cursor-pointer ${notification.read ? 'bg-white' : 'bg-blue-50'}" data-id="${notification.id}">
            <div class="flex items-start">
                <div class="flex-shrink-0 mr-3">
                    <div class="w-8 h-8 rounded-full flex items-center justify-center 
                                ${notification.type === 'project' ? 'bg-blue-100 text-blue-600' : 
                                  notification.type === 'task' ? 'bg-green-100 text-green-600' : 
                                  'bg-purple-100 text-purple-600'}">
                        <i class="${getNotificationIcon(notification.type)}"></i>
                    </div>
                </div>
                <div class="flex-1">
                    <p class="text-sm font-medium text-gray-900">${notification.title}</p>
                    <p class="text-sm text-gray-500">${notification.message}</p>
                    <p class="text-xs text-gray-400 mt-1">${formatDate(notification.timestamp)}</p>
                </div>
                ${!notification.read ? `
                    <div class="flex-shrink-0">
                        <span class="h-2 w-2 rounded-full bg-blue-500"></span>
                    </div>
                ` : ''}
            </div>
        </div>
    `).join('');
    
    // Настройка обработчиков событий
    setupNotificationEventListeners();
}

// Настройка обработчиков событий для уведомлений
function setupNotificationEventListeners() {
    // Клик по уведомлению
    document.querySelectorAll('#notificationsList > div').forEach(item => {
        item.addEventListener('click', (e) => {
            const notificationId = item.dataset.id;
            const notification = notifications.find(n => n.id === notificationId);
            
            if (notification) {
                // Пометить как прочитанное
                markAsRead(notificationId);
                
                // Выполнить действие
                handleNotificationAction(notification);
            }
        });
    });
    
    // Кнопка "Mark all as read"
    const markAllBtn = document.getElementById('markAllAsRead');
    if (markAllBtn) {
        markAllBtn.addEventListener('click', () => {
            notifications.forEach(n => markAsRead(n.id));
        });
    }
}

// Пометить уведомление как прочитанное
function markAsRead(notificationId) {
    const notification = notifications.find(n => n.id === notificationId);
    if (notification && !notification.read) {
        notification.read = true;
        renderNotifications();
        
        // Здесь будет вызов API для обновления статуса уведомления
        console.log(`Marked notification ${notificationId} as read`);
    }
}

// Обработка действия уведомления
function handleNotificationAction(notification) {
    switch (notification.action.type) {
        case 'project':
            // Открыть проект
            console.log(`Opening project ${notification.action.id}`);
            // В реальном приложении: openProjectModal(notification.action.id);
            break;
        case 'task':
            // Открыть задачу
            console.log(`Opening task ${notification.action.id}`);
            // В реальном приложении: openTaskModal(notification.action.id);
            break;
        default:
            // Ничего не делать
            break;
    }
}

// Получение иконки для типа уведомления
function getNotificationIcon(type) {
    const icons = {
        project: 'fas fa-project-diagram',
        task: 'fas fa-tasks',
        system: 'fas fa-bell',
        invoice: 'fas fa-file-invoice-dollar',
        message: 'fas fa-envelope'
    };
    return icons[type] || 'fas fa-bell';
}

// Обновление счетчика непрочитанных уведомлений
export function updateNotificationBadge() {
    const unreadCount = notifications.filter(n => !n.read).length;
    const headerBadge = document.getElementById('notificationBadge');
    const sidebarBadge = document.getElementById('sidebarNotificationBadge');
    
    if (headerBadge) {
        if (unreadCount > 0) {
            headerBadge.textContent = unreadCount;
            headerBadge.classList.remove('hidden');
        } else {
            headerBadge.classList.add('hidden');
        }
    }
    
    if (sidebarBadge) {
        if (unreadCount > 0) {
            sidebarBadge.textContent = unreadCount;
            sidebarBadge.classList.remove('hidden');
        } else {
            sidebarBadge.classList.add('hidden');
        }
    }
}