using Newtonsoft.Json;

namespace ElectricityPrice.Models
{
    public class Price
    {
        [JsonProperty("price")]
        public double PriceValue { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
        [JsonProperty("endDate")]
        public string EndDate { get; set; }
    }
}
