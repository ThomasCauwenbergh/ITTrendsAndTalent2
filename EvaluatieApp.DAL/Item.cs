//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EvaluatieApp.DAL
{
    using EvaluatieApp.DAL.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(ItemMetaData))]
    public partial class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<int> AuthorId { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<double> rating { get; set; }
    }
}