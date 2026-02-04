# Use Node.js base image
FROM node:18-alpine

# Set working directory
WORKDIR /opt/starsky/mail

# Copy application files
COPY starsky-tools/mail/ ./

# Install dependencies (dotenv and imap-simple are required)
RUN npm ci

# Set environment variables (can be overridden at runtime)
ENV NODE_ENV=production

# Default command to run the mail.js script
CMD ["node", "mail.js"]