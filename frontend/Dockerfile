# Этап сборки
FROM node:18 AS build
WORKDIR /app

# Копируем package.json и package-lock.json (если есть)
COPY package*.json ./
RUN npm install

# Копируем весь фронт
COPY . .

# Собираем Vite-проект
RUN npm run build

# Этап запуска
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
