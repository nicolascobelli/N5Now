# UserPermissions Project

## Overview
The UserPermissions project is a .NET solution designed to manage user permissions within an application. It follows a clean architecture approach, separating concerns into different projects for better maintainability and scalability.

## Project Structure
The solution consists of the following projects:

- **UserPermissions.API**: Contains the API controllers and configuration for the web application.
- **UserPermissions.Domain**: Defines the core entities of the application, such as the User entity.
- **UserPermissions.Infrastructure**: Handles data access, including SQL Server, ElasticSearch, and Kafka configurations.
- **UserPermissions.Application**: Implements business logic using the Command Query Responsibility Segregation (CQRS) pattern.
- **UserPermissions.UnitTests**: Contains unit tests for the application services.
- **UserPermissions.IntegrationTests**: Contains integration tests to verify the interaction between components.

## Setup Instructions
1. Clone the repository:
   ```
   git clone <repository-url>
   cd UserPermissions
   ```

2. Restore the NuGet packages:
   ```
   dotnet restore
   ```

3. Update the `appsettings.json` file in the `UserPermissions.API` project with your database connection string and other necessary configurations.

4. Run the application:
   ```
   dotnet run --project UserPermissions.API
   ```

## Usage
- The API exposes endpoints for managing user permissions. You can interact with the API using tools like Postman or curl.
- The application supports creating users and querying user information through the defined commands and queries.

## Testing
- Unit tests can be run using:
  ```
  dotnet test UserPermissions.UnitTests
  ```

- Integration tests can be run using:
  ```
  dotnet test UserPermissions.IntegrationTests
  ```

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.