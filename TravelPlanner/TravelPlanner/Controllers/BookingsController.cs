using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;

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
                // Map the selected accommodation details to the Bookings model
                Bookings bookings = new Bookings
                {
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

                    string queryInsertBooking = "INSERT INTO Bookings (AccommodationId, CheckInDate, CheckOutDate, TotalPrice, CreatedAt) " +
                                                "VALUES (@AccommodationId, @CheckInDate, @CheckOutDate, @TotalPrice, @CreatedAt)";

                    using (SqlCommand insertCommand = new SqlCommand(queryInsertBooking, conn))
                    {
                        insertCommand.Parameters.AddWithValue("@AccommodationId", bookings.AccommodationId);
                        insertCommand.Parameters.AddWithValue("@CheckInDate", bookings.CheckInDate);
                        insertCommand.Parameters.AddWithValue("@CheckOutDate", bookings.CheckOutDate);
                        insertCommand.Parameters.AddWithValue("@TotalPrice", bookings.TotalPrice);
                        insertCommand.Parameters.AddWithValue("@CreatedAt", bookings.CreatedAt);

                        insertCommand.ExecuteNonQuery();
                    }
                }

                // Redirect to the confirmation page with the accommodation details and total price
                return RedirectToAction("Confirmation", new { accommodationId = bookings.AccommodationId, totalPrice });
            }

            return RedirectToAction("Confirmation", new { id = model.AccommodationId });
        }




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
                            model.AccommodationName = reader.GetString(0);
                            model.AccommodationDescription = reader.GetString(1);
                            model.AccommodationType = reader.GetString(2);
                            model.AccommodationLocation = reader.GetString(3);
                            model.Price = reader.GetDecimal(4);
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