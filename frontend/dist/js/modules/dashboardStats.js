// modules/dashboardStats.js
export function updateDashboardStats(projects) {
    const now = new Date();
    
    const totalEl = document.getElementById('totalProjects');
    const activeEl = document.getElementById('activeProjects');
    const completedEl = document.getElementById('completedProjects');
    const archivedEl = document.getElementById('archivedProjects');
    const deadlinesEl = document.getElementById('upcomingDeadlines');
    
    if (totalEl) totalEl.textContent = projects.length;
    if (activeEl) activeEl.textContent = projects.filter(p => p.status === 1).length;
    if (completedEl) completedEl.textContent = projects.filter(p => p.status === 2).length;
    if (archivedEl) archivedEl.textContent = projects.filter(p => p.status === 3).length;
    
    if (deadlinesEl) {
        deadlinesEl.textContent = projects.filter(p => {
            if (!p.expiresAt || p.status !== 1) return false;
            const diff = Math.ceil((new Date(p.expiresAt) - now) / 86400000);
            return diff > 0 && diff <= 7;
        }).length;
    }
}