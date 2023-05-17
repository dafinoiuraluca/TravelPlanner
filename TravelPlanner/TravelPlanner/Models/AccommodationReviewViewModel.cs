using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class AccommodationReviewViewModel
    {
        public int ReviewId { get; set; }
        public int AccommodationId { get; set; }
        public string AccommodationName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}