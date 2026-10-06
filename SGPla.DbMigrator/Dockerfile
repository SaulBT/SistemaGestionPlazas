# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

COPY ["SGPla.DbMigrator/SGPla.DbMigrator.csproj", "SGPla.DbMigrator/"]
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "SGPla.DbMigrator/SGPla.DbMigrator.csproj"

COPY SGPla.DbMigrator/ SGPla.DbMigrator/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "SGPla.DbMigrator/SGPla.DbMigrator.csproj" -c Release -o /app/publish \
      --no-restore /p:UseAppHost=false /p:DebugType=none /p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SGPla.DbMigrator.dll"]
