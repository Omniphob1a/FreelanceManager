const STORAGE_KEY = 'app_lang';
const DEFAULT_LANG = 'ru';

const phrasePairs = [
    ['Dashboard', 'Панель'],
    ['Projects', 'Проекты'],
    ['Tasks', 'Задачи'],
    ['Profile', 'Профиль'],
    ['My Profile', 'Профиль'],
    ['Sign in to your account', 'Вход в систему'],
    ['Use your username and password', 'Используйте логин и пароль'],
    ['Username', 'Логин'],
    ['Password', 'Пароль'],
    ['Sign in', 'Войти'],
    ["Don't have an account? Register", 'Нет аккаунта? Зарегистрироваться'],
    ['Create a new account', 'Создание аккаунта'],
    ['Register', 'Зарегистрироваться'],
    ['Already have an account? Sign in', 'Уже есть аккаунт? Войти'],
    ['Not signed in', 'Не выполнен вход'],
    ['Guest', 'Гость'],
    ['Log out', 'Выйти'],
    ['Project Status', 'Статусы проектов'],
    ['Recent Projects', 'Последние проекты'],
    ['View All', 'Все проекты'],
    ['Total Projects', 'Всего проектов'],
    ['Active Projects', 'Активные проекты'],
    ['Completed', 'Завершено'],
    ['Upcoming Deadlines', 'Ближайшие дедлайны'],
    ['Last 30 Days', 'Последние 30 дней'],
    ['Last 90 Days', 'Последние 90 дней'],
    ['This Year', 'Последний год'],
    ['All Time', 'За все время'],
    ['Task Deadlines', 'Сроки задач'],
    ['In time', 'В срок'],
    ['Overdue', 'Просрочено'],
    ['Today', 'На сегодня'],
    ['Create New Project', 'Создать проект'],
    ['Project Title*', 'Название проекта*'],
    ['Description*', 'Описание*'],
    ['Due Date', 'Срок'],
    ['Click to select date', 'Нажмите, чтобы выбрать дату'],
    ['Click the field to open calendar', 'Нажмите на поле для открытия календаря'],
    ['Category*', 'Категория*'],
    ['Select Category', 'Выберите категорию'],
    ['Design', 'Дизайн'],
    ['Development', 'Разработка'],
    ['Frontend', 'Фронтенд'],
    ['Backend', 'Бэкенд'],
    ['Fullstack', 'Фулстек'],
    ['Minimum Budget', 'Минимальный бюджет'],
    ['Maximum Budget', 'Максимальный бюджет'],
    ['Currency', 'Валюта'],
    ['Tags', 'Теги'],
    ['Add a tag and press Enter', 'Введите тег и нажмите Enter'],
    ['Add', 'Добавить'],
    ['Cancel', 'Отмена'],
    ['Create Project', 'Создать проект'],
    ['Edit Project', 'Редактировать проект'],
    ['Update Project', 'Сохранить изменения'],
    ['Create New Task', 'Создать задачу'],
    ['Task Title*', 'Название задачи*'],
    ['Project*', 'Проект*'],
    ['Select Project', 'Выберите проект'],
    ['Assignee', 'Исполнитель'],
    ['Select project first', 'Сначала выберите проект'],
    ['Estimated Hours', 'Плановые часы'],
    ['(optional)', '(необязательно)'],
    ['How many hours will this task take?', 'Сколько часов займет задача?'],
    ['Priority*', 'Приоритет*'],
    ['Low', 'Низкий'],
    ['Medium', 'Средний'],
    ['High', 'Высокий'],
    ['Urgent', 'Срочный'],
    ['Billable Task', 'Оплачиваемая задача'],
    ['Amount', 'Сумма'],
    ['Create Task', 'Создать задачу'],
    ['Assign Task', 'Назначение задачи'],
    ['Select Assignee', 'Выберите исполнителя'],
    ['Unassign', 'Снять назначение'],
    ['Assign', 'Назначить'],
    ['Log Time', 'Учет времени'],
    ['Date', 'Дата'],
    ['Start Time', 'Время начала'],
    ['End Time', 'Время окончания'],
    ['Description', 'Описание'],
    ['What did you work on?', 'Что было сделано?'],
    ['Billable', 'Оплачиваемо'],
    ['Save Time', 'Сохранить время'],
    ['Overview', 'Обзор'],
    ['Milestones', 'Вехи'],
    ['Team', 'Команда'],
    ['Files', 'Файлы'],
    ['Loading...', 'Загрузка...'],
    ['Progress', 'Прогресс'],
    ['Edit', 'Изменить'],
    ['No description provided', 'Описание отсутствует'],
    ['Project description', 'Описание проекта'],
    ['Save', 'Сохранить'],
    ['Project Details', 'Детали проекта'],
    ['Status', 'Статус'],
    ['Email', 'Эл. почта'],
    ['Category', 'Категория'],
    ['Budget', 'Бюджет'],
    ['Created', 'Создан'],
    ['Expires', 'Срок действия'],
    ['Actions', 'Действия'],
    ['Publish Project', 'Опубликовать проект'],
    ['Expiration Date', 'Дата истечения'],
    ['Select date', 'Выберите дату'],
    ['Publish', 'Опубликовать'],
    ['Complete', 'Завершить'],
    ['Archive', 'Архивировать'],
    ['Delete', 'Удалить'],
    ['Loading project status...', 'Загрузка статуса проекта...'],
    ['tags', 'тегов'],
    ['No tags added', 'Теги не добавлены'],
    ['Add tag', 'Добавить тег'],
    ['New Milestone', 'Новая веха'],
    ['Title', 'Название'],
    ['Milestone name', 'Название вехи'],
    ['No milestones yet', 'Пока нет вех'],
    ['Add Milestone', 'Добавить веху'],
    ['Team Members', 'Участники команды'],
    ['Add Member', 'Добавить участника'],
    ['Add Team Member', 'Добавить участника'],
    ['User Login', 'Логин пользователя'],
    ['Enter user login', 'Введите логин пользователя'],
    ['Role', 'Роль'],
    ['Member', 'Участник'],
    ['Admin', 'Администратор'],
    ['Viewer', 'Наблюдатель'],
    ['No team members added', 'Участники не добавлены'],
    ['Files & Attachments', 'Файлы и вложения'],
    ['Add File', 'Добавить файл'],
    ['No attachments yet', 'Пока нет вложений'],
    ['Close', 'Закрыть'],
    ['View Details', 'Подробнее'],
    ['Loading project details...', 'Загрузка данных проекта...'],
    ['Project not found', 'Проект не найден'],
    ['Project updated successfully', 'Проект обновлен'],
    ['Project created successfully', 'Проект создан'],
    ['Failed to load project details', 'Не удалось загрузить детали проекта'],
    ['Failed to load task details', 'Не удалось загрузить детали задачи'],
    ['Failed to delete project', 'Не удалось удалить проект'],
    ['Project deleted successfully', 'Проект удален'],
    ['Failed to archive project', 'Не удалось архивировать проект'],
    ['Project archived', 'Проект архивирован'],
    ['Project marked as completed', 'Проект отмечен как завершенный'],
    ['Failed to complete project', 'Не удалось завершить проект'],
    ['Description cannot be empty', 'Описание не может быть пустым'],
    ['Description updated successfully', 'Описание обновлено'],
    ['Failed to update description', 'Не удалось обновить описание'],
    ['Please fill all required fields', 'Заполните обязательные поля'],
    ['Milestone added', 'Веха добавлена'],
    ['Failed to add milestone', 'Не удалось добавить веху'],
    ['Milestone completed', 'Веха завершена'],
    ['Failed to complete milestone', 'Не удалось завершить веху'],
    ['Tag added', 'Тег добавлен'],
    ['Failed to add tag', 'Не удалось добавить тег'],
    ['Tag removed', 'Тег удален'],
    ['Failed to remove tag', 'Не удалось удалить тег'],
    ['Please enter login', 'Введите логин'],
    ['Member added successfully', 'Участник добавлен'],
    ['Member removed successfully', 'Участник удален'],
    ['Failed to load team members', 'Не удалось загрузить участников'],
    ['Please select expiration date', 'Выберите дату истечения'],
    ['Project published successfully', 'Проект опубликован'],
    ['Failed to publish project', 'Не удалось опубликовать проект'],
    ['Publishing...', 'Публикация...'],
    ['Deleting...', 'Удаление...'],
    ['Archiving...', 'Архивация...'],
    ['Completing...', 'Завершение...'],
    ['Saving...', 'Сохранение...'],
    ['Updating...', 'Обновление...'],
    ['Error loading', 'Ошибка загрузки'],
    ['No projects found', 'Проекты не найдены'],
    ['Get started by creating your first project', 'Создайте первый проект'],
    ['No tasks found', 'Задачи не найдены'],
    ['Get started by creating your first task', 'Создайте первую задачу'],
    ['Enter your username', 'Введите логин'],
    ['Account', 'Профиль'],
    ['Collapse sidebar', 'Свернуть боковую панель'],
    ['Details', 'Подробности'],
    ['Task Details', 'Детали задачи'],
    ['Project:', 'Проект:'],
    ['Status:', 'Статус:'],
    ['Priority:', 'Приоритет:'],
    ['Due Date:', 'Срок:'],
    ['Estimated Hours:', 'Плановые часы:'],
    ['Billable:', 'Оплачиваемая:'],
    ['Hourly Rate:', 'Ставка в час:'],
    ['People', 'Участники'],
    ['Assignee:', 'Исполнитель:'],
    ['Reporter:', 'Постановщик:'],
    ['Time Entries', 'Записи времени'],
    ['Add Time Entry', 'Добавить время'],
    ['Comments', 'Комментарии'],
    ['No comments', 'Нет комментариев'],
    ['No time entries', 'Нет записей времени'],
    ['No due date', 'Срок не задан'],
    ['No title', 'Без названия'],
    ['No project', 'Проект не указан'],
    ['Not set', 'Не задано'],
    ['Not estimated', 'Не оценено'],
    ['Unassigned', 'Не назначен'],
    ['Unknown', 'Неизвестно'],
    ['Unknown User', 'Неизвестный пользователь'],
    ['Add a comment...', 'Добавить комментарий...'],
    ['Start', 'Начать'],
    ['Yes', 'Да'],
    ['No', 'Нет'],
    ['days left', 'дн. осталось'],
    ['days ago', 'дн. назад'],
    ['Failed to refresh project data', 'Не удалось обновить данные проекта'],
    ['Description updated', 'Описание обновлено'],
    ['Min budget cannot be greater than max', 'Мин. бюджет не может быть больше макс.'],
    ['Budget updated', 'Бюджет обновлён'],
    ['Failed to update budget', 'Не удалось обновить бюджет'],
    ['Due date updated', 'Срок обновлён'],
    ['Failed to update due date', 'Не удалось обновить срок'],
    ['Invalid request. Please check the expiration date.', 'Некорректный запрос. Проверьте дату истечения.'],
    ['Are you sure you want to delete this project?', 'Удалить этот проект?'],
    ['Mark this project as completed?', 'Отметить проект как завершённый?'],
    ['Archive this project?', 'Архивировать этот проект?'],
    ['Please fill all required fields correctly', 'Заполните обязательные поля корректно'],
    ['Invalid data provided', 'Переданы некорректные данные'],
    ['Attachment uploaded', 'Файл загружен'],
    ['Failed to upload attachment', 'Не удалось загрузить файл'],
    ['Untitled Milestone', 'Веха без названия'],
    ['Due', 'Срок'],
    ['Pending', 'В ожидании'],
    ['Milestone or project not found', 'Веха или проект не найдены'],
    ['Invalid request - milestone may already be completed', 'Некорректный запрос — возможно, веха уже завершена'],
    ['You do not have permission to complete this milestone', 'Недостаточно прав для завершения вехи'],
    ['Remove tag', 'Удалить тег'],
    ['Untitled Project', 'Проект без названия'],
    ['No login', 'Логин не указан'],
    ['Birthday', 'Дата рождения'],
    ['Remove', 'Удалить'],
    ['from project?', 'из проекта?'],
    ['Please select a valid date', 'Выберите корректную дату'],
    ['Expiration date must be in the future', 'Дата истечения должна быть в будущем'],
    ['Task Management', 'Управление задачами'],
    ['Manage and track tasks in one place', 'Управляйте и отслеживайте задачи в одном месте'],
    ['New Task', 'Новая задача'],
    ['Total Tasks', 'Всего задач'],
    ['In Progress', 'В работе'],
    ['Due Soon', 'Скоро срок'],
    ['Ownership', 'Принадлежность'],
    ['All Tasks', 'Все задачи'],
    ['Assigned to me', 'Назначенные мне'],
    ['Created by me', 'Созданные мной'],
    ['All statuses', 'Все статусы'],
    ['To Do', 'К выполнению'],
    ['Canceled', 'Отменена'],
    ['All priorities', 'Все приоритеты'],
    ['Search tasks...', 'Поиск задач...'],
    ['Filters', 'Фильтры'],
    ['Due from', 'Срок с'],
    ['Due to', 'Срок по'],
    ['Select start date', 'Выберите начальную дату'],
    ['Select end date', 'Выберите конечную дату'],
    ['All projects', 'Все проекты'],
    ['Planned Hours', 'Плановые часы'],
    ['Min', 'Мин'],
    ['Max', 'Макс'],
    ['Billability', 'Оплачиваемость'],
    ['Only billable', 'Только оплачиваемые'],
    ['Only non-billable', 'Только неоплачиваемые'],
    ['Only overdue', 'Только просроченные'],
    ['Apply', 'Применить'],
    ['Reset', 'Сбросить'],
    ['Task', 'Задача'],
    ['Planned, h', 'План, ч'],
    ['Loading tasks...', 'Загрузка задач...'],
    ['Shown', 'Показано'],
    ['of', 'из'],
    ['tasks', 'задач'],
    ['Back', 'Назад'],
    ['Next', 'Далее']
];

