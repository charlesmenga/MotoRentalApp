# MotoRentalApp

MotoRentalApp is a .NET 8.0 Web API project designed for managing motorcycle rentals created for a test based on  https://github.com/Mottu-ops/Desafio-BackEnd/?tab=readme-ov-file instructions. It leverages modern technologies such as Entity Framework Core with PostgreSQL for data persistence, RabbitMQ for messaging, and Swagger for API documentation.

## Technologies Used

- .NET 8.0
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Swagger (Swashbuckle)

## Setup Instructions

1. **Clone the repository**

```bash
git clone [repository-url]
cd MotoRentalApp
```

2. **Configure the database connection**

Update the connection string in `MotoRentalApp/appsettings.json` or `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=mottudb;Username=mottuuser;Password=mottupassword"
}
```

3. **Build the project**

```bash
dotnet build MotoRentalApp/MotoRentalApp.csproj
```

4. **Run the project**

```bash
dotnet run --project MotoRentalApp/MotoRentalApp.csproj
```

The API will be available at `https://localhost:{port}`.

## Running Tests

Integration and unit tests are located in the `Tests` directory. To run all tests:

```bash
dotnet test
```

## API Documentation

Swagger UI is available when running the project in the development environment. Access it at:

```
https://localhost:{port}/swagger
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
