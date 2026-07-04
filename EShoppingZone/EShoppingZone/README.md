# EShoppingZone - ASP.NET Core 8 Web API

A complete e-commerce shopping cart backend system built with ASP.NET Core 8, following a 3-layer architecture pattern.

## Architecture

- **Controller Layer**: Handles HTTP requests and responses
- **Service Layer**: Contains business logic
- **Repository Layer**: Manages data access
- **Database Layer**: SQL Server with Entity Framework Core

## Tech Stack

- ASP.NET Core 8 Web API
- C# 12
- Entity Framework Core 8
- SQL Server (LocalDB)
- JWT Authentication
- Swagger API Documentation
- BCrypt for password hashing

## Features

### Authentication
- User registration with password hashing
- JWT token-based authentication
- Role-based authorization

### Product Management
- CRUD operations for products
- Category-based product organization
- Stock management

### Shopping Cart
- Add/remove items from cart
- Quantity management
- Cart persistence per user

### Order Management
- Checkout process
- Order history
- Payment method selection

### Wallet System
- Digital wallet for users
- Add funds functionality
- Wallet-based payments

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

### Cart
- `POST /api/cart/add` - Add item to cart
- `DELETE /api/cart/remove` - Remove item from cart
- `GET /api/cart` - Get cart items

### Orders
- `POST /api/orders` - Create order
- `GET /api/orders/my` - Get user orders

### Wallet
- `GET /api/wallet/balance` - Get wallet balance
- `POST /api/wallet/add` - Add funds to wallet

## Getting Started

1. **Clone the repository**
2. **Restore packages**: `dotnet restore`
3. **Build the project**: `dotnet build`
4. **Run the application**: `dotnet run`
5. **Access Swagger UI**: Navigate to `https://localhost:7xxx/swagger`

## Database

The application uses SQL Server LocalDB. The database is automatically created on first run with seed data including:
- Sample categories (Electronics, Books, Apparel, Personal Care)
- Sample products

## Authentication

The API uses JWT tokens for authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## Sample Usage

1. **Register a user**:
```json
POST /api/auth/register
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123",
  "address": "123 Main St"
}
```

2. **Login**:
```json
POST /api/auth/login
{
  "email": "john@example.com",
  "password": "password123"
}
```

3. **Add item to cart**:
```json
POST /api/cart/add
{
  "productId": 1,
  "quantity": 2
}
```

4. **Create order**:
```json
POST /api/orders
{
  "paymentMethod": "Wallet"
}
```

## Configuration

Update `appsettings.json` for:
- Database connection string
- JWT settings (Key, Issuer, Audience)

## Project Structure

```
EShoppingZone/
├── Controllers/     # API Controllers
├── Services/        # Business Logic
├── Repositories/    # Data Access
├── Models/          # Entity Models
├── DTOs/           # Data Transfer Objects
├── Data/           # DbContext
├── Interfaces/     # Repository Interfaces
├── Helpers/        # JWT Helper
├── Middleware/     # Exception Handling
└── Program.cs      # Application Configuration
```