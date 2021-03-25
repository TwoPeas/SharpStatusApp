FROM node:lts-buster-slim AS node_base
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
COPY --from=node_base . .

WORKDIR /src
COPY ["SharpStatusApp/SharpStatusApp.csproj", "SharpStatusApp/"]
RUN dotnet restore "SharpStatusApp/SharpStatusApp.csproj"
WORKDIR "/src/SharpStatusApp/"
COPY "SharpStatusApp/." .

ENV NODE_ENV=production
RUN npm ci --prefer-offline --no-audit --progress=false
RUN npm run build

RUN dotnet build "SharpStatusApp.csproj" -c Release -o /app/build
RUN dotnet publish "SharpStatusApp.csproj" -c Release -o /app/publish

FROM binxio/gcp-get-secret:0.4.1 as gcpget
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS runtime
EXPOSE 8080
WORKDIR /app
COPY --from=gcpget /gcp-get-secret /usr/local/bin/
COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection=gcp:///twopeas/perfect-day

ENTRYPOINT [ "/usr/local/bin/gcp-get-secret", "--project", "twopeas", "--use-default-credentials"]
ENTRYPOINT ["dotnet", "SharpStatusApp.dll"]