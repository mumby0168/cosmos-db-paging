using CosmosRepositoryPagingSample;
using Microsoft.Azure.CosmosRepository;

var builder = WebApplication.CreateBuilder(args);

const string databaseName = "people-database";
const string peopleContainerName = "people-container";
const string partitionKey = "/partitionKey";

builder.Services.AddCosmosRepository(options =>
{
    // In order to set this connection string run
    // dotnet user-secrets set RepositoryOptions:CosmosConnectionString "<your-connection-string>
    // options.CosmosConnectionString = "<taken-from-config>";

    options.DatabaseId = databaseName;
    options.ContainerPerItemType = true;

    options.ContainerBuilder
        .Configure<Person>(containerOptionsBuilder =>
    {
        containerOptionsBuilder
            .WithContainer(peopleContainerName)
            .WithPartitionKey(partitionKey);
    });
});

var app = builder.Build();


app.MapGet("/", () => "Azure Cosmos Repository Paging Sample");

app.MapGet("/generate", async (IRepository<Person> repository) =>
{
    var tasks = new List<Task>();

    for (var i = 0; i < 100; i++)
    {
        var personBuilder = new Bogus.Person();

        var person = new Person(
            personBuilder.FullName,
            personBuilder.Random.Number(10, 80),
            personBuilder.Address.Street);

        tasks.Add(repository.CreateAsync(person).AsTask());
    }

    await Task.WhenAll(tasks);
});

app.Run();