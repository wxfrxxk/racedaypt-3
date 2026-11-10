using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

// Make sure wwwroot exists so uploaded files can be served when Azure is not configured.
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Show the XML summaries written on each endpoint in Swagger UI.
    string xmlFile = Path.Combine(AppContext.BaseDirectory, "RaceDay.API.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// Connect the API to SQL Server.
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayConnection")));

// Configure session storage.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// File storage: Azure Blob Storage when a connection string is configured, local folder otherwise.
string? blobConnection = builder.Configuration["AzureBlob:ConnectionString"];

if (!string.IsNullOrWhiteSpace(blobConnection))
{
    string containerName = builder.Configuration["AzureBlob:ContainerName"] ?? "raceday-images";
    builder.Services.AddSingleton<IFileStorageService>(
        new AzureBlobStorageService(blobConnection, containerName));
}
else
{
    string webRoot = builder.Environment.WebRootPath
        ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    builder.Services.AddSingleton<IFileStorageService>(new LocalFileStorageService(webRoot));
}

var app = builder.Build();

// Create / update the database and seed demo data on start-up.
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    DbSeeder.MigrateAndSeed(db);
}

// Configure the HTTP request pipeline.
// Swagger is enabled in every environment so it also works inside Docker.
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
