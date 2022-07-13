using System.Net;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using orleans_simple;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Test data");

var ip = "not run";

Console.WriteLine($"IP: {ip}");

builder.Host.UseOrleans(siloBuilder =>
{
    if (builder.Environment.IsDevelopment())
    {
        siloBuilder.UseLocalhostClustering()
            .AddMemoryGrainStorageAsDefault();
    }
    else
    {
        var siloPort = 11111;
        var gatewayPort = 30000;

        // Is running as an App Service?
        if (builder.Configuration["WEBSITE_PRIVATE_IP"] is string ip &&
            builder.Configuration["WEBSITE_PRIVATE_PORTS"] is string ports)
        {
            var endpointAddress = IPAddress.Parse(ip);
            var splitPorts = ports.Split(',');
            if (splitPorts.Length < 2) throw new Exception("Insufficient private ports configured.");

            siloPort = int.Parse(splitPorts[0]);
            gatewayPort = int.Parse(splitPorts[1]);

            siloBuilder.ConfigureEndpoints(endpointAddress, siloPort, gatewayPort);
        }
        else // Assume Azure Container Apps.
        {
            siloBuilder.ConfigureEndpoints(siloPort, gatewayPort);
        }

        var connectionString = builder.Configuration["ORLEANS_AZURE_STORAGE_CONNECTION_STRING"];

        siloBuilder.Configure<ClusterOptions>(
                options =>
                {
                    options.ClusterId = "SimpleCluster";
                    options.ServiceId = "SimpleOrleans";
                })
            .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(connectionString))
            .AddAzureTableGrainStorageAsDefault(options => options.ConfigureTableServiceClient(connectionString));
    }
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("", () => "Everything is OK");
app.MapGet("grain/{id}", (string id, IGrainFactory grainfactory) => grainfactory.GetGrain<IHelloWorld>(id).Hello());
app.MapGet("grain/{id}/{data}", (string id, string data, IGrainFactory grainfactory) => grainfactory.GetGrain<IHelloWorld>(id).Write(data));

//app.UseOrleansDashboard();
app.Run();