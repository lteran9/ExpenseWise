# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Api.dll"]   