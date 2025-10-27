# Use the official Nginx image as the base image
FROM node:18-alpine

# Copy the custom Nginx configuration file to the container
COPY src/server.js /server.js
COPY package.json /package.json

# Copy any static files or assets to the default Nginx directory
# COPY ./public /usr/share/nginx/html

# Expose the default Nginx port
EXPOSE 80

CMD ["npm", "install"]

CMD ["node", "server.js"]

