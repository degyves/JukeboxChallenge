# Use the official Nginx image as the base image
FROM node:20-alpine

WORKDIR /app

COPY package.json ./

RUN npm install

COPY src/server.js ./

EXPOSE 4000

CMD ["node", "server.js"]

