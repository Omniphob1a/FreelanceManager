// modules/profile.js
import { UserAPI } from '../api.js';
import { getCurrentUser } from './auth.js';
import { formatDate } from './ui.js';

export async function initProfilePage() {
    try {
        const user = await getCurrentUser();
        renderProfile(user);
    } catch (error) {
        console.error('Failed to load profile:', error);
    }
}

function renderProfile(user) {
    document.getElementById('profileName').textContent = user.name || 'No name';
    document.getElementById('profileEmail').textContent = user.email || 'No email';
    document.getElementById('profileJoined').textContent = user.createdAt ? formatDate(user.createdAt) : 'Unknown';
}