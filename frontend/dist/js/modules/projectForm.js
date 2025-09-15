// modules/projectForm.js
import { ProjectAPI } from '../api.js';
import { getCurrentUser } from './auth.js';
import { showToast } from './ui.js';
import { formatDateForInput } from './ui.js';

export async function initProjectForm(queryString = '') {
    const params = new URLSearchParams(queryString);
    const projectId = params.get('id');
    const form = document.getElementById('projectForm');
    const dueDateInput = document.getElementById('dueDate');

    if (typeof flatpickr === 'undefined') {
        await import('https://cdn.jsdelivr.net/npm/flatpickr');
    }

    if (dueDateInput) {
        flatpickr(dueDateInput, {
            dateFormat: "Y-m-d",
            minDate: "today",
            static: true
        });
    }
    
    // Установка ownerId
    const ownerIdInput = document.getElementById('ownerId');
    if (ownerIdInput) {
        try {
            const user = await getCurrentUser();
            ownerIdInput.value = user.id;
        } catch (error) {
            showToast('You must be logged in to create projects', 'error');
            window.location.hash = 'login';
            return;
        }
    }
    
    initTagsHandlers(form);
    
    if (projectId) {
        document.getElementById('projectFormTitle').textContent = 'Edit Project';
        document.getElementById('submitButtonText').textContent = 'Update Project';
        await loadProjectForEditing(projectId);
    }
    
    if (form) {
        form.addEventListener('submit', handleProjectSubmit);
    }
    
    // Кнопка отмены
    document.querySelector('.cancel-btn')?.addEventListener('click', () => {
        window.location.hash = projectId ? 'projects' : 'dashboard';
    });
}

async function handleProjectSubmit(e) {
    e.preventDefault();
    const form = e.target;
    const projectId = form.projectId.value;
    
    try {
        if (!form.title.value.trim()) {
            throw new Error('Project title is required');
        }
        
        if (!form.category.value) {
            throw new Error('Category is required');
        }
        
        const budgetMin = parseFloat(form.budgetMin.value) || 0;
        const budgetMax = parseFloat(form.budgetMax.value) || 0;
        
        if (budgetMin !== null && budgetMax !== null && budgetMin > budgetMax) {
            throw new Error('Max budget should be greater than min budget');
        }

        const projectData = {
            title: form.title.value.trim(),
            description: form.description.value.trim(),
            ownerId: form.ownerId.value,
            budgetMin,
            budgetMax,
            currencyCode: form.currency.value, // ИЗМЕНЕНО: currency -> currencyCode
            category: form.category.value,
            tags: JSON.parse(form.tags.value || '[]')
        };
        
        if (projectId) {
            await ProjectAPI.updateProject(projectId, projectData);
            showToast('Project updated successfully');
        } else {
            await ProjectAPI.createProject(projectData);
            showToast('Project created successfully');
        }
        
        // Обновляем UI без перезагрузки страницы
        if (typeof window.updateProjectsUI === 'function') {
            window.updateProjectsUI();
        }
        
        // Переходим на страницу проектов
        window.location.hash = 'projects';
        
    } catch (error) {
        console.error('Project operation failed:', error);
        showToast(error.message, 'error');
    }
}
async function loadProjectForEditing(projectId) {
    try {
        const project = await ProjectAPI.getProjectById(projectId);
        fillProjectForm(project);
    } catch (error) {
        console.error('Failed to load project for editing:', error);
        showToast('Failed to load project details', 'error');
        window.location.hash = 'projects';
    }
}

function fillProjectForm(project) {
    const form = document.getElementById('projectForm');
    if (!form) return;
    
    form.projectId.value = project.id;
    form.title.value = project.title || '';
    form.description.value = project.description || '';
    form.category.value = project.category || '';
    form.budgetMin.value = project.budgetMin || '';
    form.budgetMax.value = project.budgetMax || '';
    form.currency.value = project.currencyCode || 'USD';
    form.dueDate.value = project.dueDate ? formatDateForInput(project.dueDate) : '';
    
    // Clear existing tags
    const tagsContainer = document.getElementById('tagsContainer');
    tagsContainer.innerHTML = '';
    
    // Add tags from project data
    const tagInput = document.getElementById('tagInput');
    const hiddenTagsInput = document.getElementById('projectTags');
    const tags = project.tags || [];
    
    tags.forEach(tag => {
        const tagElement = document.createElement('span');
        tagElement.className = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800';
        tagElement.innerHTML = `
            ${tag}
            <button type="button" class="ml-1.5 inline-flex text-blue-500 hover:text-blue-700 remove-tag-btn" data-tag="${tag}">
                <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                </svg>
            </button>
        `;
        tagsContainer.appendChild(tagElement);
    });
    
    hiddenTagsInput.value = JSON.stringify(tags);
}

function initTagsHandlers(form) {
    const tagsContainer = document.getElementById('tagsContainer');
    const tagInput = document.getElementById('tagInput');
    const addTagBtn = document.getElementById('addTagBtn');
    const hiddenTagsInput = document.getElementById('projectTags');
    
    function addTag() {
        const tagText = tagInput.value.trim();
        if (tagText && !tagExists(tagText)) {
            const tagElement = document.createElement('span');
            tagElement.className = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800';
            tagElement.innerHTML = `
                ${tagText}
                <button type="button" class="ml-1.5 inline-flex text-blue-500 hover:text-blue-700 remove-tag-btn" data-tag="${tagText}">
                    <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            `;
            tagsContainer.appendChild(tagElement);
            updateHiddenTags();
            tagInput.value = '';
        }
    }
    
    function tagExists(tagText) {
        return Array.from(tagsContainer.querySelectorAll('.remove-tag-btn'))
            .some(btn => btn.dataset.tag === tagText);
    }
    
    function updateHiddenTags() {
        const tags = Array.from(tagsContainer.querySelectorAll('.remove-tag-btn'))
            .map(btn => btn.dataset.tag);
        hiddenTagsInput.value = JSON.stringify(tags);
    }
    
    // Удаление тега
    tagsContainer.addEventListener('click', (e) => {
        if (e.target.closest('.remove-tag-btn')) {
            e.target.closest('span').remove();
            updateHiddenTags();
        }
    });
    
    addTagBtn.addEventListener('click', addTag);
    tagInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            addTag();
        }
    });
    
    // Загрузка существующих тегов (если они есть)
    if (hiddenTagsInput.value) {
        const tags = JSON.parse(hiddenTagsInput.value);
        tags.forEach(tag => {
            tagInput.value = tag;
            addTag();
        });
    }
}