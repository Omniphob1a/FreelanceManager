let statusChart = null;
let deadlineChart = null;

function toDate(value) {
    if (!value) return null;
    const d = new Date(value);
    return Number.isNaN(d.getTime()) ? null : d;
}

function withinDays(date, days) {
    if (!date || days === 'all') return true;
    const rangeDays = Number(days);
    if (!Number.isFinite(rangeDays) || rangeDays <= 0) return true;
    const now = new Date();
    const from = new Date(now);
    from.setHours(0, 0, 0, 0);
    from.setDate(from.getDate() - rangeDays);
    return date >= from && date <= now;
}

function filterProjectsByRange(projects, range) {
    if (range === 'all') return projects;
    return projects.filter((project) => {
        const date = toDate(project.createdAt || project.modifiedOn || project.expiresAt);
        if (!date) return true;
        return withinDays(date, range);
    });
}

function filterTasksByRange(tasks, range) {
    if (range === 'all') return tasks;
    return tasks.filter((task) => {
        const date = toDate(task.dueDate || task.createdAt || task.modifiedOn);
        if (!date) return true;
        return withinDays(date, range);
    });
}

export function initCharts(projects, tasks, filters = { statusRange: '30', deadlineRange: '30' }) {
    if (typeof Chart === 'undefined') {
        console.warn('[charts] Chart.js не загружен');
        return;
    }

    const statusEl = document.getElementById('statusChart');
    const deadlineEl = document.getElementById('deadlineChart');
    if (!statusEl || !deadlineEl) return;

    if (statusChart) {
        statusChart.destroy();
        statusChart = null;
    }
    if (deadlineChart) {
        deadlineChart.destroy();
        deadlineChart = null;
    }

    const projectList = filterProjectsByRange(Array.isArray(projects) ? projects : [], filters.statusRange);
    const taskList = filterTasksByRange(Array.isArray(tasks) ? tasks : [], filters.deadlineRange);

    const statusCounts = { draft: 0, active: 0, completed: 0, archived: 0 };
    projectList.forEach((project) => {
        switch (project.status) {
            case 0: statusCounts.draft += 1; break;
            case 1: statusCounts.active += 1; break;
            case 2: statusCounts.completed += 1; break;
            case 3: statusCounts.archived += 1; break;
            default: break;
        }
    });

    const statusCtx = statusEl.getContext('2d');
    statusChart = new Chart(statusCtx, {
        type: 'doughnut',
        data: {
            labels: ['Черновик', 'Активный', 'Завершен', 'Архив'],
            datasets: [{
                data: [
                    statusCounts.draft,
                    statusCounts.active,
                    statusCounts.completed,
                    statusCounts.archived
                ],
                backgroundColor: ['#e2e8f0', '#93c5fd', '#86efac', '#fcd34d'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'right' }
            }
        }
    });

    const now = new Date();
    now.setHours(0, 0, 0, 0);
    const deadlineCounts = { onTime: 0, overdue: 0, today: 0 };

    taskList.forEach((task) => {
        const dueDate = toDate(task.dueDate);
        if (!dueDate) return;

        const due = new Date(dueDate);
        due.setHours(0, 0, 0, 0);
        const diffDays = Math.ceil((due - now) / 86400000);

        if (diffDays < 0) deadlineCounts.overdue += 1;
        else if (diffDays === 0) deadlineCounts.today += 1;
        else deadlineCounts.onTime += 1;
    });

    const deadlineCtx = deadlineEl.getContext('2d');
    deadlineChart = new Chart(deadlineCtx, {
        type: 'bar',
        data: {
            labels: ['В срок', 'Просрочено', 'На сегодня'],
            datasets: [{
                label: 'Задачи',
                data: [deadlineCounts.onTime, deadlineCounts.overdue, deadlineCounts.today],
                backgroundColor: ['#10b981', '#ef4444', '#f59e0b'],
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { precision: 0 }
                },
                x: {
                    grid: { display: false }
                }
            },
            plugins: {
                legend: { display: false }
            }
        }
    });
}
