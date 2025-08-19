// modules/ui.js
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

export function getStatusClass(status) {
    const statusClasses = {
        0: 'bg-gray-100 text-gray-800',
        1: 'bg-blue-100 text-blue-800',
        2: 'bg-green-100 text-green-800',
        3: 'bg-yellow-100 text-yellow-800'
    };
    return statusClasses[status] || 'bg-gray-100 text-gray-800';
}

export function formatCategory(category) {
    if (!category) return '';
    // Преобразуем "machine-learning" в "Machine Learning"
    return category.split('-').map(word => 
        word.charAt(0).toUpperCase() + word.slice(1)
    ).join(' ');
}
export function getStatusText(status) {
    const statusTexts = {
        0: 'Draft',
        1: 'Active',
        2: 'Completed',
        3: 'Archived'
    };
    return statusTexts[status] || 'Draft';
}

export function getStatusHint(status) {
    const hints = {
        0: 'You can add milestones and tags in draft status',
        1: 'Project is published and accepting applications',
        2: 'Project is in progress, no structural changes allowed',
        3: 'Project is completed, read-only mode',
        4: 'Project is archived, read-only mode'
    };
    return hints[status] || '';
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
    const dueDate = new Date(endDate);
    
    if (isNaN(dueDate.getTime())) return 'Invalid date';
    
    const diffTime = dueDate - today;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays < 0) return `${Math.abs(diffDays)} days overdue`;
    if (diffDays === 0) return 'Today';
    return `${diffDays} days left`;
  } catch {
    return 'Error calculating';
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