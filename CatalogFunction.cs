using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class CatalogFunction
{
    private static CosmosClient cosmosClient = new CosmosClient("your-cosmosdb-connection-string");
    private static Database database = cosmosClient.GetDatabase("NetflixCatalog");
    private static Container container = database.GetContainer("Catalogs");

    [FunctionName("AddCatalog")]
    public static async Task<HttpResponseMessage> AddCatalog(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "catalog")] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request to add a catalog.");

        var content = await req.Content.ReadAsStringAsync();
        var catalog = JsonConvert.DeserializeObject<Catalog>(content);

        // Insira o catálogo no Cosmos DB
        await container.CreateItemAsync(catalog, new PartitionKey(catalog.Id.ToString()));

        return req.CreateResponse(HttpStatusCode.Created, catalog);
    }

    [FunctionName("GetCatalogs")]
    public static async Task<HttpResponseMessage> GetCatalogs(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "catalog")] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request to get catalogs.");

        var query = container.GetItemQueryIterator<Catalog>("SELECT * FROM c");
        List<Catalog> catalogs = new List<Catalog>();

        while (query.HasMoreResults)
        {
            var result = await query.ReadNextAsync();
            catalogs.AddRange(result);
        }

        return req.CreateResponse(HttpStatusCode.OK, catalogs);
    }
}

public record Catalog(Guid Id, string Title, string Description, string Genre, string ReleaseYear);
