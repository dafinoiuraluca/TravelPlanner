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
        string connectionString = "Data Source=DESKTOP-6A1HP7T;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: Bookings
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BookAccommodation(Bookings bookings)
        {
            if(ModelState.IsValid)
            {
                bookings.CreatedAt = DateTime.Now;

                using(SqlConnection conn = new SqlConnection(connectionString)) 
                {
                    conn.Open();
                    string queryCheckAvailabilty = "SELECT COUNT(*) FROM Bookings WHERE AccommodationId = @AccommodationId AND (CheckInDate <= @ CheckInDate AND CheckOutDate > @CheckOutDate)";

                    using(SqlCommand command = new SqlCommand(queryCheckAvailabilty, conn))
                    {
                        command.Parameters.AddWithValue("@AccommodationId", bookings.AccommodationId);
                        command.Parameters.AddWithValue("@CheckInDate", bookings.CheckInDate);
                        command.Parameters.AddWithValue("@CheckOutDate", bookings.CheckOutDate);
                    }

                    string queryGetPricePerNight = "SELECT Price FROM Accommodation Where AccommodationId = @AccommodationId";
                    using(SqlCommand cmd = new SqlCommand(queryGetPricePerNight, conn))
                    {
                        cmd.Parameters.AddWithValue("@AccommodationId", bookings.AccommodationId);
                        decimal price = (decimal)cmd.ExecuteScalar();
                    }
                }
            }
            return View();
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