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

// Here i configure the backend API URL
#region backend url setup
var backendUrl = builder.Configuration["BackendUrl"] ?? "https://api.spacexdata.com/v3";
var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
#endregion

#region Welcome status endpoint
app.MapGet("/", () => 
{
    return Results.Text("Hello World iChoosr", contentType:"text/plain");
})
    .Produces<string>(StatusCodes.Status200OK, "text/plain")
    .WithSummary("Hello World iChoosr.")
    .WithDescription("This is root of the endpoint and after this will be other endpoint that we will work with.")
    .WithTags("Status")
    .WithOpenApi();
#endregion


#region All Payloads endpoint
app.MapGet("/payloads", async (HttpContext context) =>
{
    var client = httpClientFactory.CreateClient();
    var fullBackendURL = $"{backendUrl}/payloads";

    try
    {
        var response = await client.GetAsync(fullBackendURL);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex) 
    {
        return Results.Problem($"Error proxying request: {ex.Message}", statusCode: 500);
    }
})
    .WithName("SpaceXPayloads")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithDescription("Here we retrive space x payloads for all satelites.")
    .WithOpenApi();
#endregion


//Here for peyload i add few in description because retrival is done by name, so for easy use I add in description part in the endpoint
#region One payload endpoint
app.MapGet("/payloads/{{payload_id}}", async (string payloadName) =>
{
    var client = httpClientFactory.CreateClient();
    var fullBackendURL = $"{backendUrl}/payloads/{payloadName}";

    try
    {
        var response = await client.GetAsync(fullBackendURL);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error proxying request: {ex.Message}", statusCode: 500);
    }
})
    .WithName("SpaceXPayload")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithDescription("Here we retrive one space x payload. Here our most popular and available payloads from satelites: \n FalconSAT-2 , \n Trailblazer, \n PRESat, \n RatSat, \n RazakSAT, \n Dragon Qualification Unit")
    .WithOpenApi();
#endregion



app.Run();
