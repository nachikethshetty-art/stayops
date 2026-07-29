# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY StayOps.slnx ./
COPY src/StayOps.Domain/StayOps.Domain.csproj src/StayOps.Domain/
COPY src/StayOps.Application/StayOps.Application.csproj src/StayOps.Application/
COPY src/StayOps.Infrastructure/StayOps.Infrastructure.csproj src/StayOps.Infrastructure/
COPY src/StayOps.Api/StayOps.Api.csproj src/StayOps.Api/
COPY src/StayOps.Tests/StayOps.Tests.csproj src/StayOps.Tests/
RUN dotnet restore src/StayOps.Api/StayOps.Api.csproj

COPY src/ src/
RUN dotnet publish src/StayOps.Api/StayOps.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
# database/ ships inside the image so DatabaseScriptRunner can apply indexes/views/procedures on
# startup exactly like local dev does - see Database:ScriptsPath below.
COPY database/ /app/database/

ENV ASPNETCORE_URLS=http://+:8080
ENV Database__ScriptsPath=/app/database
EXPOSE 8080

ENTRYPOINT ["dotnet", "StayOps.Api.dll"]
