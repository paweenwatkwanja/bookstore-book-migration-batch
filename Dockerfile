# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/BookMigrationBatch.csproj src/
RUN dotnet restore src/BookMigrationBatch.csproj

COPY src/ src/
RUN dotnet publish src/BookMigrationBatch.csproj -c Release -o /app/publish --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookMigrationBatch.dll"]
CMD ["BookMigrate"]
