using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CosmosSqlDb.Model
{
    public class Company
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public BusinessField BusinessField { get; set; }
        public List<Employee> Employees { get; set; }
    }
}
