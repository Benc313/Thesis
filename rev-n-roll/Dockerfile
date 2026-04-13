# ============================================================================
# Multi-stage Dockerfile for Angular 19 Frontend
# ============================================================================
# 
# This Dockerfile creates an optimized production image for the Rev_n_Roll frontend.
# 
# Stages:
# 1. build - Build environment with Node.js
# 2. final - Production image with Nginx
#
# Build: docker build -t revnroll-frontend .
# Run: docker run -p 4200:80 revnroll-frontend

# ============================================================================
# STAGE 1: Build Environment
# ============================================================================
FROM node:20-alpine AS build

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Build argument for API URL
ARG API_URL=http://localhost:5000
ENV API_URL=$API_URL

# Build application for production
RUN npm run build -- --configuration production

# ============================================================================
# STAGE 2: Production Image with Nginx
# ============================================================================
FROM nginx:alpine AS final

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built application from build stage
COPY --from=build /app/dist/rev-n-roll/browser /usr/share/nginx/html

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
