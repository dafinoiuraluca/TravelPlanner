﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FullName { get; set; }
        public string ValidatePassword { get; set; }
    }
}