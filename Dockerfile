# syntax=docker/dockerfile:1.20

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY --parents src/**/*.csproj .
RUN dotnet restore src/Mechanics.Api/Mechanics.Api.csproj --locked-mode

COPY src src
RUN dotnet build src/Mechanics.Api/Mechanics.Api.csproj --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final

ENV TZ=America/Sao_Paulo
ENV LANG=pt_BR.UTF-8 LANGUAGE=pt_BR:pt LC_ALL=pt_BR.UTF-8
EXPOSE 8080

WORKDIR /app
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "Mechanics.Api.dll"]
