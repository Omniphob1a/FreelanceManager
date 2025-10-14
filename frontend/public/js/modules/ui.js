import { formatCurrency } from '../api.js';

export function showToast(message, type = 'success') {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) return;
    
    let displayMessage = message;
    if (typeof message === 'string' && message.includes('failed:')) {
        const parts = message.split(':');
        if (parts.length > 2) displayMessage = parts.slice(2).join(':').trim();
    }
    
    const toast = document.createElement('div');
    toast.className = `p-4 rounded-md shadow-md text-white ${
        type === 'success' ? 'bg-green-500' : 'bg-red-500'
    } mb-2 transition-all duration-300`;
    toast.innerHTML = `
        <div class="flex items-center">
            <i class="fas fa-${type === 'success' ? 'check' : 'exclamation'}-circle mr-3"></i>
            <span>${displayMessage}</span>
            <button class="ml-4 text-white hover:text-gray-200 close-toast">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
    
    toast.querySelector('.close-toast').addEventListener('click', () => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    });
}

// Функции для статусов задач
export function getTaskStatusClass(status) {
    const statusClasses = {
        0: 'bg-gray-100 text-gray-800',    // To Do
        1: 'bg-blue-100 text-blue-800',    // In Progress
        2: 'bg-green-100 text-green-800',  // Completed
        3: 'bg-red-100 text-red-800'       // Cancelled
    };
    return statusClasses[status] || 'bg-gray-100 text-gray-800';
}

export function getTaskStatusText(status) {
    const texts = {
        0: 'To Do',
        1: 'In Progress',
        2: 'Completed',
        3: 'Cancelled'
    };
    return texts[status] || 'Unknown';
}

// Функции для статусов проектов
export function getStatusClass(status) {
    status = parseInt(status, 10); // Ensure status is number
    const statusClasses = {
        0: 'bg-gray-100 text-gray-800',    // Draft
        1: 'bg-blue-100 text-blue-800',    // Active
        2: 'bg-green-100 text-green-800',  // Completed
        3: 'bg-yellow-100 text-yellow-800' // Archived
    };
    return statusClasses[status] || 'bg-gray-100 text-gray-800';
}

export function getStatusText(status) {
    status = parseInt(status, 10); // Ensure status is number
    const statusTexts = {
        0: 'Draft',
        1: 'Active',
        2: 'Completed',
        3: 'Archived'
    };
    return statusTexts[status] || 'Unknown';
}

export function getStatusHint(status) {
    const hints = {
        0: 'You can add milestones and tags in draft status',
        1: 'Project is active and accepting applications',
        2: 'Project is completed, read-only mode',
        3: 'Project is archived, read-only mode'
    };
    return hints[status] || '';
}

// Функции для приоритетов задач
export function getPriorityClass(priority) {
    const classes = {
        0: 'bg-gray-100 text-gray-800',    // Low
        1: 'bg-blue-100 text-blue-800',    // Medium
        2: 'bg-yellow-100 text-yellow-800',// High
        3: 'bg-red-100 text-red-800'       // Urgent
    };
    return classes[priority] || 'bg-gray-100 text-gray-800';
}

export function getPriorityText(priority) {
    const texts = {
        0: 'Low',
        1: 'Medium',
        2: 'High',
        3: 'Urgent'
    };
    return texts[priority] || 'Unknown';
}

// Общие вспомогательные функции
export function formatCategory(category) {
    if (!category) return '';
    // Преобразуем "machine-learning" в "Machine Learning"
    return category.split('-').map(word => 
        word.charAt(0).toUpperCase() + word.slice(1)
    ).join(' ');
}

export function formatBudget(project) {
    if (!project.currency) project.currency = 'USD';
    if (project.budgetMin && project.budgetMax) {
        return `${formatCurrency(project.budgetMin, project.currency)} - ${formatCurrency(project.budgetMax, project.currency)}`;
    }
    return 'Budget not set';
}

export function formatDate(dateString) {
    if (!dateString) return '-';
    
    try {
        const date = new Date(dateString);
        if (isNaN(date.getTime())) return '-';
        
        const options = { year: 'numeric', month: 'short', day: 'numeric' };
        return date.toLocaleDateString(undefined, options);
    } catch {
        return '-';
    }
}

export function getDaysLeft(endDate) {
    if (!endDate) return 'No due date';
    
    try {
        const today = new Date();
        today.setHours(0, 0, 0, 0); // Сбрасываем время для точного расчета дней
        
        let dueDate;
        if (typeof endDate === 'string') {
            // Try to parse ISO format or YYYY-MM-DD
            dueDate = new Date(endDate.replace(' г.', '').trim()); // Remove ' г.' if present
            if (isNaN(dueDate.getTime())) {
                // Try Russian date format: 31 окт. 2025
                const months = {
                    'янв': 0, 'фев': 1, 'мар': 2, 'апр': 3, 'май': 4, 'июн': 5,
                    'июл': 6, 'авг': 7, 'сен': 8, 'окт': 9, 'ноя': 10, 'дек': 11
                };
                const parts = endDate.match(/(\d+)\s+(\w+)\.\s+(\d+)/);
                if (parts) {
                    const day = parseInt(parts[1]);
                    const month = months[parts[2].toLowerCase().slice(0,3)];
                    const year = parseInt(parts[3]);
                    if (!isNaN(month)) {
                        dueDate = new Date(year, month, day);
                    }
                }
            }
        } else {
            dueDate = new Date(endDate);
        }
        
        if (isNaN(dueDate.getTime())) return 'Invalid date';
        
        dueDate.setHours(0, 0, 0, 0);
        
        const diffTime = dueDate - today;
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        
        if (diffDays < 0) return `${Math.abs(diffDays)} days overdue`;
        if (diffDays === 0) return 'Today';
        return `${diffDays} days left`;
    } catch (error) {
        console.error('Error in getDaysLeft:', error);
        return 'Error calculating';
    }
}

// Добавим новую функцию для получения количества дней в числовом формате
export function getDaysLeftNumber(endDate) {
    if (!endDate) return null;
    
    try {
        const today = new Date();
        const dueDate = new Date(endDate);
        
        if (isNaN(dueDate.getTime())) return null;
        
        const diffTime = dueDate - today;
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    } catch {
        return null;
    }
}


export function formatDateForInput(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return '';
    
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

export function getFileIconClass(fileName) {
    if (!fileName) return 'fa-file text-gray-500';
    
    const ext = fileName.split('.').pop().toLowerCase();
    switch (ext) {
        case 'pdf': return 'fa-file-pdf text-red-500';
        case 'doc': case 'docx': return 'fa-file-word text-blue-500';
        case 'xls': case 'xlsx': return 'fa-file-excel text-green-500';
        case 'jpg': case 'jpeg': case 'png': case 'gif': return 'fa-file-image text-yellow-500';
        case 'zip': case 'rar': return 'fa-file-archive text-purple-500';
        default: return 'fa-file text-gray-500';
    }
}

export function formatFileSize(bytes) {
    if (!bytes) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

export function formatActivityTime(timestamp) {
    const now = new Date();
    const diffMs = now - new Date(timestamp);
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minutes ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;
    
    return formatDate(timestamp);
}