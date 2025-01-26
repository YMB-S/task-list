using TaskList;
using TaskList.Core;

// CLI Mode
if (args.Length > 0)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddScoped<IConsole, RealConsole>();
    builder.Services.AddScoped<ITaskListService, TaskListService>();
    builder.Services.AddScoped<TaskList.TaskList>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var taskList = scope.ServiceProvider.GetRequiredService<TaskList.TaskList>();
        taskList.Run();
    }
}
// Web mode
else
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();

    builder.Services.AddScoped<IConsole, RealConsole>();
    builder.Services.AddScoped<ITaskListService, TaskListService>();
    builder.Services.AddScoped<TaskList.TaskList>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
