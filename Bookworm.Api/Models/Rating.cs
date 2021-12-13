using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookworm.Api.Models
{
    public class Rating
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public int RatingValue { get; set; }
    }
}
