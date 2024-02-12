using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElectricityPrice.Models
{
    public class PriceData
    {
        [JsonProperty("prices")]
        public List<Price> PriceValues { get; set; }

        public PriceData()
        {
            PriceValues = new List<Price>();
        }
    }
}