const toRu = new Map(phrasePairs.map(([en, ru]) => [en, ru]));
const toEn = new Map(phrasePairs.map(([en, ru]) => [ru, en]));
const toRuEntries = [...toRu.entries()].sort((a, b) => b[0].length - a[0].length);
const toEnEntries = [...toEn.entries()].sort((a, b) => b[0].length - a[0].length);

let currentLanguage = localStorage.getItem(STORAGE_KEY) || DEFAULT_LANG;
let observerStarted = false;

function translateExact(value, lang) {
    if (typeof value !== 'string') return value;
    const trimmed = value.trim();
    if (!trimmed) return value;
    const map = lang === 'ru' ? toRu : toEn;
    if (map.has(trimmed)) return value.replace(trimmed, map.get(trimmed));
    return translateContains(value, lang);
}

function translateContains(value, lang) {
    const entries = lang === 'ru' ? toRuEntries : toEnEntries;
    let result = value;
    for (const [from, to] of entries) {
        if (!from || !result.includes(from)) continue;
        result = result.split(from).join(to);
    }
    return result;
}

function shouldSkipElement(el) {
    const tag = el.tagName;
    return tag === 'SCRIPT' || tag === 'STYLE';
}

function translateTextNodes(root, lang) {
    if (!root) return;
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    const textNodes = [];
    while (walker.nextNode()) {
        const node = walker.currentNode;
        if (!node?.parentElement) continue;
        if (shouldSkipElement(node.parentElement)) continue;
        textNodes.push(node);
    }

    textNodes.forEach((node) => {
        const translated = translateExact(node.nodeValue, lang);
        if (translated !== node.nodeValue) node.nodeValue = translated;
    });
}

