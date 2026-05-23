FROM node:20 AS builder
WORKDIR /app

# Install all dependencies (build requires devDependencies)
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
RUN npm run build

# Prune to production dependencies only
RUN npm prune --only=production

# Runtime stage - use alpine for smaller image
FROM node:20-alpine AS runtime
WORKDIR /app

# Add non-root user for security
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

# Copy built application from builder
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package*.json ./

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:3000 || exit 1

USER nextjs
EXPOSE 3000

ENV NODE_ENV=production
CMD ["npm", "start"]