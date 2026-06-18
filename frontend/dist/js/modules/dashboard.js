import { TaskAPI } from '../api.js';
import { loadProjects } from './projects.js';
import { initCharts } from './charts.js';
import { setupDashboardFilters } from './dashboardFilters.js';
import { updateDashboardStats } from './dashboardStats.js';

let dashboardProjects = [];
let dashboardTasks = [];
let chartFilters = { statusRange: '30', deadlineRange: '30' };

function normalizeTasksResponse(response) {
    if (!response) return [];
    if (Array.isArray(response)) return response;
    if (Array.isArray(response.items)) return response.items;
    return [];
}

function renderCharts() {
    initCharts(dashboardProjects, dashboardTasks, chartFilters);
}

export async function initDashboard() {
    const projectsPromise = loadProjects({ page: 1, pageSize: 6, includeMilestones: false });
    const tasksPromise = TaskAPI.getTasks({ actualPage: 1, ItemsPerPage: 50 });

    const projectsResult = await projectsPromise;
    dashboardProjects = projectsResult.items ? projectsResult.items : projectsResult;

    try {
        const tasksResult = await tasksPromise;
        dashboardTasks = normalizeTasksResponse(tasksResult);
    } catch (error) {
        console.warn('Не удалось загрузить задачи для графика сроков:', error);
        dashboardTasks = [];
    }

    updateDashboardStats(dashboardProjects);
    renderCharts();

    setupDashboardFilters((nextFilters) => {
        chartFilters = nextFilters;
        renderCharts();
    });
}
