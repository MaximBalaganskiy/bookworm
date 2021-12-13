using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookworm.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bookworm.Api.Functions
{
    public static class AggregateRating
    {
        [FunctionName(nameof(AggregateRating))]
        public static async Task Run(
            [CosmosDBTrigger(databaseName: "%DatabaseName%",
                collectionName: "Ratings",
                ConnectionStringSetting = "ConnectionString",
                LeaseCollectionName = "RatingLeases",
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> input,

            [CosmosDB(databaseName: "%DatabaseName%", collectionName: "Authors", ConnectionStringSetting = "ConnectionString")]
            DocumentClient authorsClient)
        {
            if (input == null || input.Count == 0)
            {
                return;
            }

            var authorsUri = UriFactory.CreateDocumentCollectionUri(Environment.GetEnvironmentVariable("DatabaseName"), "Authors");
            var groups = input.Select(x => JsonConvert.DeserializeObject<Rating>(x.ToString()))
                .GroupBy(x => x.Author.ToUpper().Replace(" ", ""))
                .ToList();


            foreach (var g in groups)
            {
                var authorId = g.Key;

                var authorQuery = await authorsClient.CreateDocumentQuery<Author>(authorsUri)
                    .Where(x => x.Id == authorId)
                    .AsDocumentQuery()
                    .ExecuteNextAsync<Author>();
                var count = g.Count();
                var author = authorQuery.FirstOrDefault();
                if (author == null)
                {
                    author = new Author { Id = authorId, Name = g.First().Author, RatingCount = count, Rating = g.Average(x => x.RatingValue) };
                }
                else
                {
                    var newCount = author.RatingCount + count;
                    author.Rating = (author.Rating * author.RatingCount + g.Sum(x => x.RatingValue)) / newCount;
                    author.RatingCount = newCount;
                }
                await authorsClient.UpsertDocumentAsync(authorsUri, author);
            }
        }
    }
}
