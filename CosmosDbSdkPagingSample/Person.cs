using Newtonsoft.Json;

namespace CosmosDbSdkPagingSample;

record Person
{
    public Person(string name, int age, string address)
    {
        Id = name;
        Name = name;
        Age = age;
        Address = address;
        PartitionKey = nameof(Person);
    }

    [JsonConstructor]
    private Person(string id, string name, int age, string address, string partitionKey)
    {
        Id = id;
        Name = name;
        Age = age;
        Address = address;
        PartitionKey = partitionKey;
    }

    public string Id { get; }
    public string Name { get; }
    public int Age { get; }
    public string Address { get; }
    
    public string PartitionKey { get; }
}