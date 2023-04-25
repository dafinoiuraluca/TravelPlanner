﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;
using Microsoft.AspNet.Identity;

namespace TravelPlanner.Controllers
{
    public class ActivityController : Controller
    {
        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: Activity
        public ActionResult Index()
        {
            return View();
        }

        //ADD ACTIVITY
        [HttpPost]
        public ActionResult CreateActivity(Activities activity)
        {
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string queryToInsertActivity = "INSERT INTO Activities(ActivityName, ActivityType, ActivityDescription, Price, CreatedAt, UpdatedAt) VALUES (@ActivityName, @ActivityType, @ActivityDescription, @Price, @CreatedAt, @UpdatedAt)";

                using (SqlCommand command = new SqlCommand(queryToInsertActivity, conn))
                {
                    command.Parameters.AddWithValue("@ActivityName", activity.ActivityName);
                    command.Parameters.AddWithValue("@ActivityType", activity.ActivityType);
                    command.Parameters.AddWithValue("@ActivityDescription", activity.ActivityDescription);
                    command.Parameters.AddWithValue("@Price", activity.Price);
                    command.Parameters.AddWithValue("@CreatedAt", activity.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", activity.UpdatedAt);

                    command.ExecuteNonQuery();
                }
            }

            return Content("Activity Added Successfully");
        }


        // BOOK ACTIVITY
        [HttpPost]
        public ActionResult BookActivity(int activityId)
        {
            string queryToBookActivity = "SELECT * FROM Activities where UserId = @UserId";

            using(SqlConnection conn = new SqlConnection(connectionString)) 
            {
                SqlCommand command = new SqlCommand(queryToBookActivity, conn);

                command.Parameters.AddWithValue("@UserId", User.Identity.GetUserId()); //getuserid model

                conn.Open();

                SqlDataReader reader = command.ExecuteReader();

                if(!reader.HasRows)
                {
                    return HttpNotFound();
                }

                reader.Close();

                string queryToInsertInBookings = "INSERT INTO Bookings (UserId, ActivityId, CreatedAt, UpdatedAt) VALUES (@UserId, @ActivityId, @CreatedAt, @UpdatedAt)";

                SqlCommand commandBooking = new SqlCommand(queryToInsertInBookings, conn);
                commandBooking.Parameters.AddWithValue("@UserId", User.Identity.GetUserId());
                commandBooking.Parameters.AddWithValue("@ActivityId", activityId);
                commandBooking.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                commandBooking.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                command.ExecuteNonQuery();

                conn.Close();
            }
            return Content("Activity booked");
        }

        //WISH LIST - cred ca trebe inca un tabel in DB cu UserId si ActivityId

        // SEARCH
        public ActionResult SearchActivity(string searchActivty)
        {
            List<Activities> activities = new List<Activities>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string queryToSearch = "SELECT * FROM Activities WHERE ActivityName LIKE @searchActivty";
                using (SqlCommand command = new SqlCommand(queryToSearch, conn))
                {
                    command.Parameters.AddWithValue("@searchActivty", "%" + searchActivty + "%");
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Activities activity = new Activities
                        {
                            ActivityId = (int)reader["ActivityId"],
                            ActivityName = reader["ActivityName"].ToString(),
                            ActivityType = reader["ActivityType"].ToString(),
                            ActivityDescription = reader["ActivityDescription"].ToString(),
                            Price = (decimal)reader["Price"],
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            UpdatedAt = (DateTime)reader["UpdatedAt"]
                        };
                        activities.Add(activity);
                    }
                }
            }
            return View("ViewActivities", activities);
        }
        //Check If Is Already In WishList
    }
}