﻿using System;
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

        public ActionResult StoreAccommodationIdAndRedirect(int accommodationId, string actionName, string controllerName)
        {
            Session["AccommodationId"] = accommodationId;
            return RedirectToAction(actionName, controllerName);
        }


        [HttpPost]
        public ActionResult BookAccommodation(BookAccommodationViewModel model, int accommodationId)
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
                    Bookings bookings = new Bookings
                    {
                        UserId = userId,
                        AccommodationId = accommodationId,
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

                            insertCommand.ExecuteScalar();
                        }
                    }
                    int bookingId = 0;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string queryGetBookingId = "SELECT TOP 1 BookingId FROM Bookings ORDER BY BookingId DESC";
                        using (SqlCommand command = new SqlCommand(queryGetBookingId, connection))
                        {
                            object result = command.ExecuteScalar();
                            if (result != null)
                            {
                                bookingId = (int)result;
                            }
                        }

                        string queryGetBooking = "SELECT BookingId, UserId, AccommodationId, CheckInDate, CheckOutDate, TotalPrice, CreatedAt " +
                                                 "FROM Bookings " +
                                                 "WHERE BookingId = @BookingId";

                        using (SqlCommand getBookingCommand = new SqlCommand(queryGetBooking, connection))
                        {
                            getBookingCommand.Parameters.AddWithValue("@BookingId", bookingId);

                            using (SqlDataReader reader = getBookingCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    bookings = new Bookings
                                    {
                                        BookingId = (int)reader["BookingId"],
                                        UserId = (int)reader["UserId"],
                                        AccommodationId = (int)reader["AccommodationId"],
                                        CheckInDate = (DateTime)reader["CheckInDate"],
                                        CheckOutDate = (DateTime)reader["CheckOutDate"],
                                        TotalPrice = (decimal)reader["TotalPrice"],
                                        CreatedAt = (DateTime)reader["CreatedAt"]
                                    };

                                    return RedirectToAction("Confirmation", new { bookingId = bookings.BookingId });
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Confirmation", new { id = model.AccommodationId });
        }
            

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

        public ActionResult Confirmation(int bookingId)
        {
            Bookings booking;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT BookingId, UserId, AccommodationId, CheckInDate, CheckOutDate, TotalPrice, CreatedAt FROM Bookings WHERE BookingId = @BookingId";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@BookingId", bookingId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            booking = new Bookings
                            {
                                BookingId = (int)reader["BookingId"],
                                UserId = (int)reader["UserId"],
                                AccommodationId = (int)reader["AccommodationId"],
                                CheckInDate = (DateTime)reader["CheckInDate"],
                                CheckOutDate = (DateTime)reader["CheckOutDate"],
                                TotalPrice = (decimal)reader["TotalPrice"],
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            };
                        }
                        else
                        {
                            // Handle the case when the booking is not found
                            return RedirectToAction("BookingNotFound");
                        }
                    }
                }
            }

            return View(booking);
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

            return null;
        }
    }
}