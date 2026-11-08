# 🍔 Talabat E-Commerce Web API

[![.NET 6](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-10.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-6.0-512BD4)](https://docs.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%2B-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Redis](https://img.shields.io/badge/Redis-Upstash%20Cloud-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![Stripe](https://img.shields.io/badge/Payments-Stripe-008CDD?logo=stripe&logoColor=white)](https://stripe.com/)
[![Swagger](https://img.shields.io/badge/API%20Docs-Swagger%20UI-85EA2D?logo=swagger&logoColor=black)](https://talabatfood.runasp.net/)
[![Hosted on MonsterASP](https://img.shields.io/badge/Hosted%20on-MonsterASP-0078D7?logo=windows&logoColor=white)](https://talabatfood.runasp.net/)
[![Frontend Live](https://img.shields.io/badge/Frontend-Vercel%20Live-black?logo=vercel&logoColor=white)](https://talabat-ui.vercel.app/)

A robust, enterprise-grade **E-Commerce RESTful Web API** built with **ASP.NET Core 6**, implementing **Clean / Onion Architecture**, **Generic Repository Pattern**, **Specification Pattern**, **Unit of Work**, **Redis Caching**, and **Stripe Payment Gateway**.

---

## 🌐 Live Deployments

| Component | Platform | URL |
| :--- | :--- | :--- |
| **API & Swagger UI** | MonsterASP (IIS Cloud) | [talabatfood.runasp.net](https://talabatfood.runasp.net/) |
| **Angular 18 Frontend** | Vercel | [talabat-ui.vercel.app](https://talabat-ui.vercel.app/) |
| **Frontend Repository** | GitHub | [github.com/ahmeddevcv/TalabatUI](https://github.com/ahmeddevcv/TalabatUI) |

---

## 🔑 Demo Credentials

| Role | Email | Password | Access |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@talabat.com` | `Admin@123` | Full Dashboard, Products CRUD, Orders Moderation, Stripe Sync |
| **User** | `Ahmed.Shaban@linkdev.com` | `Pa$$w0rd` | Shopping, Basket, Checkout, Order History, User Profile |

---

## 🧠 Architecture & Design Patterns

The solution is structured following **Clean / Onion Architecture** principles with strict separation of concerns:

```text
Talabat.G04.Solution/
│
├── 📂 Talabat.Core/            # Domain Entities, Interfaces & Specifications (No dependencies)
│   ├── Entities/               # Product, ProductBrand, ProductType, Basket, Order Aggregate
│   ├── Repositories/           # IGenericRepository<T>, IBasketRepository, IUnitOfWork
│   ├── Services/               # ITokenService, IOrderService, IPaymentService, IResponseCacheService
│   └── Specifications/         # BaseSpecification<T>, ISpecification<T>, query params
│
├── 📂 Talabat.Repository/      # Data Access Layer & EF Core Implementation
│   ├── Data/                   # StoreContext, Configurations, Migrations, DataSeed JSONs
│   ├── Identity/               # AppIdentityDbContext, Identity Migrations & Seed
│   └── GenericRepository.cs    # SpecificationEvaluator implementation
│
├── 📂 Talabat.Service/         # Business Logic Layer
│   ├── TokenService.cs         # JWT generation with Claims and Roles
│   ├── OrderService.cs         # Order creation, delivery calculations, stock tracking
│   ├── PaymentService.cs       # Stripe PaymentIntent creation & webhook fulfillment
│   └── ResponseCacheService.cs # Distributed Redis caching service
│
└── 📂 Talabat.APIs/            # Presentation Layer (REST API)
    ├── Controllers/            # Accounts, Admin, Products, Basket, Order, Payments
    ├── Dtos/                   # Data Transfer Objects
    ├── Errors/                 # ApiResponse, ApiExceptionResponse, ApiValidationErrorResponse
    ├── Extensions/             # ServiceCollection & Middleware Extensions
    ├── Helpers/                # AutoMapper Profiles, [Cached] Attribute, Picture URL Resolvers
    └── Middlewares/            # Global ExceptionMiddleware
```

### Key Design Patterns:
- **Onion Architecture:** Core domain has zero dependencies on external frameworks or databases.
- **Specification Pattern:** Encapsulates query logic (filtering, sorting, paging, includes) into reusable specification classes.
- **Generic Repository & Unit of Work:** Decouples business logic from EF Core DbContext and ensures atomic transactions.
- **Response Caching Pattern:** Custom `[Cached]` action filter attribute that caches HTTP responses in Redis for high read performance.
- **Picture URL Resolvers:** AutoMapper value resolvers dynamically prepend `ApiBaseUrl` for seamless asset serving across environments.

---

## 🚀 Features

### 🛒 E-Commerce & Customer Features
- **Product Catalog:** Filtering by Brand and Type, searching by keyword, sorting (price, name), and responsive pagination.
- **Shopping Basket:** High-performance, distributed Redis basket with auto-expiration (TTL).
- **Checkout & Orders:** Multi-step order creation with delivery method selection and address capture.
- **Stripe Payments:** Secure credit card payments using Stripe PaymentIntents.
- **Order History:** Customer order tracking with detailed line items and shipping statuses.

### 🛡️ Admin Dashboard & Management (`[Authorize(Roles = "Admin")]`)
- **Dashboard Analytics (`GET /api/admin/stats`):** Real-time statistics including total revenue, order count, product count, and registered users.
- **Product Management:** Full CRUD operations with secure image file upload (size/extension validation).
- **Order Moderation:** Filter and view all customer orders, update order status, and execute manual status overrides.
- **Live Stripe Payment Verification:** Directly inspect order payment status on Stripe servers (`verify-payment` and `verify-all-pending`).
- **User Account Moderation:** View registered users, roles, and administrative account deletion.

### 🔐 Security & Identity
- **ASP.NET Core Identity:** Secure password hashing, lockout, and user entity customization (`AppUser`).
- **JWT Authentication:** Signed JSON Web Tokens containing user claims and roles (`Admin` / `User`).
- **Role-Based Authorization:** Controller and endpoint-level authorization policies.
- **UserProfile Management:** Profile picture upload/delete, display name editing, password changes, and account deletion.

### ⚙️ Production & Cloud Engineering
- **Automated Startup Migration & Seeding:** Automatically runs `MigrateAsync()` and seeds brands, types, products, delivery methods, and default accounts on boot.
- **Multi-Tier AppSettings:** Secure separation between base templates, local development, and production cloud secrets.
- **Cross-Origin Resource Sharing (CORS):** Flexible origin validation supporting both production Vercel frontend and local development.
- **Swagger UI at Root (`/`):** OpenAPI documentation served directly at application root for both Development and Production environments.

---

## 📡 API Endpoints Overview

| Category | Method | Endpoint | Description | Auth |
| :--- | :---: | :--- | :--- | :---: |
| **Products** | `GET` | `/api/products` | Get paginated products with filtering & sorting | Public |
| **Products** | `GET` | `/api/products/{id}` | Get product details by ID | Public |
| **Products** | `GET` | `/api/products/brands` | Get all product brands | Public |
| **Products** | `GET` | `/api/products/types` | Get all product types | Public |
| **Basket** | `GET` | `/api/basket?id={id}` | Get customer basket from Redis | Public |
| **Basket** | `POST` | `/api/basket` | Create or update customer basket | Public |
| **Basket** | `DELETE`| `/api/basket?id={id}` | Delete customer basket | Public |
| **Auth** | `POST` | `/api/accounts/login` | Login and receive JWT token | Public |
| **Auth** | `POST` | `/api/accounts/register`| Register new customer account | Public |
| **Auth** | `GET` | `/api/accounts/get` | Get current logged-in user | `Bearer` |
| **Auth** | `POST` | `/api/accounts/upload-picture` | Upload user profile picture | `Bearer` |
| **Orders** | `POST` | `/api/order` | Place new order | `Bearer` |
| **Orders** | `GET` | `/api/order` | Get customer orders | `Bearer` |
| **Orders** | `GET` | `/api/order/{id}` | Get order details by ID | `Bearer` |
| **Orders** | `GET` | `/api/order/deliveryMethods` | Get available shipping methods | `Bearer` |
| **Payments** | `POST` | `/api/payments/{basketId}` | Create or update Stripe PaymentIntent | `Bearer` |
| **Admin** | `GET` | `/api/admin/stats` | Dashboard KPI summary statistics | `Admin` |
| **Admin** | `POST` | `/api/admin/products` | Create new product with image | `Admin` |
| **Admin** | `PUT` | `/api/admin/products/{id}` | Update existing product | `Admin` |
| **Admin** | `DELETE`| `/api/admin/products/{id}` | Delete product | `Admin` |
| **Admin** | `GET` | `/api/admin/orders` | Get all platform customer orders | `Admin` |
| **Admin** | `PUT` | `/api/admin/orders/{id}/status` | Update or override order status | `Admin` |
| **Admin** | `POST` | `/api/admin/orders/{id}/verify-payment` | Verify order payment status with Stripe | `Admin` |

---

## 🛠️ Local Development Setup

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, SQL Express, or Docker)
- [Redis](https://redis.io/download/) (Local or free cloud instance on [Upstash](https://upstash.com/))

### Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ahmeddevcv/TalabatApi.git
   cd TalabatApi
   ```

2. **Configure Connection Strings:**
   Open `Talabat.APIs/appsettings.Development.json` (or `appsettings.json`) and verify your connection strings:
   ```json
   {
     "ConnectionStrings": {
       "cs": "Server=.; Database=Talabat.APIs; Trusted_Connection=True; MultipleActiveResultSets=True",
       "IdentityConnection": "Server=.; Database=Talabat.APIs.Identity; Trusted_Connection=True; MultipleActiveResultSets=True",
       "redis": "localhost"
     }
   }
   ```

3. **Build the Solution:**
   ```bash
   dotnet build
   ```

4. **Run the Application:**
   ```bash
   dotnet run --project Talabat.APIs
   ```

5. **Explore Swagger UI:**
   Navigate to `https://localhost:7084/` or `http://localhost:5000/` in your browser. All tables and seed data will be automatically generated!

---

## 👨‍💻 Author

**Ahmed Shaban**  
- GitHub: [@ahmeddevcv](https://github.com/ahmeddevcv)
- Project Repository: [TalabatApi](https://github.com/ahmeddevcv/TalabatApi)
- Frontend Repository: [TalabatUI](https://github.com/ahmeddevcv/TalabatUI)
