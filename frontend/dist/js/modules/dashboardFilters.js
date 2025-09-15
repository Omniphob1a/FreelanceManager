// modules/dashboardFilters.js
import { loadProjects } from './projects.js';
import { updateDashboardStats } from './dashboardStats.js';
import { initCharts } from './charts.js';

export function setupDashboardFilters() {
    const statusFilter = document.getElementById('statusFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const dateRangeFilter = document.getElementById('dateRangeFilter');
    
    const applyFilters = async () => {
        const filters = {};
        
        if (statusFilter.value !== 'all') {
            filters.status = statusFilter.value;
        }
        
        if (categoryFilter.value !== 'all') {
            filters.category = categoryFilter.value;
        }
        
        if (dateRangeFilter.value !== 'all') {
            let createdLastDays;
            switch(dateRangeFilter.value) {
                case 'week': createdLastDays = 7; break;
                case 'month': createdLastDays = 30; break;
                case 'quarter': createdLastDays = 90; break;
                default: createdLastDays = null;
            }
            if (createdLastDays) {
                filters.createdLastDays = createdLastDays;
            }
        }
        
        filters.limit = 6;
        
        try {
            const projects = await loadProjects(filters);
            updateDashboardStats(projects);
            initCharts(projects);
        } catch (error) {
            console.error('Failed to apply filters:', error);
        }
    };
    
    statusFilter.addEventListener('change', applyFilters);
    categoryFilter.addEventListener('change', applyFilters);
    dateRangeFilter.addEventListener('change', applyFilters);
}