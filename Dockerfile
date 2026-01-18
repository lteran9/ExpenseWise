# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy everything
COPY . .

# Navigate to presentation layer
WORKDIR /src/Presentation/Api

# Restore as distinct layers
RUN dotnet restore "./Api.csproj"
# Build project
RUN dotnet build "./Api.csproj" -c release -o /app/build --no-restore
# Publish project
RUN dotnet publish "./Api.csproj" -c release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 8080 (default for ASP.NET Core in containers)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Api.dll"]
