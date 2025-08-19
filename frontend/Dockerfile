# Используем Node.js для сборки
FROM node:18 AS build

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# Используем nginx для сервинга собранного фронта
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

# Копируем nginx конфиг (если нужен)
# COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
