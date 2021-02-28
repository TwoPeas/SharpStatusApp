FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app

FROM node:lts-buster-slim AS node_base
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
COPY --from=node_base . .

WORKDIR /src
COPY ["SharpStatusApp/SharpStatusApp.csproj", "SharpStatusApp/"]
RUN dotnet restore "SharpStatusApp/SharpStatusApp.csproj"
WORKDIR "/src/SharpStatusApp/"
COPY "SharpStatusApp/." .

ENV NODE_ENV=production
RUN npm ci --also=dev
RUN npm run build
RUN dotnet build "SharpStatusApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SharpStatusApp.csproj" -c Release -o /app/publish

FROM base AS final
EXPOSE 8080
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharpStatusApp.dll"]