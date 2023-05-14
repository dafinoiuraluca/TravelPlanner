using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;
using System.Security.Claims;
using System.Web.Security;

namespace TravelPlanner.Controllers
{
    public class BookingsController : Controller
    {
        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: Bookings
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BookAccommodation(BookAccommodationViewModel model)
        {
            if (ModelState.IsValid)
            {
                int userId = 0;
                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    int.TryParse(ticket.Name, out userId);
                }

                if(userId != 0)
                {
                    Bookings bookings = new Bookings
                    {
                        UserId = userId,
                        AccommodationId = model.AccommodationId,
                        CheckInDate = model.CheckInDate,
                        CheckOutDate = model.CheckOutDate,
                    };

                    TimeSpan stayDuration = bookings.CheckOutDate.Date.Subtract(bookings.CheckInDate.Date);
                    int numberOfNights = stayDuration.Days;
                    decimal totalPrice = model.Price * numberOfNights;

                    bookings.TotalPrice = totalPrice;
                    bookings.CreatedAt = DateTime.Now;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string queryInsertBooking = "INSERT INTO Bookings (UserId, AccommodationId, CheckInDate, CheckOutDate, TotalPrice, CreatedAt) " +
                                                    "VALUES (@UserId, @AccommodationId, @CheckInDate, @CheckOutDate, @TotalPrice, @CreatedAt)";

                        using (SqlCommand insertCommand = new SqlCommand(queryInsertBooking, conn))
                        {
                            insertCommand.Parameters.AddWithValue("@UserId", bookings.UserId);
                            insertCommand.Parameters.AddWithValue("@AccommodationId", bookings.AccommodationId);
                            insertCommand.Parameters.AddWithValue("@CheckInDate", bookings.CheckInDate);
                            insertCommand.Parameters.AddWithValue("@CheckOutDate", bookings.CheckOutDate);
                            insertCommand.Parameters.AddWithValue("@TotalPrice", bookings.TotalPrice);
                            insertCommand.Parameters.AddWithValue("@CreatedAt", bookings.CreatedAt);

                            insertCommand.ExecuteNonQuery();
                        }
                    }

                    return RedirectToAction("Confirmation", new { accommodationId = bookings.AccommodationId, totalPrice });
                }
                
            }

            return RedirectToAction("Confirmation", new { id = model.AccommodationId });
        }


        //private int GetLoggedInUserId()
        //{
        //    if(User.Identity.IsAuthenticated)
        //    {
        //        var claimsIdentity = User.Identity as ClaimsIdentity;
        //        var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        //        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        //        {
        //            return userId;
        //        }
        //    }
        //    return 0;
        //}

        private Accommodation GetAccommodationById(int accommodationId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Accommodations WHERE AccommodationId = @AccommodationId";
                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@AccommodationId", accommodationId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Accommodation accommodation = new Accommodation
                            {
                                AccommodationId = (int)reader["AccommodationId"],
                                AccomodationName = (string)reader["AccommodationName"],
                                AccommodationType = (string)reader["AccommodationType"],
                                AccommodationLocation = (string)reader["AccommodationLocation"],
                                Price = (decimal)reader["Price"],
                                AccomodationDescription = (string)reader["AccommodationDescription"],
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            };

                            return accommodation;
                        }
                    }
                }
            }

            return null; // Return null if no accommodation is found with the specified ID
        }

        // Method to store the AccommodationId in a cookie
        private void StoreAccommodationId(int accommodationId)
        {
            var ticket = new FormsAuthenticationTicket(1, accommodationId.ToString(), DateTime.Now, DateTime.Now.AddMinutes(20), false, "");
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var authCookie = new HttpCookie("AccommodationIdCookie", encryptedTicket);
            Response.Cookies.Add(authCookie);
        }

        // Method to retrieve the AccommodationId from the cookie
        private int GetAccommodationId()
        {
            var authCookie = Request.Cookies["AccommodationIdCookie"];
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int accommodationId;
                if (int.TryParse(ticket.Name, out accommodationId))
                {
                    return accommodationId;
                }
            }
            return 0;
        }

        [HttpGet]
        public ActionResult StoreAccommodationIdAndRedirect(int accommodationId)
        {
            StoreAccommodationId(accommodationId);
            return RedirectToAction("BookAccommodation");
        }



        // Retrieve the name
        public ActionResult BookAccommodationView(int id)
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
 

        [HttpPost]
        public ActionResult CancelTrip(int bookingId)
        {
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string queryDeleteBooking = "DELETE FROM Bookings WHERE BookingId = @BookingId";

                using(SqlCommand command = new SqlCommand(queryDeleteBooking, conn))
                {
                    command.Parameters.AddWithValue("@BookingId", bookingId);

                    int rowsAffected = command.ExecuteNonQuery();
                    if(rowsAffected > 0)
                    {
                        TempData["Message"] = "Booking was successfuly canceled";
                    }
                    else
                    {
                        TempData["Message"] = "Booking could not be canceled. Please try again!";
                    }
                }
                conn.Close();
            }
            return Redirect("Index");
        }

        //[HttpGet]
        //public ActionResult AccommodationAvailable(int accommodationId, DateTime checkInDate, DateTime checkOutDate)
        //{
        //    bool isAvailable = true;

        //    using(SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        string queryToCheck
        //    }
        //}

        //Create Itinerary

        [HttpPost]
        public ActionResult CreateItinerary(List<int> activityIds, int accommodationId)
        {
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Accommodation WHERE AccommodationId = @AccommodationId";
                using(SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@AccommodationId", accommodationId);
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            Accommodation accommodation = new Accommodation
                            {
                                AccomodationName = (string)reader["AccommodationName"],
                                AccommodationType = (string)reader["AccommodationType"],
                                AccommodationLocation = (string)reader["AccommodationLocation"],
                                Price = (decimal)reader["Price"],
                                AccomodationDescription = (string)reader["AccommodationDescription"],
                                CreatedAt = (DateTime)reader["CreateAt"],
                            };
                        }
                    }
                }
                conn.Close();
            }

            List<Activities> activities = new List<Activities>();
            foreach(int activityId in activityIds)
            {
                using(SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Activities WHERE ActivityId = @ActivityId";
                    using(SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@ActivityId", activityId);
                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                Activities activity = new Activities
                                {
                                    ActivityName = (string)reader["ActivityName"],
                                    ActivityType = (string)reader["ActivityType"],
                                    ActivityDescription = (string)reader["ActivityDescription"],
                                    Price = (decimal)reader["Price"],
                                    CreatedAt = (DateTime)reader["CreateAt"],
                                };
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return View();
        }
    }
}