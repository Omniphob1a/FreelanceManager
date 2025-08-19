// modules/dashboard.js
import { ActivityAPI } from '../api.js';
import { formatDate, formatActivityTime } from './ui.js';
import { loadProjects } from './projects.js';
import { initCharts } from './charts.js';
import { setupDashboardFilters } from './dashboardFilters.js';
import { updateDashboardStats } from './dashboardStats.js';

export async function initDashboard() {
    await loadActivities();
     const projectsResult = await loadProjects({ 
        page: 1, 
        pageSize: 6 
    });
    const projects = projectsResult.items ? projectsResult.items : projectsResult;

    updateDashboardStats(projects);
    initCharts(projects);
}

async function loadActivities() {
    const container = document.getElementById('activityList');
    if (!container) return;
    
    try {
        const activities = await ActivityAPI.getRecentActivities();
        renderActivities(activities);
    } catch (error) {
        console.error('Failed to load activities:', error);
        container.innerHTML = '<div class="p-4 text-center text-gray-500">Failed to load activities</div>';
    }
}

function renderActivities(activities) {
    const container = document.getElementById('activityList');
    if (!container) return;
    
    if (activities.length === 0) {
        container.innerHTML = '<div class="p-4 text-center text-gray-500">No recent activity</div>';
        return;
    }
    
    container.innerHTML = activities.map(activity => {
        let content = '';
        const timeAgo = formatActivityTime(activity.timestamp);
        
        switch(activity.type) {
            case 'completed':
                content = `
                    <p class="text-sm font-medium text-gray-900">Project completed</p>
                    <p class="text-sm text-gray-500">You marked "${activity.project}" as completed</p>
                `;
                break;
            case 'comment':
                content = `
                    <p class="text-sm font-medium text-gray-900">New comment</p>
                    <p class="text-sm text-gray-500">${activity.user} commented on "${activity.project}"</p>
                `;
                break;
            default:
                content = `
                    <p class="text-sm font-medium text-gray-900">New activity</p>
                    <p class="text-sm text-gray-500">${activity.description}</p>
                `;
        }
        
        return `
            <div class="p-4 hover:bg-gray-50">
                <div class="flex items-start">
                    <div class="flex-shrink-0">
                        <div class="w-10 h-10 rounded-full ${
                            activity.type === 'completed' ? 'bg-green-100' : 'bg-blue-100'
                        } flex items-center justify-center">
                            <i class="fas ${
                                activity.type === 'completed' ? 'fa-check text-green-600' : 'fa-comment text-blue-600'
                            }"></i>
                        </div>
                    </div>
                    <div class="ml-4">
                        ${content}
                        <p class="text-xs text-gray-400 mt-1">${timeAgo}</p>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}