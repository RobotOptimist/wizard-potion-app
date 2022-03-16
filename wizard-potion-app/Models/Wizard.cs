using Newtonsoft.Json;
using System.Text.Json;

namespace wizard_potion_app.Models
{
    public class Wizard
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string User { get; set; }
        public string Name { get; set; }
    }
}
