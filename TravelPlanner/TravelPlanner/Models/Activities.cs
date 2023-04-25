using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class Activities
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ActivityDescription { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}