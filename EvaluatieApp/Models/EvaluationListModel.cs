﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvaluatieApp.Models
{
    public class EvaluationListModel
    {
        public User user { get; set; }

        public List<Item> items { get; set; }
    }
}