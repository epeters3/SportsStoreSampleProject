using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// This model is used to pass information to our List View so it knows
// how to properly do pagination.
// This view model is not part of the domain model. It's just a convenient way
// to pass data between the controller and view.
namespace SportsStore.WebUI.Models
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
    }
}