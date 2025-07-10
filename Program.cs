var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for all origins (for custom domain)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCustomDomain", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Configure for Azure Container Instances
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // HTTP only for Azure
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("AllowCustomDomain");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

Console.WriteLine("🏗️ Beam Calculator starting on Azure...");
Console.WriteLine("🌐 Port 80 (HTTP) for Azure Load Balancer");

app.Run();