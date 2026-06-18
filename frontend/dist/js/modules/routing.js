// modules/routing.js
import { checkAuth, getCurrentUser, login, register } from './auth.js';
import { loadProjects, loadProjectsPage, initProjectsPage } from './projects.js';
import { initProjectForm } from './projectForm.js';
import { initTasksPage } from './tasks.js';
import { initNotificationsPage } from './notifications.js';
import { initProfilePage } from './profile.js';
import { showToast } from './ui.js';
import { initDashboard } from './dashboard.js';

const protectedPages = ['dashboard', 'projects', 'project-form', 'tasks', 'notifications', 'profile'];

export async function initRouting() {
    window.addEventListener('hashchange', handleHashChange);
    await handleHashChange();
}

async function handleHashChange() {
    let pageName = window.location.hash.substring(1) || 'dashboard';
    const [basePage, query] = pageName.split('?');
    
    if (protectedPages.includes(basePage) && !(await checkAuth())) {
        pageName = 'login';
        window.location.hash = 'login';
    }
    
    await loadPage(basePage, query);
}

export async function loadPage(pageName, queryString = '') {
    try {
        const basePage = pageName.split('?')[0];
        const response = await fetch(`partials/${basePage}.html`);
        if (!response.ok) throw new Error(`Failed to load ${pageName}.html: ${response.status}`);
        
        const content = await response.text();
        document.getElementById('content').innerHTML = content;
        
        document.getElementById('pageTitle').textContent = 
            basePage.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
            
        document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('bg-blue-600', 'text-white');
        item.classList.add('text-gray-700', 'hover:bg-gray-100');
    });
    
    const activeNavItem = document.querySelector(`.nav-item[data-page="${basePage}"]`);
    if (activeNavItem) {
        activeNavItem.classList.add('bg-blue-600', 'text-white');
        activeNavItem.classList.remove('text-gray-700', 'hover:bg-gray-100');
    }
        switch (basePage) {
            case 'dashboard': 
                await initDashboard();
                break;
            case 'projects': 
                await loadProjectsPage();
                initProjectsPage();
                break;
            case 'project-form': 
                await initProjectForm(queryString);
                break;
            case 'login': 
                initLoginPage();
                break;
            case 'register': 
                initRegisterPage();
                break;
            case 'tasks': 
                await initTasksPage();
                break;
            case 'notifications': 
                await initNotificationsPage();
                break;
            case 'profile': 
                await initProfilePage();
                break;
            default:
                throw new Error(`Unknown page: ${pageName}`);
        }
    } catch (error) {
        console.error('Error loading page:', error);
        document.getElementById('content').innerHTML = `
            <div class="bg-white rounded-lg shadow p-6 text-center">
                <h2 class="text-xl font-semibold text-red-600 mb-4">Error Loading Content</h2>
                <p class="text-gray-600 mb-4">${error.message}</p>
                <button onclick="window.location.hash='dashboard'" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
                    Go to Dashboard
                </button>
            </div>`;
    }
}

function initLoginPage() {
    const registerLink = document.querySelector('[data-page="register"]');
    if (registerLink) {
        registerLink.addEventListener('click', (e) => {
            e.preventDefault();
            window.location.hash = 'register';
        });
    }
}

function loginFromText(text) {
    const local = String(text || 'user').replace(/[^A-Za-z0-9]/g, '');
    return local.length ? local : 'user';
}

function initRegisterPage() {
    const form = document.getElementById('registerForm');
    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            if (form.password.value !== form.confirmPassword.value) {
                showToast('Passwords do not match', 'error');
                return;
            }

            const email = form.email.value.trim();
            const birthday = form.birthday.value
                ? new Date(`${form.birthday.value}T12:00:00`).toISOString()
                : new Date('1990-01-01T00:00:00Z').toISOString();
            
            const userData = {
                login: loginFromText(form.fullName.value),
                password: form.password.value,
                name: form.fullName.value.trim(),
                gender: parseInt(form.gender.value, 10),
                birthday,
                email,
                isAdmin: false,
                createdBy: 'self-registration'
            };
            
            try {
                const success = await register(userData);
                if (success) window.location.hash = 'dashboard';
                else showToast('Registration failed. Please try again.', 'error');
            } catch (error) {
                showToast('Registration error: ' + error.message, 'error');
            }
        });
    }
}
