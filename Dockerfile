# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY WebApplication1/*.csproj ./WebApplication1/
RUN dotnet restore ./WebApplication1/WebApplication1.csproj

COPY WebApplication1/. ./WebApplication1/
WORKDIR /app/WebApplication1
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/WebApplication1/out .

# Environment variables
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Use PORT environment variable from Render
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE ${PORT:-8080}

ENTRYPOINT ["dotnet", "WebApplication1.dll"]