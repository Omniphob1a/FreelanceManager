export function setupEventListeners() {
    document.addEventListener('click', function (e) {
        const pageLink = e.target.closest('[data-page]');
        if (!pageLink) return;
        e.preventDefault();
        const page = pageLink.getAttribute('data-page');
        if (!page) return;
        window.location.hash = page;
    });

    document.addEventListener('click', function (e) {
        if (e.target.closest('#newProjectBtn')) {
            e.preventDefault();
            window.location.hash = 'project-form';
        }
    });
}