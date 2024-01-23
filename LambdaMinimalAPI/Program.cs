using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Auth config
string cognitoAppClientId = builder.Configuration["Cognito:AppClientId"].ToString();
string cognitoUserPoolId = builder.Configuration["Cognito:UserPoolId"].ToString();
string cognitoAWSRegion = builder.Configuration["Cognito:AWSRegion"].ToString();

string validIssuer = $"https://cognito-idp.{cognitoAWSRegion}.amazonaws.com/{cognitoUserPoolId}";
string validAudience = cognitoAppClientId;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = validIssuer;
        options.TokenValidationParameters = new ()
        {
            ValidateLifetime = true,
            ValidAudience = validAudience,
            ValidateAudience = true,
        };
    });


// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");
app.MapGet("/greetings", (string? name) => $"Greetings {name}");
app.MapGet("/claims", (ClaimsPrincipal? user) => string.Join(Environment.NewLine, user?.Claims?.Select(c => $"{c.Type} - {c.Value}") ?? Array.Empty<string>()));
app.MapGet("/user-info", (ClaimsPrincipal? user) => new { user?.Identity?.AuthenticationType, IsAutheticated = user?.Identity?.IsAuthenticated == true, user?.Identity?.Name});

app.Run();
