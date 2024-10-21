using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StealTheCatsAPI.Application.Database;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;
using StealTheCatsAPI.Application.Services;
using StealTheCatsAPI.Application.Validations;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "StealTheCats API",
        Description = "This API interacts with the Cats as a Service (CaaS) API to fetch and store cat images and associated metadata " +
        "into a local database and a file directory. The system uses ASP.NET Core 8 with Entity Framework Core for data storage, with support for Microsoft" +
        " SQL Server.Fetches 25 unique cat images from the CaaS API and stores them in the local database. " +
        "Includes information such as the image's CatId, Width, Height, and associated Tags (e.g., cat temperament). Duplicate entries are " +
        "prevented." +
        "\nRetrieves a paginated list of stored cats, with support for specifying the page number and page size," +
        " filtered by a specific tag (e.g., a cat's temperament like \"playful\"). " +
        "Supports pagination with page and pageSize query parameters.",
        Contact = new OpenApiContact
        {
            Name = "Batis-Odysseas Kapopoulos",
            Url = new Uri("https://github.com/BatisKap"),
        }
    });

    c.EnableAnnotations();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICatRepository, CatRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

builder.Services.AddScoped<IValidator<Cat>, CatValidator>();

builder.Services.Decorate<ICatRepository, CatRepositoryDecorator>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IImageDownloadService, ImageDownloadService>();

builder.Services.AddScoped<ICatService, CatService>();

builder.Services.AddHttpClient("ImageDownloadClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        MaxConnectionsPerServer = 10, // Set the maximum number of concurrent connections per server
    };
});

builder.Services.AddHttpClient("TheCatApiClient", config =>
{
    config.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["CatApiKey"]);
    config.BaseAddress = new Uri("https://api.thecatapi.com/v1/");
    config.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient("ImageDownloaderClient", config =>
{
    config.Timeout = TimeSpan.FromSeconds(60);
});

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
