# Build context = thư mục repo gốc: docker build -f apps/student-web/Dockerfile .
FROM node:24-slim AS build
ENV PNPM_HOME=/pnpm PATH=/pnpm:$PATH
RUN corepack enable
WORKDIR /repo
COPY pnpm-lock.yaml pnpm-workspace.yaml package.json turbo.json tsconfig.base.json ./
COPY apps/student-web apps/student-web
COPY packages packages
RUN pnpm install --frozen-lockfile --ignore-scripts --filter @tedu-market/student-web...
RUN pnpm turbo build --filter=@tedu-market/student-web

FROM node:24-slim AS web
ENV NODE_ENV=production PORT=3000 HOSTNAME=0.0.0.0
WORKDIR /app
COPY --from=build --chown=node:node /repo/apps/student-web/.next/standalone ./
COPY --from=build --chown=node:node /repo/apps/student-web/.next/static apps/student-web/.next/static
USER node
EXPOSE 3000
CMD ["node", "apps/student-web/server.js"]
