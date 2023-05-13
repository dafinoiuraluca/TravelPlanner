using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class BookAccommodationViewModel
    {
        public int AccommodationId { get; set; }
        public string AccommodationName { get; set; }
        public string AccommodationDescription { get; set; }
        public string AccommodationType { get; set; }
        public string AccommodationLocation { get; set; }
        public decimal Price { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}