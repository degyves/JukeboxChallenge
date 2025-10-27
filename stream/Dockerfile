# Use the official Nginx image as the base image
FROM node:18-alpine

# Copy the custom Nginx configuration file to the container
COPY nginx.conf /etc/nginx/nginx.conf

# Copy any static files or assets to the default Nginx directory
# COPY ./public /usr/share/nginx/html

# Expose the default Nginx port
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]