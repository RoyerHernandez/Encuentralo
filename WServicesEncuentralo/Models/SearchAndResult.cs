using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WServicesEncuentralo.Models
{
    public class Search
    {
        public string Criteria { get; set; }
    }

    public class Result
    {
        public List<Book> Books { get; set; }
    }
}