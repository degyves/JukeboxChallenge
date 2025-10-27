FROM mongo:7.0
 
# Optional: set environment variables (you can override at runtime)
ENV MONGO_INITDB_ROOT_USERNAME=admin \
    MONGO_INITDB_ROOT_PASSWORD=secret \
    MONGO_INITDB_DATABASE=mydb
 
# Optional: copy initialization scripts
# Any .js or .sh files in /docker-entrypoint-initdb.d are run on container startup
COPY ./init-scripts/ /docker-entrypoint-initdb.d/
 
# Expose MongoDB port
EXPOSE 27017
 
# Default command (inherited from base image)
CMD ["mongod"]