function translateAttributes(root, lang) {
    if (!root?.querySelectorAll) return;
    const attrs = ['placeholder', 'title', 'aria-label'];
    const elements = root.querySelectorAll('*');
    elements.forEach((el) => {
        attrs.forEach((attr) => {
            if (!el.hasAttribute(attr)) return;
            const original = el.getAttribute(attr);
            const translated = translateExact(original, lang);
            if (translated !== original) el.setAttribute(attr, translated);
        });
    });
}

export function applyLocalization(root = document) {
    const target = root === document ? document.body : root;
    if (!target) return;
    translateTextNodes(target, currentLanguage);
    translateAttributes(target, currentLanguage);
}

export function getLanguage() {
    return currentLanguage;
}

export function setLanguage(lang) {
    const nextLang = lang === 'en' ? 'en' : 'ru';
    currentLanguage = nextLang;
    localStorage.setItem(STORAGE_KEY, nextLang);
    document.documentElement.lang = nextLang;
    applyLocalization(document);
    document.dispatchEvent(new CustomEvent('app:language-changed', { detail: { language: nextLang } }));
}

export function localizeRuntimeText(value) {
    return translateExact(value, currentLanguage);
}

function initLanguageSwitcher() {
    const switcher = document.getElementById('appLanguageSwitcher');
    if (!switcher) return;
    switcher.value = currentLanguage;
    switcher.addEventListener('change', (e) => {
        setLanguage(e.target.value);
    });
}

function initMutationObserver() {
    if (observerStarted || typeof MutationObserver === 'undefined') return;
    observerStarted = true;
    const observer = new MutationObserver((mutations) => {
        for (const mutation of mutations) {
            for (const node of mutation.addedNodes) {
                if (!(node instanceof Element)) continue;
                applyLocalization(node);
            }
        }
    });
    observer.observe(document.body, { childList: true, subtree: true });
}

export function initLocalization() {
    document.documentElement.lang = currentLanguage;
    initLanguageSwitcher();
    initMutationObserver();
    applyLocalization(document);
}
