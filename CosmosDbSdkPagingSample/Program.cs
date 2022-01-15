using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Database = Microsoft.Azure.Cosmos.Database;
using Person = CosmosDbSdkPagingSample.Person;

var builder = WebApplication.CreateBuilder(args);

const string databaseName = "people-database";
const string peopleContainerName = "people-container";
const string partitionKey = "/partitionKey";

var connectionString = builder.Configuration.GetConnectionString("Cosmos");
builder.Services.AddSingleton(() => new CosmosClient(connectionString));

var app = builder.Build();

Container? peopleContainer = null;

async Task<Container> GetPeopleContainer(CosmosClient cosmosClient)
{
    if (peopleContainer is not null)
    {
        return peopleContainer;
    }

    Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
    var container = await database.CreateContainerIfNotExistsAsync(peopleContainerName, partitionKey);
    peopleContainer = container;

    return peopleContainer;
}

static async Task<(List<Person> items, double charge, string? continuationToken)> GetAllItemsAsync(
    IQueryable<Person> query,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    string? continuationToken = null;
    List<Person> results = new();
    int readItemsCount = 0;
    double charge = 0;
    using FeedIterator<Person> iterator = query.ToFeedIterator();
    
    while (readItemsCount < pageSize && iterator.HasMoreResults)
    {
        FeedResponse<Person> next = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

        foreach (Person result in next)
        {
            if (readItemsCount == pageSize)
            {
                break;
            }

            results.Add(result);
            readItemsCount++;
        }

        charge += next.RequestCharge;
        continuationToken = next.ContinuationToken;
    }

    return (results, charge, continuationToken);
}

app.MapGet("/generate", async (CosmosClient client) =>
{
    var container = await GetPeopleContainer(client);

    var tasks = new List<Task>();

    for (var i = 0; i < 100; i++)
    {
        var personBuilder = new Bogus.Person();

        var person = new Person(
            personBuilder.FullName,
            personBuilder.Random.Number(10, 80),
            personBuilder.Address.Street);

        tasks.Add(container.CreateItemAsync(person, new PartitionKey(person.PartitionKey)));
    }

    await Task.WhenAll(tasks);
});


app.MapGet("/skipTake", async (
    CosmosClient client,
    [FromQuery] int pageNumber,
    [FromQuery] int pageSize) =>
{
    var container = await GetPeopleContainer(client);

    QueryRequestOptions queryOptions = new() {MaxItemCount = pageSize};

    IQueryable<Person> query = container
        .GetItemLinqQueryable<Person>(requestOptions: queryOptions)
        .Where(x => x.PartitionKey == nameof(Person))
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize);

    var (items, charge, _) =
        await GetAllItemsAsync(query, pageSize);

    return new
    {
        people = items,
        requestUnits = charge
    };
});

app.MapGet("/tokenBased", async (
    CosmosClient client,
    [FromQuery] string? continuationToken,
    [FromQuery] int pageSize) =>
{
    var container = await GetPeopleContainer(client);

    QueryRequestOptions queryOptions = new() {MaxItemCount = pageSize};

    IQueryable<Person> query = container
        .GetItemLinqQueryable<Person>(requestOptions: queryOptions, continuationToken: continuationToken)
        .Where(x => x.PartitionKey == nameof(Person));

    var (items, charge, newContinuationToken) =
        await GetAllItemsAsync(query, pageSize);

    return new
    {
        people = items,
        requestUnits = charge,
        continuationToken = newContinuationToken
    };
});


app.Run();