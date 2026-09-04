FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SGPla.DbMigrator/SGPla.DbMigrator.csproj", "SGPla.DbMigrator/"]
RUN dotnet restore "SGPla.DbMigrator/SGPla.DbMigrator.csproj"

COPY . .
WORKDIR "/src/SGPla.DbMigrator"
RUN dotnet publish "SGPla.DbMigrator.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SGPla.DbMigrator.dll"]
