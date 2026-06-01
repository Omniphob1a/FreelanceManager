export function setupDashboardFilters(onChange) {
    const statusFilter = document.getElementById('statusChartRange');
    const deadlineFilter = document.getElementById('deadlineChartRange');

    if (!statusFilter || !deadlineFilter || typeof onChange !== 'function') {
        return;
    }

    const emit = () => {
        onChange({
            statusRange: statusFilter.value || '30',
            deadlineRange: deadlineFilter.value || '30'
        });
    };

    statusFilter.addEventListener('change', emit);
    deadlineFilter.addEventListener('change', emit);
}
