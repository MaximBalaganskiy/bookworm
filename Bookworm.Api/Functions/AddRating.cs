using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Bookworm.Api.Models;

namespace Bookworm.Api.Functions
{
    public static class AddRating
    {
        [FunctionName(nameof(AddRating))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,

            [CosmosDB(databaseName: "%DatabaseName%", 
                collectionName: "Ratings", 
                ConnectionStringSetting = "ConnectionString", 
                CreateIfNotExists = true, 
                PartitionKey = "/Author")]
            IAsyncCollector<Rating> documentsOut)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var rating = JsonConvert.DeserializeObject<Rating>(requestBody);

            if (rating == null)
            {
                return new BadRequestObjectResult(new { Error = "Request body is empty" });
            }

            rating.Author = rating.Author?.Trim();
            if (string.IsNullOrWhiteSpace(rating.Author))
            {
                return new BadRequestObjectResult(new { Error = "Author is empty" });
            }

            rating.Title = rating.Title?.Trim();
            if (string.IsNullOrWhiteSpace(rating.Title))
            {
                return new BadRequestObjectResult(new { Error = "Title is empty" });
            }

            if (rating.RatingValue < 1 || rating.RatingValue > 5)
            {
                return new BadRequestObjectResult(new { Error = "Rating must be between 1 and 5" });
            }

            rating.ISBN = rating.ISBN?.Trim();

            await documentsOut.AddAsync(rating);

            return new OkResult();
        }
    }
}
