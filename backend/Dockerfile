# Build context = thư mục repo gốc: docker build -f backend/Dockerfile --target api|migrator .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/backend
COPY backend/Directory.Build.props backend/Directory.Packages.props backend/BannedSymbols.txt backend/dotnet-tools.json ./
COPY backend/src/ src/
RUN dotnet restore src/TeduMarket.Api/TeduMarket.Api.csproj \
 && dotnet tool restore

FROM build AS publish-api
RUN dotnet publish src/TeduMarket.Api/TeduMarket.Api.csproj -c Release --no-restore -o /out

FROM build AS publish-migrator
# Giá trị giả chỉ để factory dựng được DbContext lúc sinh bundle; không vào image cuối.
RUN ConnectionStrings__Default="Host=build-only" dotnet ef migrations bundle \
      --project src/TeduMarket.Infrastructure \
      --startup-project src/TeduMarket.Api \
      --self-contained -r linux-x64 -o /out/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
RUN apt-get update \
 && apt-get install -y --no-install-recommends ffmpeg \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish-api /out ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeduMarket.Api.dll"]

# Chuỗi kết nối lấy từ biến môi trường ConnectionStrings__Default (DesignTimeDbContextFactory).
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrator
WORKDIR /app
COPY --from=publish-migrator /out/efbundle ./
USER $APP_UID
ENTRYPOINT ["./efbundle"]
