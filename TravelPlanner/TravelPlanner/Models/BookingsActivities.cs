using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class BookingsActivities
    {
        public int BookingsActivitiesId { get; set; }
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public TimeSpan StartHour { get; set; }
        public TimeSpan StopHour { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}