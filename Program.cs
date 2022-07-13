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
    ip = builder.Configuration["WEBSITE_PRIVATE_PORTS"];

    Console.WriteLine($"IP in orleans: {ip}");
    
    var endpointAddress = IPAddress.Parse(builder.Configuration["WEBSITE_PRIVATE_IP"]);
    var strPorts = builder.Configuration["WEBSITE_PRIVATE_PORTS"].Split(',');
    if (strPorts.Length < 2)
        throw new Exception("Insufficient private ports configured.");
    var (siloPort, gatewayPort) = (int.Parse(strPorts[0]), int.Parse(strPorts[1]));
    var connectionString = builder.Configuration["ORLEANS_AZURE_STORAGE_CONNECTION_STRING"];
    
    Console.WriteLine($"____ USING endpoints {endpointAddress} {siloPort} {gatewayPort}");

    siloBuilder
        .ConfigureEndpoints(endpointAddress, siloPort, gatewayPort)
        .Configure<ClusterOptions>(
            options =>
            {
                options.ClusterId = "SimpleCluster";
                options.ServiceId = "SimpleService";
            }).UseAzureStorageClustering(options => options.ConfigureTableServiceClient(connectionString));
    //siloBuilder.AddAzureTableGrainStorageAsDefault(options => options.ConfigureTableServiceClient(connectionString));
});

// Add services to the container.
//
// builder.Host.UseOrleans(c =>
// {
//     c.UseDashboard();
//     c.Configure<ClusterOptions>(options =>
//     {
//         options.ClusterId = "comicDownloader";
//         options.ServiceId = "ComicDownloader";
//     });
//
//     Console.WriteLine("____ Configuring Orleans");
//
//     // var endpointAddress = IPAddress.Parse(builder.Configuration["WEBSITE_PRIVATE_IP"]);
//     // var strPorts = builder.Configuration["WEBSITE_PRIVATE_PORTS"].Split(',');
//     // if (strPorts.Length < 2)
//     //     throw new Exception("Insufficient private ports configured.");
//     // var (siloPort, gatewayPort) = (int.Parse(strPorts[0]), int.Parse(strPorts[1]));
//     // var connectionString = builder.Configuration["ORLEANS_AZURE_STORAGE_CONNECTION_STRING"];
//     //
//     // Console.WriteLine($"____ USING endpoints {endpointAddress} {strPorts} {gatewayPort}");
//
//
//     //c.ConfigureEndpoints(endpointAddress, siloPort, gatewayPort);
//     //c.UseAzureStorageClustering(options => options.ConfigureTableServiceClient(builder.Configuration.GetValue<string>("azurestorage:connectionstring")));
//     c.UseLocalhostClustering();
//     c.AddMemoryGrainStorageAsDefault();
//     c.ConfigureLogging(logging => logging.AddConsole());
//
//     c.ConfigureApplicationParts(manager =>
//         manager.AddApplicationPart(Assembly.GetExecutingAssembly()).WithReferences());
//
//     Console.WriteLine($"____ Configured Orleans");
//
//     c.AddStartupTask(async (provider, token) =>
//     {
//         var helloWorld = provider.GetService<IGrainFactory>().GetGrain<IHelloWorld>(Guid.NewGuid());
//         var result = await helloWorld.Hello();
//         Console.WriteLine($"____ Result: {result}");
//     });
// });

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

app.MapGet("", () => ip);
app.MapGet("grain/{id}", (string id, IGrainFactory grainfactory) => grainfactory.GetGrain<IHelloWorld>(id).Hello());
app.MapGet("grain/{id}/{data}", (string id, string data, IGrainFactory grainfactory) => grainfactory.GetGrain<IHelloWorld>(id).Write(data));

//app.UseOrleansDashboard();
app.Run();
