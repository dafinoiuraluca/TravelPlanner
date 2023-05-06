using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    public class ReviewActivityController : Controller
    {
        string connectionString = "Data Source=DESKTOP-6A1HP7T;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: ReviewActivity
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LeaveReview(ReviewActivity review)
        {
            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string queryToInsertReview = "INSERT INTO ReviewActivities(Rating, Comment) VALUES (@Rating, @Comment)";
                        using (SqlCommand command = new SqlCommand(queryToInsertReview, conn))
                        {
                            command.Parameters.AddWithValue("@Rating", review.Rating);
                            command.Parameters.AddWithValue("@Comment", review.Comment);

                            command.ExecuteNonQuery();
                        }
                    }
                    return View("Success");
                }
                catch (Exception e)
                {
                    return View("Error" + e);
                }

            }
            return View(review);
        }
        [HttpGet]
        public ActionResult DisplayReview(int activityId)
        {
            List<ReviewActivity> reviewActivities = new List<ReviewActivity>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string queryToDisplayReview = "SELECT * FROM ReviewActivities WHERE ActivityId = @ActivityId";
                    using (SqlCommand command = new SqlCommand(queryToDisplayReview, conn))
                    {
                        command.Parameters.AddWithValue("@ActivityId", activityId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReviewActivity review = new ReviewActivity
                                {
                                    Rating = (int)reader["Rating"],
                                    Comment = reader["Comment"].ToString(),
                                    CreatedAt = (DateTime)reader["CreatedAt"]
                                };
                                reviewActivities.Add(review);
                            }
                        }
                    }
                }
                return View(reviewActivities);
            }
            catch (Exception e)
            {
                return View("Error" + e);
            }
        }
    }
}