using Microsoft.EntityFrameworkCore;
using OWASPVulnerableApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database context (vulnerable configuration)
builder.Services.AddDbContext<VulnerableDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=VulnerableApp;Integrated Security=true;"));

var app = builder.Build();

// Enable detailed error messages (A05 - Security Misconfiguration)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();

app.Run();
