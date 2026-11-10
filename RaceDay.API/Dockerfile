# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RaceDay.API.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish RaceDay.API.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

# wwwroot/uploads is only used when Azure Blob Storage is not configured.
RUN mkdir -p /app/wwwroot/uploads && chown -R app:app /app/wwwroot
USER app
ENTRYPOINT ["dotnet", "RaceDay.API.dll"]
