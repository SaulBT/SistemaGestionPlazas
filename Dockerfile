FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SGPla/SGPla.csproj", "SGPla/"]
RUN dotnet restore "SGPla/SGPla.csproj"
COPY . .
WORKDIR "/src/SGPla"
RUN dotnet build "SGPla.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "SGPla.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/Archivos
ENTRYPOINT ["dotnet", "SGPla.dll"]
