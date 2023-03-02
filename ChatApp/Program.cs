using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Configuration
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<BlobSettings>(builder.Configuration.GetSection("BlobSettings"));
builder.Services.AddSingleton(x =>
{
    var connectionString = builder.Configuration.GetSection("BlobStorage:ConnectionString").Value;
    return new BlobServiceClient(connectionString);
});
// Add Services
builder.Services.AddSingleton<IProfileStore, CosmosProfileStore>();
builder.Services.AddSingleton<IImageStore, BlobImageStore>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
