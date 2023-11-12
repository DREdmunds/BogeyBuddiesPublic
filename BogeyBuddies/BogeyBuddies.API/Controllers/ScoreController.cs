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
    public class ScoreController
    {
        private Container _container;
        private readonly string _databaseId;
        private readonly string _containerId;

        public ScoreController(CosmosClient cosmosClient, IConfiguration configuration)
        {
            _databaseId = configuration["CosmosDbDatabaseId"];
            _containerId = configuration["CosmosDbContainerId"];
            _container = cosmosClient.GetContainer(_databaseId, _containerId);

        }

        [FunctionName("CreateWeeklyScore")]
        public async Task<IActionResult> CreateWeeklyScore([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var score = JsonConvert.DeserializeObject<WeeklyScore>(requestBody);

                ItemResponse<WeeklyScore> response = await _container.CreateItemAsync(score, new PartitionKey(score.UserId));  

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

        [FunctionName("GetAllUserScoresAsync")]
        public async Task<IActionResult> GetAllUserScoresAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "scores")] HttpRequest req, ILogger log)
        {
            // The userId usually would be grabbed from the auth token, it wouldn't be sent in.
            var userId = "user-001";

            var queryString = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @userId")
                    .WithParameter("@userId", userId);

            List<WeeklyScore> userScores = new List<WeeklyScore>();
            var iterator = _container.GetItemQueryIterator<WeeklyScore>(queryString);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                userScores.AddRange(response.ToList());
            }

            return new OkObjectResult(userScores);
        }
    }
}
