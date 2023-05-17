﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TravelPlanner
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "LeaveReview",
                url: "ReviewAccommodation/LeaveReview/{accommodationId}",
                defaults: new { controller = "ReviewAccommodation", action = "LeaveReview" }
                );


            routes.MapRoute(
                name: "ActivityView",
                url: "Activity/ActivityView",
                defaults: new { controller = "Activity", action = "ActivityView" }
                );

            routes.MapRoute(
                name: "NewActivity",
                url: "Activities/NewActivity",
                defaults: new { controller = "Activity", action = "NewActivity" }
            );

            routes.MapRoute(
                    name: "BookAccommodationView",
                    url: "Bookings/BookAccommodationView/{id}",
                    defaults: new { controller = "Bookings", action = "BookAccommodationView", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "UserSignUp",
                url: "UserController/SignUp",
                defaults: new { controller = "UserController", action = "SignUp" }
                );

            routes.MapRoute(
                name: "Login",
                url: "UserController/SignIn",
                defaults: new { controller = "UserController", action = "SignIn" }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
