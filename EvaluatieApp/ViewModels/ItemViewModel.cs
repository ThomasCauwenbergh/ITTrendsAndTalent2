using EvaluatieApp.Models;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvaluatieApp.ViewModels
{
    public class ItemViewModel
    {
        public IEnumerable<Item> Items1{ get; set; }
        public IEnumerable<Item> Items2 { get; set; }
        public IEnumerable<Item> Items3 { get; set; }
        public IEnumerable<Item> Items4 { get; set; }

        public List<string> types { get; set; }
        public IUser user { get; set; }

        public DateTime date { get; set; }


        public ItemViewModel()
        {
            types = new List<string>();
            types.Add("Onderwijs");
            types.Add("Onderzoek");
            types.Add("Dienstverlening");
            types.Add("Evenementen");
            date = DateTime.Now;
        }
    }
}