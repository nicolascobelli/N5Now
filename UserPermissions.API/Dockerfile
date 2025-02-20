# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the csproj and restore as distinct layers
COPY *.sln .
COPY UserPermissions.API/*.csproj ./UserPermissions.API/
COPY UserPermissions.Application/*.csproj ./UserPermissions.Application/
COPY UserPermissions.Domain/*.csproj ./UserPermissions.Domain/
COPY UserPermissions.Infrastructure/*.csproj ./UserPermissions.Infrastructure/
RUN dotnet restore

# Copy the remaining source code and build the application
COPY . .
WORKDIR /app/UserPermissions.API
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/UserPermissions.API/out .
ENTRYPOINT ["dotnet", "UserPermissions.API.dll"]
