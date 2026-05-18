
using Azure;
using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Data;
using Shiko.ChatProvider.API.Endpoints;
using Shiko.ChatProvider.API.Middleware;
using Shiko.ChatProvider.API.Services;

var builder = WebApplication.CreateBuilder(args);

// connection strings
var connectionString = builder.Configuration.GetConnectionString("SqlStorage");
var acsConnectionString = builder.Configuration["AzureCommunicationServices:ConnectionString"];

// ASC endpoint and key
var endpoint = new Uri(builder.Configuration["AzureCommunicationServices:Endpoint"]!);
var key = new AzureKeyCredential(builder.Configuration["AzureCommunicationServices:AccessKey"]!);

builder.Services.AddScoped<IChatRoomService, ChatRoomService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();


// register DbContext
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.MapChatEndpoints();

app.Run();
