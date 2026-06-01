import { UserAPI } from '../api.js';
import { showToast, formatDate, formatDateForInput } from './ui.js';
import { checkAuth } from './auth.js';

const genderLabels = { 0: 'Женский', 1: 'Мужской', 2: 'Не указан' };

function showErr(msg) {
    const el = document.getElementById('profileLoadError');
    if (!el) return;
    el.textContent = msg;
    el.classList.toggle('hidden', !msg);
}

function setViewMode(user) {
    document.getElementById('profileViewSection')?.classList.remove('hidden');
    document.getElementById('profileEditSection')?.classList.add('hidden');

    document.getElementById('profileName').textContent = user.name || user.login || '—';
    document.getElementById('profileLogin').textContent = user.login ? `Логин: ${user.login}` : '—';
    const rolesText = user.roles?.length ? user.roles.join(', ') : (user.admin ? 'Администратор' : 'Пользователь');
    document.getElementById('profileRoles').textContent = rolesText;
    document.getElementById('profileEmail').textContent = user.email || '—';
    document.getElementById('profileGender').textContent = genderLabels[user.gender] ?? genderLabels[2];
    document.getElementById('profileBirthday').textContent = user.birthday ? formatDate(user.birthday) : '—';
    document.getElementById('profileJoined').textContent = user.createdAt ? formatDate(user.createdAt) : '—';

    const badge = document.getElementById('profileLetterBadge');
    if (badge) badge.innerHTML = '<i class="fas fa-user"></i>';
}

function fillEditForm(user) {
    document.getElementById('editName').value = user.name || '';
    document.getElementById('editEmail').value = user.email || '';
    document.getElementById('editGender').value = String([0, 1, 2].includes(user.gender) ? user.gender : 2);
    document.getElementById('editBirthday').value = user.birthday ? formatDateForInput(user.birthday) : '';
}

export async function initProfilePage() {
    showErr('');

    let user;
    try {
        await checkAuth(true);
        user = await UserAPI.getProfile();
    } catch (e) {
        console.error('Profile load failed:', e);
        showErr('Не удалось загрузить профиль. Проверьте вход и доступ к /api/Users/get-my-profile.');
        return;
    }

    if (!user) {
        showErr('Профиль недоступен (нет данных пользователя).');
        return;
    }

    setViewMode(user);

    const editBtn = document.getElementById('editProfileBtn');
    const cancelBtn = document.getElementById('cancelEditProfileBtn');
    const editForm = document.getElementById('profileEditForm');
    const pwdForm = document.getElementById('changePasswordForm');

    const openEdit = () => {
        fillEditForm(user);
        document.getElementById('profileEditSection')?.classList.remove('hidden');
    };
    const closeEdit = () => {
        document.getElementById('profileEditSection')?.classList.add('hidden');
    };

    editBtn?.replaceWith(editBtn.cloneNode(true));
    cancelBtn?.replaceWith(cancelBtn.cloneNode(true));
    editForm?.replaceWith(editForm.cloneNode(true));
    pwdForm?.replaceWith(pwdForm.cloneNode(true));

    document.getElementById('editProfileBtn')?.addEventListener('click', openEdit);
    document.getElementById('cancelEditProfileBtn')?.addEventListener('click', closeEdit);

    document.getElementById('profileEditForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const name = document.getElementById('editName').value.trim();
        const email = document.getElementById('editEmail').value.trim();
        const gender = parseInt(document.getElementById('editGender').value, 10);
        const bStr = document.getElementById('editBirthday').value;
        if (!bStr) {
            showToast('Укажите дату рождения.', 'error');
            return;
        }
        const birthday = new Date(`${bStr}T12:00:00`);
        if (Number.isNaN(birthday.getTime())) {
            showToast('Некорректная дата рождения.', 'error');
            return;
        }
        try {
            await UserAPI.updateUser(user.id, {
                newName: name,
                newEmail: email,
                newGender: gender,
                newBirthday: birthday.toISOString()
            });
            showToast('Профиль обновлен.', 'success');
            user = await UserAPI.getProfile();
            if (user) setViewMode(user);
            closeEdit();
            await checkAuth(true);
        } catch (err) {
            console.error(err);
            showToast(err?.message || 'Не удалось обновить профиль.', 'error');
        }
    });

    document.getElementById('changePasswordForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const p1 = document.getElementById('newPassword').value;
        const p2 = document.getElementById('newPasswordConfirm').value;
        if (p1 !== p2) {
            showToast('Пароли не совпадают.', 'error');
            return;
        }
        try {
            await UserAPI.changePassword(user.id, p1);
            showToast('Пароль изменен.', 'success');
            document.getElementById('newPassword').value = '';
            document.getElementById('newPasswordConfirm').value = '';
        } catch (err) {
            console.error(err);
            showToast(err?.message || 'Не удалось изменить пароль.', 'error');
        }
    });
}
