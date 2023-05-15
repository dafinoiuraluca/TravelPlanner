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
        public ActionResult LeaveReview(ReviewAccommodation model, int accommodationId)
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
                        Comment = model.Comment,
                        CreatedAt = DateTime.Now
                    };

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string queryInsertReview = "INSERT INTO ReviewsAccommodation (UserId, AccommodationId, Rating, Comment, CreatedAt) " +
                                                    "VALUES (@UserId, @AccommodationId, @Rating, @Comment, @CreatedAt)";

                        using (SqlCommand insertCommand = new SqlCommand(queryInsertReview, conn))
                        {
                            insertCommand.Parameters.AddWithValue("@UserId", review.UserId);
                            insertCommand.Parameters.AddWithValue("@AccommodationId", review.AccommodationId);
                            insertCommand.Parameters.AddWithValue("@Rating", review.Rating);
                            insertCommand.Parameters.AddWithValue("@Comment", review.Comment);
                            insertCommand.Parameters.AddWithValue("@CreatedAt", review.CreatedAt);

                            insertCommand.ExecuteScalar();
                        }
                    }
                    return RedirectToAction("Confirmation", new { id = accommodationId });
                }
            }
            return RedirectToAction("Confirmation", new { id = model.AccommodationId });
        }

        public ActionResult LeaveReviewView(int id)
        {
            BookAccommodationViewModel model = new BookAccommodationViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT AccommodationName, AccommodationDescription, AccommodationType, AccommodationLocation, Price FROM Accommodations WHERE AccommodationId = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.AccommodationName = (string)reader["AccommodationName"];
                            model.AccommodationDescription = (string)reader["AccommodationDescription"];
                            model.AccommodationType = (string)reader["AccommodationType"];
                            model.AccommodationLocation = (string)reader["AccommodationLocation"];
                            model.Price = (decimal)reader["Price"];
                        }
                    }
                }
            }


            return View(model);
        }




        [HttpGet]
        public ActionResult DisplayReview(int accommodationId)
        {
            List<ReviewAccommodation> reviewAccommodations = new List<ReviewAccommodation>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string queryToDisplayReview = "SELECT * FROM ReviewAccommodation WHERE AccommodationId = @AccommodationId";
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
                return View(reviewAccommodations);
            }
            catch (Exception e)
            {
                return View("Error" + e);
            }
        }
    }
}