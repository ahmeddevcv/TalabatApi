## 📸 Screenshots

>                                   💡 صور المشروع موجودة في مجلد واحد داخل الريبو، تقدر تشوفها من هنا:
> 💬 Go check out the project screenshots

- [`screenshots_angular_api`](./screenshots_angular_api): Contains both Angular UI and API documentation screenshots



Online e-commerce website to sell products developped using : Asp.Net core Api , SQL , Entity Framework ,Onion Architecture , Repository Design Pattern And Unit Of Work , Specification Pattern , Redis.

I added Admin Panel that is built using ASP.NET Core MVC and provides essential management tools for the e-commerce platform.



🧠 Architecture \& Patterns Used

* 🧅 Onion Architecture: Separation of concerns with clear layering between Core, Infrastructure, and API.
* 📐 Specification Pattern: For flexible and reusable query logic.
* 📁 Repository Pattern: Abstracts data access logic.
* 🔄 Unit of Work: Manages transactions across multiple repositories.
* 💳 Stripe Integration: Handles secure payment processing.
* ⚡ Redis Caching: Improves performance with response caching.
* User authentication with JWT



🛠️ Technologies

* ASP.NET Core
* Entity Framework Core
* Postman
* AutoMapper
* Swagger
* Redis
* Stripe

Talabat.API/
├── Controllers/
├── DTOs/
├── Helpers/
├── Extensions/

Talabat.Core/
├── Entities/
├── Repositories-->
├── Services
├── Specifications

Talabat.Repository(implement of Core)/
├── Data --> Config, Migrations, DataSeed, StoreContext,StoreContextDataSeed
├── Identity

Talabat.Service/
├── TokenService
├── OrderService
├── PaymentService
├── ResponseCacheService

