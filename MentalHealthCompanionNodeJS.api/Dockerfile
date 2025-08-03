FROM node:20-alpine
WORKDIR /app
COPY . .
RUN npm install
RUN chmod +x ./wait-for-postgres.sh
EXPOSE 3000
CMD ["./wait-for-postgres.sh", "node", "index.js"]

