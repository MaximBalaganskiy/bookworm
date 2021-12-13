using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using Bookworm.Api.Models;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;

namespace Bookworm.Api.Functions
{
    public static class TopAuthors
    {
        [FunctionName(nameof(TopAuthors))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] 
            HttpRequest req,

            [CosmosDB(databaseName: "%DatabaseName%", collectionName: "Authors", ConnectionStringSetting = "ConnectionString")]
            DocumentClient authorsClient)
        {
            var countStr = req.Query["count"];
            if (!int.TryParse(countStr, out var count) || count < 0)
            {
                return new BadRequestObjectResult(new { Error = "Invalid count parameter" });
            }

            var authorsUri = UriFactory.CreateDocumentCollectionUri(Environment.GetEnvironmentVariable("DatabaseName"), "Authors");
            var query = authorsClient.CreateDocumentQuery<Author>(authorsUri, new FeedOptions { EnableCrossPartitionQuery = true })
                .OrderByDescending(x => x.Rating).ThenBy(x => x.Name)
                .Take(count)
                .AsDocumentQuery();
            var authors = new List<Author>();
            while (query.HasMoreResults)
            {
                authors.AddRange(await query.ExecuteNextAsync<Author>());
            }

            return new OkObjectResult(authors);
        }
    }
}
