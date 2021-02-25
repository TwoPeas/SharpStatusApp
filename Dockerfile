#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM node:latest AS node_base
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
COPY --from=node_base . .

WORKDIR /src
COPY ["SharpStatusApp/SharpStatusApp.csproj", "SharpStatusApp/"]
RUN dotnet restore "SharpStatusApp/SharpStatusApp.csproj"
WORKDIR "/src/SharpStatusApp/"
COPY "SharpStatusApp/." .

RUN npm ci

ENV NODE_ENV=production
RUN npm run build
RUN dotnet build "SharpStatusApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SharpStatusApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharpStatusApp.dll"]