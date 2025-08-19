import { AuthAPI, UserAPI } from '../api.js';
import { showToast } from './ui.js';

// Состояние аутентификации
let currentUser = null;

export async function initAuth() {
    await checkAuth();
}

// Проверка аутентификации при загрузке
export async function checkAuth() {
    const token = localStorage.getItem('token');
    if (!token) return false;

    try {
        const user = await UserAPI.getProfile();
        currentUser = user;
        updateUI();
        return true;
    } catch (error) {
        console.error('checkAuth failed:', error);
        // Удаляем недействительный токен
        localStorage.removeItem('token');
        currentUser = null;
        return false;
    }
}

// Обновление UI в зависимости от состояния аутентификации
function updateUI() {
    const userMenu = document.getElementById('user-menu');
    if (userMenu) {
        if (currentUser) {
            userMenu.innerHTML = `
                <span class="text-gray-700">${currentUser.name}</span>
                ${currentUser.avatarUrl ? `<img class="h-8 w-8 rounded-full" src="${currentUser.avatarUrl}" alt="User">` : ''}
            `;
        } else {
            userMenu.innerHTML = `
                <a href="/login" class="text-gray-700 hover:text-blue-600">Login</a>
            `;
        }
    }
}

// Логин - ИСПРАВЛЕННЫЙ МЕТОД
export async function login(email, password) {
    try {
        // Исправляем структуру запроса
        const response = await AuthAPI.login({ 
            Login: email, // Изменяем поле email на Login
            Password: password 
        });
        
        if (!response.token) {
            throw new Error('Token not found in response');
        }
        
        localStorage.setItem('token', response.token);
        currentUser = await UserAPI.getProfile();
        updateUI();
        return true;
    } catch (error) {
        console.error('Login failed:', error);
        showToast('Login failed. Please check your credentials.', 'error');
        return false;
    }
}

// Регистрация
export async function register(userData) {
    try {
        await AuthAPI.register(userData);
        return login(userData.email, userData.password);
    } catch (error) {
        console.error('Registration failed:', error);
        showToast('Registration failed. Please try again.', 'error');
        return false;
    }
}

export function getCurrentUser() {
    return currentUser;
}