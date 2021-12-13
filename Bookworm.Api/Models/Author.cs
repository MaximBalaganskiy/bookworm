using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookworm.Api.Models
{
    public class Author
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public int RatingCount { get; set; }
        public double Rating { get; set; }
    }
}
