# RapidPay Card Management System

A .NET 9 application for managing credit cards and transactions with secure API endpoints.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (Choose one option):
  - [PostgreSQL](https://www.postgresql.org/download/) installed locally
  - [Docker](https://www.docker.com/products/docker-desktop/) for running PostgreSQL in a container

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/appitoriadev/RapidPay.git
cd RapidPay
```

### 2. Set Up PostgreSQL

#### Option 1: Using Docker (Recommended)

```bash
docker run --env=POSTGRES_USER=postgres \
    --env=POSTGRES_PASSWORD=admin123 \
    --env=POSTGRES_DB=rapidpay \
    -p 5432:5432 \
    --name rapidPay \
    -d postgres
```

#### Option 2: Using Local PostgreSQL Installation

1. Install PostgreSQL from the [official website](https://www.postgresql.org/download/)
2. Create a new database:
```sql
CREATE DATABASE rapidpay;
```

### 3. Configure Application Settings

1. Navigate to `Rapidpay.API/appsettings.Development.json`
2. Update the connection string if needed:
```json
{
  "ConnectionStrings": {
    "RapidPayDbConnection": "Host=localhost;Port=5432;Database=rapidpay;Username=postgres;Password=admin123"
  }
}
```
3. For first-time setup, enable database seeding by setting:
```json
{
  "DbPopulate": true
}
```

### 4. Apply Database Migrations

```bash
# Navigate to the API project directory
cd Rapidpay.API

# Apply migrations
dotnet ef database update
```

### 5. Build and Run the Application

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project Rapidpay.API
```

The API will be available at:
- HTTPS: https://localhost:7158
- HTTP: http://localhost:5158
- Swagger UI: https://localhost:7158/swagger/index.html

### 6. Run Tests

```bash
dotnet test
```

## API Documentation

The API provides the following main endpoints with example requests:

### Authentication
- POST `/api/auth/register` - Register a new user
```json
{
  "username": "Jane Doe",
  "password": "password345"
}
```

- POST `/api/auth/login` - Login and get JWT token
```json
{
  "username": "Jane Doe",
  "password": "password345"
}
```

- POST `/api/auth/refresh` - Refresh JWT token

### Cards
- POST `/api/cards` - Create a new card
```json
{
  "userId": 2,
  "cardNumber": "1234567890120000",
  "expiryMonth": "03",
  "expiryYear": "2035",
  "cvv": "123",
  "cardHolderName": "Jane Doe",
  "balance": 2500,
  "cardType": 1,
  "cardBrand": 1
}
```

- GET `/api/cards/{id}` - Get card by ID
- GET `/api/cards/user/{userId}` - Get user's cards

### Transactions
- POST `/api/transactions` - Create a new transaction
```json
{
  "cardId": 2,
  "amount": 105.0,
  "currency": "USD",
  "description": "test"
}
```

- GET `/api/transactions/{id}` - Get transaction by ID
- GET `/api/transactions/card/{cardId}` - Get card's transactions

## Initial Test Data

When `DbPopulate` is set to `true`, the following test data will be created:

1. Test User:
   - Email: test@example.com
   - Password: Test123!

2. Test Card:
   - Card Number: 1234567890123456
   - Expiry: 12/25
   - CVV: 123

## Security Notes

1. The JWT secret key in appsettings is for development only. In production:
   - Use a secure key management system
   - Never commit production keys to source control
   - Use different keys for different environments

2. Update the default database password in production

## Troubleshooting

1. Database Connection Issues:
   - Verify PostgreSQL is running: `docker ps` or `pg_isready`
   - Check connection string in appsettings
   - Ensure port 5432 is not in use

2. Migration Issues:
   ```bash
   # Remove existing migrations
   dotnet ef migrations remove
   
   # Add new migration
   dotnet ef migrations add InitialCreate
   
   # Update database
   dotnet ef database update
   ```

3. Common Issues:
   - If you get a certificate error, you may need to trust the development certificate:
     ```bash
     dotnet dev-certs https --trust
     ```
   - If the database is not accessible, ensure PostgreSQL is running and the port is not blocked
   - For JWT token issues, check that the system time is correct and the token hasn't expired

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details
