using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvaluatieApp.Models
{
    public class UserListModel
    {
        [JsonProperty("Value")]
        public List<User> users { get; set; }
    }
}