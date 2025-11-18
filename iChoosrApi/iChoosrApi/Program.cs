var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "openapi/{documentname}.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "MyiChoosrApi v1");
        c.RoutePrefix = "";
    });
}

app.MapGet("/", () => 
{
    return Results.Text("Hello World iChoosr", contentType:"text/plain");
})
    .Produces<string>(StatusCodes.Status200OK, "text/plain")
    .WithSummary("Hello World iChoosr.")
    .WithDescription("This is root of the endpoint and after this will be other endpoint that we will work with.")
    .WithTags("Status")
    .WithOpenApi();

app.Run();
