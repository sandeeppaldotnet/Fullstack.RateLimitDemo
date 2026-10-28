using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//Swagger configurations
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();


// Register rate-limiting services.
builder.Services.AddRateLimiter(options =>
{
    // Return HTTP 429 when a request is rejected.
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // Configure the named "products" policy.
    options.AddPolicy("products", httpContext =>
    {
        // Identify the client by IP address.
        var clientIp =
            httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        // Allow five requests per 30-second window.
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,

                Window = TimeSpan.FromSeconds(30),

                QueueLimit = 0,

                AutoReplenishment = true
            });
    });

    // Customize the response when the limit is exceeded.

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType =
            "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                statusCode = StatusCodes.Status429TooManyRequests,
                error = "Too Many Requests",
                message =
                    "You have exceeded the request limit. Please try again later."
            },
            cancellationToken);
    };
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRateLimiter();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
