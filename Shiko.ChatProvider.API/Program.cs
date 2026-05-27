
using Azure;
using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Data;
using Shiko.ChatProvider.API.Endpoints;
using Shiko.ChatProvider.API.Middleware;
using Shiko.ChatProvider.API.Security;
using Shiko.ChatProvider.API.Services;

var builder = WebApplication.CreateBuilder(args);



if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Configuration.AddEnvironmentVariables();


// check signinkey
var signingKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrEmpty(signingKey))
    throw new InvalidOperationException("SigningKey is empty!");

Console.WriteLine($"=== JWT CONFIG ===");
Console.WriteLine($"Issuer: '{builder.Configuration["Jwt:Issuer"]}'");
Console.WriteLine($"Audience: '{builder.Configuration["Jwt:Audience"]}'");
Console.WriteLine($"SigningKey length: {builder.Configuration["Jwt:SigningKey"]?.Length}");
Console.WriteLine($"==================");


// connection strings
var connectionString = builder.Configuration.GetConnectionString("SqlStorage");
var acsConnectionString = builder.Configuration["AzureCommunicationServices:ConnectionString"];

// ASC endpoint and key
var endpoint = new Uri(builder.Configuration["AzureCommunicationServices:Endpoint"]!);
var key = new AzureKeyCredential(builder.Configuration["AzureCommunicationServices:AccessKey"]!);

builder.Services.AddScoped<IChatRoomService, ChatRoomService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();


// register DbContext
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(connectionString));

//CORS options to allow requests from Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});


var app = builder.Build();

app.MapOpenApi();

app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  
}

app.UseHttpsRedirection();

app.UseCors("AllowNextJS");

app.UseAuthentication();
app.UseAuthorization();


app.UseExceptionHandler();

app.MapChatEndpoints();

app.Run();
