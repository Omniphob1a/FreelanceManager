import { loadPage } from './routing.js';

export function setupEventListeners() {
    // Исправляем селектор на .nav-item
    document.querySelectorAll('.nav-item').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const page = this.getAttribute('data-page');
            console.log(`Loading page: ${page}`); // Для отладки
            loadPage(page);
        });
    });

    // Глобальный обработчик для кнопки создания проекта
    document.addEventListener('click', function(e) {
        if (e.target.closest('#newProjectBtn')) {
            e.preventDefault();
            loadPage('project-form');
        }
    });
    
    console.log('Event listeners setup complete'); // Для отладки
}