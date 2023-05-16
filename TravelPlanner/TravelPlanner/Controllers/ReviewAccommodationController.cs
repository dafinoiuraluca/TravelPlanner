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
    public class ReviewAccommodationController : Controller
    {
        string connectionString = "Data Source=DESKTOP-6A1HP7T;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: ReviewAccommodation
        public ActionResult Index()
        {
            return View();
        }

        // add review

        public ActionResult StoreAccommodationIdAndRedirectReview(int accommodationId, string actionName, string controllerName)
        {
            Session["AccommodationId"] = accommodationId;
            return RedirectToAction(actionName, controllerName);
        }


        [HttpPost]
        public ActionResult LeaveReview(AccommodationReviewViewModel model, int accommodationId)
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

                //Accommodation
                if (Session["AccommodationId"] != null)
                {
                    accommodationId = (int)Session["AccommodationId"];
                }

                if (userId != 0 && accommodationId != 0)
                {
                    ReviewAccommodation review = new ReviewAccommodation
                    {
                        UserId = userId,
                        AccommodationId = accommodationId,
                        Rating = model.Rating,
                        Comment = model.Comment
                    };
                    review.CreatedAt = DateTime.Now;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string queryInsert = "INSERT INTO ReviewsAccommodations(UserId, AccommodationId, Rating, Comment, CreatedAt) VALUES (@UserId, @AccommodationId, @Rating, @Comment, @CreatedAt)";

                        using (SqlCommand command = new SqlCommand(queryInsert, connection))
                        {
                            command.Parameters.AddWithValue("@UserId", review.UserId);
                            command.Parameters.AddWithValue("@AccommodationId", review.AccommodationId);
                            command.Parameters.AddWithValue("@Rating", review.Rating);
                            command.Parameters.AddWithValue("@Comment", review.Comment);
                            command.Parameters.AddWithValue("@CreatedAt", review.CreatedAt);

                            command.ExecuteScalar();
                        }
                    }
                }
            } else
            {
                return Content("Failed to submit review");
            }
            return Content("Review submitted");
        }

        public ActionResult ReviewAccommodationView(int id)
        {
            AccommodationReviewViewModel review = new AccommodationReviewViewModel();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT AccommodationName FROM Accommodations WHERE AccommodationId = @Id";
                using(SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            review.AccommodationId = id;
                            review.AccommodationName = (string)reader["AccommodationName"];
                        }
                    }
                }
            }
            return View(review);
        }

        [HttpGet]
        public ActionResult DisplayReview(int accommodationId)
        {
            List<ReviewAccommodation> reviewAccommodations = new List<ReviewAccommodation>();
            if (Session["AccommodationId"] != null)
            {
                accommodationId = (int)Session["AccommodationId"];
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string queryToDisplayReview = "SELECT * FROM ReviewsAccommodations WHERE AccommodationId = @AccommodationId";
                    using (SqlCommand command = new SqlCommand(queryToDisplayReview, conn))
                    {
                        command.Parameters.AddWithValue("@AccommodationId", accommodationId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReviewAccommodation review = new ReviewAccommodation
                                {
                                    Rating = (int)reader["Rating"],
                                    Comment = reader["Comment"].ToString(),
                                    CreatedAt = (DateTime)reader["CreatedAt"]
                                };
                                reviewAccommodations.Add(review);
                            }
                        }
                    }
                }
                return View("DisplayReviewAccommodation", reviewAccommodations);
            }
            catch (Exception e)
            {
                return View("Error" + e);
            }
        }
    }
}