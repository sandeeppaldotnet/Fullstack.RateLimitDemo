# Fullstack Rate Limit Demo

A small ASP.NET Core Web API built with .NET 10 that demonstrates endpoint-level rate limiting with the built-in `System.Threading.RateLimiting` APIs.

The sample exposes an in-memory product catalog and applies a fixed-window rate-limit policy to the endpoint that returns all products.

## Features

- ASP.NET Core Web API targeting .NET 10.
- Swagger/OpenAPI documentation in the Development environment.
- In-memory product data with no database dependency.
- Product lookup by ID.
- Product filtering by category.
- IP-address-based rate limiting for `GET /api/products`.
- JSON responses with HTTP 429 when the request limit is exceeded.

## Requirements

- .NET 10 SDK
- An IDE such as Visual Studio 2026, or a terminal with PowerShell, Bash, or an equivalent shell

Verify the installed SDK with:

```bash
dotnet --version
```

## Project structure

```text
Fullstack.RateLimitDemo/
├── Controllers/
│   └── ProductsController.cs       # Product API endpoints
├── Models/
│   └── Product.cs                  # Product model
├── Properties/
│   └── launchSettings.json         # Local HTTP/HTTPS launch profiles
├── Program.cs                      # Application and middleware configuration
├── Fullstack.RateLimitDemo.csproj  # Project and package configuration
└── appsettings.json                # Application logging configuration
```

## Run the application

Restore dependencies and start the application from the project directory:

```bash
dotnet restore
dotnet run
```

The configured development URLs are:

- HTTP: `http://localhost:5211`
- HTTPS: `https://localhost:7092`

When running in Development, open Swagger UI at:

- `http://localhost:5211/swagger`
- `https://localhost:7092/swagger`

The HTTPS development certificate may need to be trusted on your machine. If necessary, run:

```bash
dotnet dev-certs https --trust
```

## API endpoints

### Get all products

```http
GET /api/products
```

Returns all products. This endpoint uses the `products` rate-limit policy.

Example:

```bash
curl http://localhost:5211/api/products
```

Example response:

```json
[
  {
	"id": 1,
	"name": "Laptop",
	"category": "Electronics",
	"price": 75000,
	"inStock": true
  },
  {
	"id": 2,
	"name": "Smartphone",
	"category": "Electronics",
	"price": 35000,
	"inStock": true
  }
]
```

### Get a product by ID

```http
GET /api/products/{id}
```

Example:

```bash
curl http://localhost:5211/api/products/1
```

A missing product returns `404 Not Found`:

```json
{
  "message": "Product with ID 99 was not found."
}
```

### Get products by category

```http
GET /api/products/category/{category}
```

Category matching is case-insensitive.

Example:

```bash
curl http://localhost:5211/api/products/category/electronics
```

## Rate limiting

The application registers a named policy called `products` in `Program.cs` and applies it to `ProductsController.GetAllProducts` with `[EnableRateLimiting("products")]`.

Current policy behavior:

| Setting | Value |
|---|---:|
| Algorithm | Fixed window |
| Partition key | Client IP address |
| Permit limit | 3 requests |
| Window | 30 seconds |
| Queue limit | 0 requests |
| Auto replenishment | Enabled |
| Rejection status | `429 Too Many Requests` |

Because the partition key is the remote IP address, each client IP receives its own counter. In local development, requests from the same machine normally share the same counter.

When the limit is exceeded, the API returns JSON similar to:

```json
{
  "statusCode": 429,
  "error": "Too Many Requests",
  "message": "You have exceeded the request limit. Please try again later."
}
```

To observe the limit, send four requests within 30 seconds:

```bash
curl -i http://localhost:5211/api/products
curl -i http://localhost:5211/api/products
curl -i http://localhost:5211/api/products
curl -i http://localhost:5211/api/products
```

The first three requests should be allowed and the fourth should normally return `429`. The counter is replenished after the fixed window expires.

### Important rate-limit considerations

- The rate limit currently applies only to `GET /api/products`; the ID and category endpoints are not rate limited.
- IP-based limiting can be inaccurate when the application is behind a reverse proxy or load balancer unless forwarded headers and trusted proxy configuration are handled correctly.
- The in-memory limiter is local to the application instance. A distributed limiter is needed for consistent limits across multiple instances.
- The product list is static and is not persisted between application restarts.

## Testing with Visual Studio or an HTTP file

The repository includes `Fullstack.RateLimitDemo.http` for IDE-based HTTP requests. Update the request in that file to call an available product endpoint, for example:

```http
@HostAddress = http://localhost:5211

GET {{HostAddress}}/api/products
Accept: application/json
```

You can also use Swagger UI to execute and inspect the endpoints interactively.

## Build and test

Build the project with:

```bash
dotnet build
```

If tests are added to the solution, run them with:

```bash
dotnet test
```

## Configuration

`appsettings.json` currently configures the default logging level as `Information` and ASP.NET Core logs as `Warning`. The rate-limit values are defined directly in `Program.cs`:

- `PermitLimit = 3`
- `Window = TimeSpan.FromSeconds(30)`
- `QueueLimit = 0`

For a production application, consider moving these values into configuration and adding validation around them.

## Dependencies

The project uses:

- `Microsoft.AspNetCore.OpenApi` version `10.0.8`
- `Swashbuckle.AspNetCore` version `10.3.1`

The rate-limiting implementation uses APIs included with the .NET platform and does not require a separate rate-limiting package.

## License

No license file is currently included in the repository. Add a license before distributing the project or using it as a public template.
