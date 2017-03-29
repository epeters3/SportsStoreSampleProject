using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SportsStore.Domain.Entities;

namespace SportsStore.WebUI.Models
{
    public class ProductsListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string CurrentCategory { get; set; }
        public string JsonProducts {
            get
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var json = serializer.Serialize(Products.Select(p => p.Name));
                var decodedJson = HttpUtility.HtmlDecode(json);

                return json;
            }
        }
        public int NumProducts
        {
            get
            {
                int count = 0;
                foreach (Product p in Products)
                {
                    count++;
                }
                return count;
            }
        }
    }
}