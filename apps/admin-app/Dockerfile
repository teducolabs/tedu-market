# Build context = thư mục repo gốc: docker build -f apps/admin-app/Dockerfile .
FROM node:24-slim AS build
ENV PNPM_HOME=/pnpm PATH=/pnpm:$PATH
RUN corepack enable
WORKDIR /repo
COPY pnpm-lock.yaml pnpm-workspace.yaml package.json turbo.json tsconfig.base.json ./
COPY apps/admin-app apps/admin-app
COPY packages packages
RUN pnpm install --frozen-lockfile --ignore-scripts --filter @tedu-market/admin-app...
RUN pnpm turbo build --filter=@tedu-market/admin-app

FROM nginxinc/nginx-unprivileged:stable-alpine AS admin
COPY apps/admin-app/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /repo/apps/admin-app/dist /usr/share/nginx/html
EXPOSE 8080
