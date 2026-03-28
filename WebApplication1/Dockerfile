# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# نسخ ملفات المشروع و restore
COPY WebApplication1/*.csproj ./WebApplication1/
RUN dotnet restore ./WebApplication1/WebApplication1.csproj

# نسخ باقي الملفات
COPY WebApplication1/. ./WebApplication1/
RUN dotnet publish ./WebApplication1/WebApplication1.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .

# افتح بورت 5000
EXPOSE 5000

# شغل المشروع
ENTRYPOINT ["dotnet", "WebApplication1.dll"]