// app.js - обновленный файл
import { initAuth } from './modules/auth.js';
import { initRouting } from './modules/routing.js';
import { initNotifications } from './modules/notifications.js';
import { setupEventListeners } from './modules/eventListeners.js';
import { initCharts } from './modules/charts.js'; // Добавить импорт

document.addEventListener('DOMContentLoaded', async () => {
    // Сначала устанавливаем обработчики событий
    setupEventListeners();
    
    // Инициализируем аутентификацию
    await initAuth();
    
    // Инициализируем маршрутизацию
    await initRouting();
    
    // Инициализируем уведомления
    await initNotifications();
    
    console.log('App initialization complete'); 
});