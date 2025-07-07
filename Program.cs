using Microsoft.EntityFrameworkCore;
using BeamCalculator.Data.Models;
using BeamCalculator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Business services  
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

// CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Configure Kestrel to listen on all network interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5265); // HTTP - accessible from network
    options.ListenAnyIP(7089, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS - accessible from network
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

// Database initialization - PostgreSQL setup!
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ PostgreSQL Database created successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ PostgreSQL Database error: {ex.Message}");
        Console.WriteLine("💡 Make sure PostgreSQL is installed and running");
        Console.WriteLine("💡 Default connection: Host=localhost;Database=BeamCalculator;Username=postgres;Password=postgres");
    }
}

// Get the local IP address for network access
var localIP = GetLocalIPAddress();

Console.WriteLine("🏗️ Beam Calculator starting with PostgreSQL...");
Console.WriteLine("📊 Local access: http://localhost:5265/");
Console.WriteLine($"🌐 Network access: http://{localIP}:5265/");
Console.WriteLine($"🔒 HTTPS: https://{localIP}:7089/");
Console.WriteLine("🐘 Database: PostgreSQL");
Console.WriteLine("📋 Share the network URL with your team!");

app.Run();

// Helper method to get local IP address
static string GetLocalIPAddress()
{
    try
    {
        using var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
        return endPoint?.Address?.ToString() ?? "localhost";
    }
    catch
    {
        return "localhost";
    }
}