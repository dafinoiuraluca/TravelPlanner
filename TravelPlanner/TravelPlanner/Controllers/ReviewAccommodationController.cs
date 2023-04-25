using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    public class ReviewAccommodationController : Controller
    {
        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: ReviewAccommodation
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LeaveReview(ReviewAccommodation review)
        {
            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.Now;
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string queryToInsertReview = "INSERT INTO ReviewAccommodation(Rating, Comment) VALUES (@Rating, @Comment)";
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