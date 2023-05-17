using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    public class ReviewActivityController : Controller
    {
        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: ReviewActivity
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StoreAccommodationIdAndRedirectReviewActivity(int activityId, string actionName, string controllerName)
        {
            Session["ActivityId"] = activityId;
            return RedirectToAction(actionName, controllerName);
        }

        [HttpPost]
        public ActionResult LeaveReview(ActivityReviewViewModel model, int activityId)
        {
            if (ModelState.IsValid)
            {
                int userId = 0;

                //User
                var authCookieUser = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookieUser != null)
                {
                    var ticket = FormsAuthentication.Decrypt(authCookieUser.Value);
                    int.TryParse(ticket.Name, out userId);
                }

                //Activity
                if (Session["ActivityId"] != null)
                {
                    activityId = (int)Session["ActivityId"];
                }

                if (userId != 0 && activityId != 0)
                {
                    ReviewActivity review = new ReviewActivity
                    {
                        UserId = userId,
                        ActivityId = activityId,
                        Rating = model.Rating,
                        Comment = model.Comment
                    };
                    review.CreatedAt = DateTime.Now;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string queryInsert = "INSERT INTO ReviewsActivities(UserId, ActivityId, Rating, Comment, CreatedAt) VALUES (@UserId, @ActivityId, @Rating, @Comment, @CreatedAt)";

                        using (SqlCommand command = new SqlCommand(queryInsert, connection))
                        {
                            command.Parameters.AddWithValue("@UserId", review.UserId);
                            command.Parameters.AddWithValue("@ActivityId", review.ActivityId);
                            command.Parameters.AddWithValue("@Rating", review.Rating);
                            command.Parameters.AddWithValue("@Comment", review.Comment);
                            command.Parameters.AddWithValue("@CreatedAt", review.CreatedAt);

                            command.ExecuteScalar();
                        }
                    }
                }
            }
            else
            {
                return Content("Failed to submit review");
            }
            return Content("Review submitted");
        }


        public ActionResult ReviewActivityView(int id)
        {
            ActivityReviewViewModel review = new ActivityReviewViewModel();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ActivityName FROM Activities WHERE ActivityId = @Id";
                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            review.ActivityId = id;
                            review.ActivityName = (string)reader["ActivityName"];
                        }
                    }
                }
            }
            return View(review);
        }


        [HttpGet]
        public ActionResult DisplayReview(int activityId)
        {
            List<ReviewActivity> reviewActivities = new List<ReviewActivity>();
            if (Session["ActivityId"] != null)
            {
                activityId = (int)Session["ActivityId"];
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string queryToDisplayReview = "SELECT * FROM ReviewsActivities WHERE ActivityId = @ActivityId";
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
                return View("DisplayReviewActivity", reviewActivities);
            }
            catch (Exception e)
            {
                return View("Error" + e);
            }
        }
    }
}