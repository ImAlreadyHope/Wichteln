# Builds the Wichteln API. Run from the repository root:
#   docker build -f deploy/Dockerfile -t wichteln-api .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Wichteln.Shared/Wichteln.Shared.csproj src/Wichteln.Shared/
COPY src/Wichteln.Api/Wichteln.Api.csproj src/Wichteln.Api/
RUN dotnet restore src/Wichteln.Api/Wichteln.Api.csproj
COPY src/Wichteln.Shared/ src/Wichteln.Shared/
COPY src/Wichteln.Api/ src/Wichteln.Api/
RUN dotnet publish src/Wichteln.Api/Wichteln.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Wichteln.Api.dll"]
