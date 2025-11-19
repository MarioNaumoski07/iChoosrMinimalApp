using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration["JwtKey"] ?? "THIS_IS_very_long_random_secret_key_here_change_this_to_env_value_!@#123";
string? MyToken;

#region Authentication + Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RegularPolicy", policy =>
        policy.RequireRole("User", "Admin"));  // users + admins

    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});
#endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token after you log in",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#region hardcoded users
var users = new List<(string Username, string Password, string[] Roles)>
{
    ("admin", "123", new[] { "Admin", "User" }),
    ("mario", "123", new[] { "User" })
};
#endregion

#region Generate Token
string GenerateJwt(string username, string[] roles)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, username)
    };

    foreach (var role in roles)
        claims.Add(new Claim(ClaimTypes.Role, role));

    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(1),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
#endregion

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
        c.SwaggerEndpoint("/openapi/v1.json", "MyiChoosrApi");
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
    .WithOpenApi()
    .RequireAuthorization("AdminPolicy");
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
    .WithOpenApi()
    .RequireAuthorization("RegularPolicy");
#endregion

#region Login endpoint
app.MapGet("/login", (string username, string password) =>
{
    var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
    if (user == default)
        return Results.Unauthorized();

    MyToken = GenerateJwt(user.Username, user.Roles);
    return Results.Ok(new { MyToken });
})
    .WithName("Login users")
    .WithDescription("Please after you login, copy the bearer token and past in swagger popup when you click on authorization button so you will be able to execute api endpoints.")
    .WithOpenApi();
#endregion


app.Run();
