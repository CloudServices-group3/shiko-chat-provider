
using Azure;
using Azure.Communication.Identity;
using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Data;

var builder = WebApplication.CreateBuilder(args);

// connection strings
var connectionString = builder.Configuration.GetConnectionString("SqlStorage");
var acsConnectionString = builder.Configuration["AzureCommunicationServices:ConnectionString"];

// ASC endpoint and key
var endpoint = new Uri(builder.Configuration["AzureCommunicationServices:Endpoint"]!);
var key = new AzureKeyCredential(builder.Configuration["AzureCommunicationServices:AccessKey"]!);


builder.Services.AddSingleton(new CommunicationIdentityClient(acsConnectionString!));

builder.Services.AddSingleton(endpoint);

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



app.Run();
