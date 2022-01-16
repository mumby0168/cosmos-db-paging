# Getting started

See my blog post that talks through this example in more detail [here](https://billy-mumby-blog.hashnode.dev/paging-in-azure-cosmos-db).

You will need the .NET 6 SDK installed and either VS Code, VS 2022 or the latest version of rider to edit this code.

I have also included the Postman collection for both samples under the `/Postman` directory.

### Connection Strings

In order to use the samples you will need a Cosmos DB database and a connection string.

To set the connection string you can use [`dotnet user-secrets`](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#set-a-secret).

#### .NET SDK Example

`dotnet user-secrets set "ConnectionStrings:Cosmos" "<your-cosmos-connection-string"`

#### Cosmos Repository Example

`dotnet user-secrets set "RepositoryOptions:CosmosConnectionString" "<your-cosmos-connection-string"`
