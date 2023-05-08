using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Services;
using ChatApp.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using ChatApp.Exceptions;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Configuration
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobSettings>(builder.Configuration.GetSection("BlobStorage"));

builder.Services.AddSingleton(x =>
{
    var connectionString = builder.Configuration.GetSection("BlobStorage:ConnectionString").Value;
    return new BlobServiceClient(connectionString);
});
// Add Services


builder.Services.AddSingleton<IProfileStore, CosmosProfileStore>();
builder.Services.AddSingleton<IProfileService, ProfileService>();

builder.Services.AddSingleton<IImageStore, BlobImageStore>();
builder.Services.AddSingleton<IImageService, ImageService>();

builder.Services.AddSingleton<IMessageStore,CosmosMessageStore>();

builder.Services.AddSingleton<IConversationStore, CosmosConversationStore>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["BlobStorage:ConnectionString:blob"], preferMsi: true);
});
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
