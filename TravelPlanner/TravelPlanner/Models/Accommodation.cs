using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class Accommodation
    {
        public int AccommodationId { get; set; }
        public string AccomodationName { get; set; }
        public string AccommodationType { get; set; }
        public string AccommodationLocation { get; set; }
        public decimal Price { get; set; }
        public string AccomodationDescription { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}