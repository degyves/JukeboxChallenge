FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .

ENV VITE_API_BASE=http://localhost:8080 \
  VITE_STREAM_BASE=https://stream-service-url \
  VITE_HUB_URL=https://hub-service-url

RUN npm run build


FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
