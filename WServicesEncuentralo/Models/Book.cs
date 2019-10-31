using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WServicesEncuentralo.Models
{
    public class Book
    {
        public string Store { get; set; }  
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Language { get; set; }
        public string Editorial { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
    }
}