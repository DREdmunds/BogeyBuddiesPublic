using BogeyBuddies.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BogeyBuddies.API.Controllers
{
    // Changed from static to instance class
    public class ScoreController
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger _logger;
        private readonly string _databaseId;
        private readonly string _containerId;

        // Constructor with dependency injection
        public ScoreController(CosmosClient cosmosClient, ILogger<ScoreController> logger, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
            _databaseId = configuration["CosmosDbDatabaseId"];
            _containerId = configuration["CosmosDbContainerId"];

        }

        [FunctionName("CreateWeeklyScore")]
        public async Task<IActionResult> CreateWeeklyScore(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var score = JsonConvert.DeserializeObject<WeeklyScore>(requestBody);
                var container = _cosmosClient.GetContainer(_databaseId, _containerId);

                ItemResponse<WeeklyScore> response = await container.CreateItemAsync(score, new PartitionKey(score.UserId));  

                return new OkObjectResult(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                log.LogError($"Item in conflict. Error: {ex.Message}");
                return new ConflictObjectResult(ex.Message);
            }
            catch (CosmosException ex)
            {
                log.LogError($"Cosmos DB Exception. Status code: {ex.StatusCode}, Error: {ex.Message}");
                return new ObjectResult(ex.Message) { StatusCode = (int)ex.StatusCode };
            }
            catch (Exception ex)
            {
                log.LogError($"Internal server error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("GetWeeklyScores")]
        public async Task<IActionResult> GetWeeklyScores(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "scores/week/{weekIdentifier}")] HttpRequest req,
    string weekIdentifier,
    ILogger log)
        {
            string cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
            CosmosClient cosmosClient = new CosmosClient(cosmosConnectionString);

            var queryString = new QueryDefinition("SELECT * FROM c WHERE c.weekIdentifier = @weekIdentifier")
                .WithParameter("@weekIdentifier", weekIdentifier);

            List<WeeklyScore> weeklyScores = new List<WeeklyScore>();
            var container = cosmosClient.GetContainer(_databaseId, _containerId);
            var iterator = container.GetItemQueryIterator<WeeklyScore>(queryString);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                weeklyScores.AddRange(response.ToList());
            }

            // Set CORS headers
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)req.Headers["Origin"] });
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");

            // Now create and return the result with the headers set
            return new OkObjectResult(weeklyScores);
        }

        [FunctionName("GetUserScores")]
        public async Task<IActionResult> GetUserScores(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "scores/user/{userId}")] HttpRequest req,
    string userId,
    ILogger log)
        {
           // var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
          //  var databaseId = Environment.GetEnvironmentVariable("CosmosDbDatabaseId");
           // var containerId = Environment.GetEnvironmentVariable("CosmosDbContainerId");
          //  var cosmosClient = new CosmosClient(cosmosConnectionString);

            var queryString = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                    .WithParameter("@userId", userId);

            List<WeeklyScore> userScores = new List<WeeklyScore>();
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var iterator = container.GetItemQueryIterator<WeeklyScore>(queryString);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                userScores.AddRange(response.ToList());
            }

            return new OkObjectResult(userScores);
        }


    }
}
