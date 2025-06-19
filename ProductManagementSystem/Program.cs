using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ProductManagementSystem.Interfaces;
using ProductManagementSystem.Application;
using ProductManagementSystem.Infrastructure;
using ProductManagementSystem.Presentation.Middleware;
using Amazon.Runtime;
using Amazon.DynamoDBv2.Model;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    // To enable CloudWatch logging, configure the sink as per Serilog.Sinks.AmazonCloudWatch documentation
    //.WriteTo.AmazonCloudWatch(...)
    .CreateLogger();

try
{
    Log.Information("Starting up Product Management System...");
    
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables();
    
    // Add Serilog to the logging pipeline
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddAWSService<IAmazonDynamoDB>();
    builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
    builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
    {
        string dynamoDbEndpoint = Environment.GetEnvironmentVariable("DYNAMODB_ENDPOINT") ?? builder.Configuration["DynamoDB:Endpoint"] ?? "http://localhost:8000";
        string awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? builder.Configuration["AWS:Region"] ?? "us-east-1";
        AmazonDynamoDBConfig clientConfig = new()
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion)
        };

        if (builder.Environment.IsDevelopment())
        {
            clientConfig.ServiceURL = dynamoDbEndpoint;
            clientConfig.UseHttp = true;

            var devAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID_DEV") ?? "dummy_key";
            var devSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY_DEV") ?? "dummy_secret";
            return new AmazonDynamoDBClient(new BasicAWSCredentials(devAccessKey, devSecretKey), clientConfig);
        }
        else
            return new AmazonDynamoDBClient(clientConfig);
    });

    // Use environment variable for DynamoDB table name (multi-env support)
    var tableName = builder.Configuration["DynamoDB:TableName"] ?? Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? "Products-dev";

    builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
    builder.Services.AddScoped<IProductsService, ProductsService>();
    builder.Services.AddHealthChecks();
    builder.Services.AddProblemDetails();

    WebApplication app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        var dynamoDb = app.Services.GetRequiredService<IAmazonDynamoDB>();
        var existingTables = await dynamoDb.ListTablesAsync();

        if (!existingTables.TableNames.Contains(tableName))
        {
            Log.Information("Creating DynamoDB table: {TableName}", tableName);
            var createRequest = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Id", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Id", KeyType.HASH)
                },
                ProvisionedThroughput = new ProvisionedThroughput(5, 5)
            };

            await dynamoDb.CreateTableAsync(createRequest);
            Log.Information("DynamoDB table created successfully: {TableName}", tableName);
        }
    }

    // Always enable Swagger UI, even in production
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });

    // Add a friendly root endpoint
    app.MapGet("/", () => Results.Redirect("/swagger"));

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseHttpsRedirection();
    app.UseCors();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Application starting...");
    app.Run();
    Log.Information("Application stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
