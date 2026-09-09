using SupportFlow.Modules.Organizations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SupportFlow")
                       ?? throw new InvalidOperationException("Connection string 'SupportFlow' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddOrganizationsModule(connectionString);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseStatusCodePages();
app.MapOrganizationsModule();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
