// auth.js
import { AuthAPI, UserAPI } from '../api.js';
import { showToast } from './ui.js';

let currentUser = null;
/** Время успешного ответа getProfile; снижает лишние запросы при каждом hashchange. */
let profileFetchedAt = 0;
const PROFILE_CACHE_MS = 90_000;

let __submitDelegationAttached = false;
let __mutationObserver = null;
let __shellUiBound = false;

function subtitleForUser(user) {
    if (!user) return 'Не выполнен вход';
    if (user.roles?.length) return user.roles.join(', ');
    if (user.admin) return 'Администратор';
    return 'Пользователь';
}

function loginFromEmail(email) {
    const local = (email.split('@')[0] || 'user').replace(/[^A-Za-z0-9]/g, '');
    return local.length ? local : 'user';
}

export async function initAuth() {
    await checkAuth();
    ensureSubmitDelegation();
    ensureMutationObserver();
    ensureShellUi();
}

function ensureShellUi() {
    if (__shellUiBound) return;
    __shellUiBound = true;

    document.addEventListener('click', (e) => {
        if (e.target.closest('#appLogoutBtn')) {
            e.preventDefault();
            logout();
        }
    });

    const headerBtn = document.getElementById('userMenuButton');
    if (headerBtn && !headerBtn.dataset.navProfile) {
        headerBtn.dataset.navProfile = '1';
        headerBtn.addEventListener('click', () => {
            window.location.hash = 'profile';
        });
    }
}

function ensureSubmitDelegation() {
    if (__submitDelegationAttached) return;
    __submitDelegationAttached = true;

    document.addEventListener('submit', async (e) => {
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;
        if (form.id !== 'loginForm') return;

        e.preventDefault();
        try {
            const formData = new FormData(form);
            const username = formData.get('username')?.toString().trim() ?? '';
            const password = formData.get('password')?.toString() ?? '';

            if (!username || !password) {
                showToast('Введите логин и пароль.', 'error');
                return;
            }

            const success = await login(username, password);
            if (success) {
                showToast('Вход выполнен.', 'success');
                setTimeout(() => { window.location.href = '/'; }, 300);
            }
        } catch (err) {
            console.error('Login submit handler error:', err);
            showToast(`Ошибка входа: ${err?.message ?? err}`, 'error');
        }
    }, true);
}

function ensureMutationObserver() {
    if (typeof MutationObserver === 'undefined') return;
    if (__mutationObserver) return;

    __mutationObserver = new MutationObserver((mutations) => {
        for (const m of mutations) {
            for (const node of m.addedNodes) {
                if (!(node instanceof Element)) continue;
                if (node.id === 'loginForm') return;
                const found = node.querySelector && node.querySelector('#loginForm');
                if (found) return;
            }
        }
    });

    __mutationObserver.observe(document.documentElement || document.body, {
        childList: true, subtree: true,
    });
}

/**
 * @param {boolean} forceRefresh — игнорировать кэш профиля (после смены данных в профиле).
 */
export async function checkAuth(forceRefresh = false) {
    const token = localStorage.getItem('token');
    if (!token) {
        currentUser = null;
        profileFetchedAt = 0;
        updateUI();
        return false;
    }

    const now = Date.now();
    if (!forceRefresh && currentUser && now - profileFetchedAt < PROFILE_CACHE_MS) {
        return true;
    }

    try {
        const user = await UserAPI.getProfile();
        currentUser = user;
        profileFetchedAt = now;
        updateUI();
        return !!user;
    } catch (error) {
        const status = error.status ?? 0;
        console.warn('checkAuth: profile request failed', status, error.message);

        if (status === 401 || status === 403) {
            localStorage.removeItem('token');
            currentUser = null;
            profileFetchedAt = 0;
            updateUI();
            return false;
        }

        // Таймаут, сеть, 5xx: не удаляем токен (иначе «вылет» из аккаунта при лагах gateway).
        if (currentUser) {
            updateUI();
            return true;
        }

        updateUI();
        return false;
    }
}

function updateUI() {
    const sn = document.getElementById('sidebarUserName');
    const ss = document.getElementById('sidebarUserSubtitle');
    const sb = document.getElementById('sidebarUserBadge');
    const hb = document.getElementById('headerUserBadge');
    const logoutEl = document.getElementById('appLogoutBtn');

    if (currentUser) {
        const name = currentUser.name || currentUser.login || 'Пользователь';
        if (sn) sn.textContent = name;
        if (ss) ss.textContent = subtitleForUser(currentUser);
        if (sb) sb.innerHTML = '<i class="fas fa-user" aria-hidden="true"></i>';
        if (hb) hb.innerHTML = '<i class="fas fa-user" aria-hidden="true"></i>';
        if (logoutEl) logoutEl.classList.remove('hidden');
    } else {
        if (sn) sn.textContent = 'Гость';
        if (ss) ss.textContent = 'Не выполнен вход';
        if (sb) sb.innerHTML = '<i class="fas fa-user" aria-hidden="true"></i>';
        if (hb) hb.innerHTML = '<i class="fas fa-user" aria-hidden="true"></i>';
        if (logoutEl) logoutEl.classList.add('hidden');
    }
}

export async function login(username, password) {
    try {
        const normalizedLogin = username.trim();
        const loginCandidates = [normalizedLogin];
        if (normalizedLogin.includes('@')) {
            const generatedLogin = loginFromEmail(normalizedLogin);
            if (!loginCandidates.includes(generatedLogin)) loginCandidates.push(generatedLogin);
        }

        let response = null;
        let lastError = null;
        for (const loginCandidate of loginCandidates) {
            try {
                response = await AuthAPI.login({
                    Login: loginCandidate,
                    Password: password,
                });
                break;
            } catch (error) {
                lastError = error;
            }
        }

        if (!response && lastError) throw lastError;

        if (!response || !response.token) {
            throw new Error('Токен не получен');
        }

        localStorage.setItem('token', response.token);
        currentUser = await UserAPI.getProfile();
        profileFetchedAt = Date.now();
        updateUI();
        return true;
    } catch (error) {
        console.error('Login failed:', error);
        showToast('Не удалось войти. Проверьте логин и пароль.', 'error');
        return false;
    }
}

export async function register(userData) {
    try {
        await AuthAPI.register(userData);
        return login(userData.login || userData.email, userData.password);
    } catch (error) {
        console.error('Registration failed:', error);
        showToast('Не удалось зарегистрироваться. Попробуйте снова.', 'error');
        return false;
    }
}

export function logout() {
    try {
        localStorage.removeItem('token');
        currentUser = null;
        profileFetchedAt = 0;
        updateUI();
        showToast('Вы вышли из аккаунта.', 'info');
        setTimeout(() => {
            if (window.location.pathname !== '/') window.location.href = '/';
            else window.location.hash = 'login';
        }, 250);
    } catch (err) {
        console.error('Logout error:', err);
    }
}

export function getCurrentUser() {
    return currentUser;
}
