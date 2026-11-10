# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so this layer is cached until the project file changes.
COPY RaceDay.MVC.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish RaceDay.MVC.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

# Run as the non-root user that ships with the .NET 8 images.
USER app
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]
