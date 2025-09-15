
// charts.js - полностью обновленный файл
let statusChart = null;
let revenueChart = null;

// Инициализация графиков
export function initCharts(projects) {
    // Уничтожаем предыдущие графики, если есть
    if (statusChart) {
        statusChart.destroy();
        statusChart = null;
    }
    if (revenueChart) {
        revenueChart.destroy();
        revenueChart = null;
    }
    
    // Подсчет проектов по статусам
    const statusCounts = {
        draft: 0,
        active: 0,
        completed: 0,
        archived: 0
    };
    
    projects.forEach(project => {
        switch(project.status) {
            case 0: statusCounts.draft++; break;
            case 1: statusCounts.active++; break;
            case 2: statusCounts.completed++; break;
            case 3: statusCounts.archived++; break;
        }
    });
    
    // Инициализация графика статусов проектов
    const statusCtx = document.getElementById('statusChart').getContext('2d');
    statusChart = new Chart(statusCtx, {
        type: 'doughnut',
        data: {
            labels: ['Draft', 'Active', 'Completed', 'Archived'],
            datasets: [{
                data: [
                    statusCounts.draft,
                    statusCounts.active,
                    statusCounts.completed,
                    statusCounts.archived
                ],
                backgroundColor: [
                    '#e2e8f0',
                    '#d1fae5',
                    '#bfdbfe',
                    '#f0fdf4',
                    '#f5f5f4'
                ],
                borderColor: [
                    '#cbd5e1',
                    '#a7f3d0',
                    '#93c5fd',
                    '#dcfce7',
                    '#e7e5e4'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'right',
                }
            }
        }
    });

    // Инициализация графика доходов (заглушка)
    const revenueCtx = document.getElementById('revenueChart').getContext('2d');
    revenueChart = new Chart(revenueCtx, {
        type: 'bar',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
            datasets: [{
                label: 'Revenue ($)',
                data: [3200, 2800, 4200, 3800, 5200, 4800],
                backgroundColor: '#3b82f6',
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        drawBorder: false
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
}