// Import necessary namespaces
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Models;

// Create a web application using WebApplicationBuilder
var builder = WebApplication.CreateBuilder(args);

// Use in memory DB
// builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));

// Get the connection string for the database from the configuration, or use a default if not provided
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

// Add API Explorer services
builder.Services.AddEndpointsApiExplorer();

// Configure database services with SQLite and the provided connection string
builder.Services.AddSqlite<PizzaDb>(connectionString);

// Configure Swagger documentation generation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});

// Build the application
var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();

// Configure Swagger UI to display API documentation
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

// Define a simple endpoint for the root path
app.MapGet("/", () => "Hello World!");

// Define an endpoint to get all pizzas from the database
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

// Define an endpoint to add a new pizza to the database
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

// Define an endpoint to get a specific pizza by ID
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

// Define an endpoint to update a specific pizza by ID
app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Define an endpoint to delete a specific pizza by ID
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null)
    {
        return Results.NotFound();
    }
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Start the application
app.Run();
