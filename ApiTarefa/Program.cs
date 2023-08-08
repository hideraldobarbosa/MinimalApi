using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
 opt.UseInMemoryDatabase("TaskDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!!");

app.MapGet("sentences", async () => await new HttpClient().GetStringAsync("http://ron-swanson-quotes.herokuapp.com/v2/quotes"));

app.MapGet("/task", async (AppDbContext db) => await db.task.ToListAsync());

app.MapPost("/task", async (Task task, AppDbContext db) =>
{
    db.task.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/task/{task.Id}", task);
});

app.MapGet("/task/{id}", async (int id, AppDbContext db) =>
  await db.task.FindAsync(id) is Task task ? Results.Ok(task) : Results.NotFound());

app.MapGet("/task/taskDone", async (AppDbContext db) =>
  await db.task.Where(w => w.IsDone).ToListAsync());

app.MapPut("/task/{id}", async (int id, Task inputTask, AppDbContext db) =>
{
    var task = await db.task.FindAsync(id);
    if (task is null) return Results.NotFound();
    task.Name = inputTask.Name;
    task.IsDone = inputTask.IsDone;

    await db.SaveChangesAsync();

    return Results.Ok(task);

});

app.MapDelete("/task/{id}", async (int id, AppDbContext db) =>
{
    if (await db.task.FindAsync(id) is Task task)
    {
        db.task.Remove(task);
        await db.SaveChangesAsync();
        return Results.Ok(task);
    }
    return Results.NotFound("Task not found in this context.");
});


app.Run();

class Task
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsDone { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<Task> task => Set<Task>();

}