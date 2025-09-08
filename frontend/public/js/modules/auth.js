// auth.js
import { AuthAPI, UserAPI } from '../api.js';
import { showToast } from './ui.js';

// Состояние аутентификации
let currentUser = null;
let __submitDelegationAttached = false;
let __mutationObserver = null;

/**
 * Инициализация: проверяем токен и вешаем обработчики.
 * Вызывать после импорта (можно несколько раз — безопасно).
 */
export async function initAuth() {
    await checkAuth();
    ensureSubmitDelegation();   // делегирование сабмита (работает при любом порядке загрузки)
    ensureMutationObserver();   // опционально — ловим появление формы и логируем
}

/* ----------------- Делегирование submit (универсально) ----------------- */

/**
 * Делегируем submit на document — безопасно и работает для динамически вставленных форм.
 * Обработчик проверяет, что target имеет id="loginForm".
 */
function ensureSubmitDelegation() {
    if (__submitDelegationAttached) return;
    __submitDelegationAttached = true;

    document.addEventListener('submit', async (e) => {
        // если это не форма логина — игнорируем
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;
        if (form.id !== 'loginForm') return;

        e.preventDefault();
        try {
            const formData = new FormData(form);
            const username = formData.get('username')?.toString().trim() ?? '';
            const password = formData.get('password')?.toString() ?? '';

            if (!username || !password) {
                showToast('Please enter username and password.', 'error');
                return;
            }

            const success = await login(username, password);
            if (success) {
                showToast('Login successful!', 'success');
                setTimeout(() => window.location.href = '/', 300);
            }
        } catch (err) {
            console.error('Login submit handler error:', err);
            showToast(`Login error: ${err?.message ?? err}`, 'error');
        }
    }, true); // useCapture true — можно поставить false, но capture защищает от перехвата в некоторых SPA
}

/* ----------------- MutationObserver (опционально, для логов/отладки) ----------------- */

/**
 * Наблюдает DOM и логирует, когда форма появилась. Можно расширить, чтобы навешивать
 * локальные обработчики на саму форму — не обязательно, т.к. делегирование уже работает.
 */
function ensureMutationObserver() {
    if (typeof MutationObserver === 'undefined') return;

    if (__mutationObserver) return;

    __mutationObserver = new MutationObserver((mutations) => {
        for (const m of mutations) {
            for (const node of m.addedNodes) {
                if (!(node instanceof Element)) continue;
                // если добавлена сама форма
                if (node.id === 'loginForm') {
                    console.debug('[auth] MutationObserver: #loginForm was added to DOM');
                    return;
                }
                // или форма внутри добавленного элемента
                const found = node.querySelector && node.querySelector('#loginForm');
                if (found) {
                    console.debug('[auth] MutationObserver: #loginForm found inside added subtree');
                    return;
                }
            }
        }
    });

    __mutationObserver.observe(document.documentElement || document.body, {
        childList: true, subtree: true,
    });

    // На случай если форма уже в DOM — проверим сразу:
    if (document.getElementById('loginForm')) {
        console.debug('[auth] MutationObserver: #loginForm present at init');
    } else {
        console.debug('[auth] MutationObserver: #loginForm not present at init');
    }
}

/* ----------------- Auth logic ----------------- */

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
        localStorage.removeItem('token');
        currentUser = null;
        updateUI();
        return false;
    }
}

function updateUI() {
    const userMenu = document.getElementById('user-menu');
    if (!userMenu) return;

    if (currentUser) {
        userMenu.innerHTML = `
            <span class="text-gray-700">${escapeHtml(currentUser.name || currentUser.username || '')}</span>
            ${currentUser.avatarUrl ? `<img class="h-8 w-8 rounded-full ml-2" src="${escapeHtml(currentUser.avatarUrl)}" alt="User">` : ''}
            <button id="logoutBtn" class="ml-3 text-sm text-red-600 hover:underline">Logout</button>
        `;
        const logoutBtn = document.getElementById('logoutBtn');
        if (logoutBtn) logoutBtn.addEventListener('click', logout);
    } else {
        userMenu.innerHTML = `<a href="/login" class="text-gray-700 hover:text-blue-600">Login</a>`;
    }
}

export async function login(username, password) {
    try {
        const response = await AuthAPI.login({
            Login: username,
            Password: password,
        });

        if (!response || !response.token) {
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

export async function register(userData) {
    try {
        await AuthAPI.register(userData);
        return login(userData.username || userData.email, userData.password);
    } catch (error) {
        console.error('Registration failed:', error);
        showToast('Registration failed. Please try again.', 'error');
        return false;
    }
}

export function logout() {
    try {
        localStorage.removeItem('token');
        currentUser = null;
        updateUI();
        showToast('Logged out', 'info');
        setTimeout(() => {
            if (window.location.pathname !== '/') window.location.href = '/';
        }, 250);
    } catch (err) {
        console.error('Logout error:', err);
    }
}

export function getCurrentUser() {
    return currentUser;
}

/* ----------------- Helpers ----------------- */

function escapeHtml(s) {
    if (!s) return '';
    return String(s)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
