using Microsoft.EntityFrameworkCore;
using MoviePlatform.Modules.Catalog.Application;
using MoviePlatform.Modules.Catalog.Infrastructure;
using MoviePlatform.Modules.Catalog.Infrastructure.Persistence;
using MoviePlatform.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "MoviePlatform API",
        Version = "v1",
        Description = "Full-stack movie platform API"
    });
});

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Register modules
builder.Services.AddSharedInfrastructure();
builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularClient");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedData.SeedAsync(dbContext);
}

app.Run();
