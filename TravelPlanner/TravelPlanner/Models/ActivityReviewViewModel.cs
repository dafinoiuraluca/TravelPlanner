using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class ActivityReviewViewModel
    {
        public int ReviewId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}