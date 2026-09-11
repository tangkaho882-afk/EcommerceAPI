# EcommerceAPI

## Overview
Ecommerce API is a backend project for an e-commerce platform built with ASP.NET Core Web API. It provides registration and authentication, product management, shopping cart, checkout and order management. It uses several techniques to fulfill these functions, such as JWT authentication, Entity Framework Core, rate limiting, optimistic concurrency control and centralized exception handling. This project was developed to practise and integrate skills and technologies in an ASP.NET Core environment.


## Features
### Authentication
- User registration
- User login
- Role-based authorization

### Product Management
- Product creation
- Product information updates
- Product listing and viewing
- Product activation and deactivation

### Shopping Cart
- Adding items to the cart
- Updating cart item quantities
- Viewing cart details 
- Removing items from the cart

### Checkout
- Product existence and active status validation
- Product stock validation
- Concurrency conflict handling
- Transaction rollback on checkout failure

### Order Management
- Order status tracking 
- Viewing order details 
- Order cancellation
- Product stock restoration upon cancellation


## Technical Features / Technologies
- Action and result filters
- API versioning
- BCrypt password hashing
- Centralized exception handling with middleware
- Data transfer objects (DTOs)
- Entity Framework Core
- Health checks
- Input validation with data annotations
- JWT authentication
- Structured logging and request tracing
- OpenAPI documentation with Scalar
- Optimistic concurrency control
- Rate limiting
- Standardized error format
- Transactions and rollback


## Getting Started


### Prerequisites
- .NET 8 SDK
- SQL Server Express LocalDB
- Git


### Setup
1. Clone the project from GitHub and navigate to the repository

   ```powershell
   git clone https://github.com/tangkaho882-afk/EcommerceAPI.git
   cd EcommerceAPI
   ```

2. Restore dependencies

	Restore NuGet packages:
	```powershell
	dotnet restore
	```

	Restore .NET tools:
	```powershell
	dotnet tool restore
	```

3. Configure the database connection string if necessary.
	The DefaultConnection is located under the ConnectionStrings in appsettings.json
4. Set the JWT secret key
	```powershell
	cd EcommerceAPI
   	dotnet user-secrets set "Jwt:Key" "<your-secret-key>"
	```
5. Create or update the database
	```powershell
	dotnet ef database update
	```
6. Run the application
	```powershell
	dotnet run
	```
