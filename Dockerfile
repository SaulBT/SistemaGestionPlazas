# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

# Restore aislado: solo se invalida cuando cambia el .csproj. Los paquetes NuGet se cachean entre builds.
COPY ["SGPla/SGPla.csproj", "SGPla/"]
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "SGPla/SGPla.csproj"

# Solo el código del backend (el resto del repo no entra al contexto de compilación).
COPY SGPla/ SGPla/
# publish ya compila: no hace falta un `dotnet build` previo.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "SGPla/SGPla.csproj" -c Release -o /app/publish \
      --no-restore /p:UseAppHost=false /p:DebugType=none /p:DebugSymbols=false

# Base fijada a Ubuntu 24.04 (noble): LibreOffice 24.2.x, rama estable con parches de seguridad de la distro.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
# libreoffice-writer-nogui: sin dependencias gráficas, suficiente para la conversión headless.
# fonts-liberation / fonts-crosextra-carlito: equivalentes métricos de Arial y Calibri para que el PDF conserve el formato.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libldap2 libreoffice-writer-nogui fonts-liberation fonts-crosextra-carlito \
    && rm -rf /var/lib/apt/lists/* /var/cache/apt/*

WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_TieredPGO=0 \
    DOTNET_GCHeapHardLimitPercent=60
EXPOSE 8080
COPY --from=build /app/publish .
RUN mkdir -p /app/Archivos
ENTRYPOINT ["dotnet", "SGPla.dll"]
