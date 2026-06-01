import '../css/styles.css';
import '@fortawesome/fontawesome-free/css/all.min.css';
import { initAuth } from './modules/auth.js';
import { initRouting } from './modules/routing.js';
import { setupEventListeners } from './modules/eventListeners.js';
import { initLocalization } from './modules/localization.js';

document.addEventListener('DOMContentLoaded', async () => {
    setupEventListeners();
    await initAuth();
    await initRouting();
    initLocalization();
    console.log('Инициализация приложения завершена');
});
