using Microsoft.Azure.CosmosRepository;
using Newtonsoft.Json;

namespace CosmosRepositoryPagingSample;

class Person : FullItem
{
    public Person(
        string name, 
        int age, 
        string address)
    {
        Id = name;
        Name = name;
        Age = age;
        Address = address;
        PartitionKey = nameof(Person);
    }

    [JsonConstructor]
    private Person(
        string id, 
        string name, 
        int age, 
        string address, 
        string partitionKey)
    {
        Id = id;
        Name = name;
        Age = age;
        Address = address;
        PartitionKey = partitionKey;
    }
    public string Name { get; }
    public int Age { get; }
    public string Address { get; }
    public string PartitionKey { get; }

    protected override string GetPartitionKeyValue() =>
        PartitionKey;
}