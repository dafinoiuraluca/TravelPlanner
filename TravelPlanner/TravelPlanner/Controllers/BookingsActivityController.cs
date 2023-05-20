using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    public class BookingsActivityController : Controller
    {

        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";

        // GET: BookingsActivity
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StoreAccommodationIdAndRedirectActivity(int activityId, string actionName, string controllerName)
        {
            Session["ActivityId"] = activityId;
            return RedirectToAction(actionName, controllerName, new { id = activityId });
        }

        public ActionResult BookActivityView(int id)
        {
            BookActivityViewModel model = new BookActivityViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ActivityName, ActivityDescription, ActivityType, Price FROM Activities WHERE ActivityId = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.ActivityName = (string)reader["ActivityName"];
                            model.ActivityDescription = (string)reader["ActivityDescription"];
                            model.ActivityType = (string)reader["ActivityType"];
                            model.Price = (decimal)reader["Price"];
                        }
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult BookActivity(BookActivityViewModel model, int activityId)
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
                    BookingsActivities bookings = new BookingsActivities
                    {
                        UserId = userId,
                        ActivityId = activityId,
                        StartHour = model.StartHour,
                        StopHour = model.StopHour,
                    };

                    TimeSpan activityDuration = bookings.StopHour - bookings.StartHour;
                    
                    bookings.CreatedAt = DateTime.Now;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string queryInsertBooking = "INSERT INTO BookingsActivities (UserId, ActivityId, StartHour, StopHour, CreatedAt) " +
                                                    "VALUES (@UserId, @ActivityId, @StartHour, @StopHour, @CreatedAt)";

                        using (SqlCommand insertCommand = new SqlCommand(queryInsertBooking, conn))
                        {
                            insertCommand.Parameters.AddWithValue("@UserId", bookings.UserId);
                            insertCommand.Parameters.AddWithValue("@ActivityId", bookings.ActivityId);
                            insertCommand.Parameters.AddWithValue("@StartHour", bookings.StartHour);
                            insertCommand.Parameters.AddWithValue("@StopHour", bookings.StopHour);
                            insertCommand.Parameters.AddWithValue("@CreatedAt", bookings.CreatedAt);

                            insertCommand.ExecuteScalar();
                        }
                    }
                    int bookingId = 0;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string queryGetBookingId = "SELECT TOP 1 BookingsActivitiesId FROM BookingsActivities ORDER BY BookingsActivitiesId DESC";
                        using (SqlCommand command = new SqlCommand(queryGetBookingId, connection))
                        {
                            object result = command.ExecuteScalar();
                            if (result != null)
                            {
                                bookingId = (int)result;
                            }
                        }

                        string queryGetBooking = "SELECT BookingsActivitiesId, UserId, ActivityId, StartHour, StopHour, CreatedAt " +
                                                 "FROM BookingsActivities " +
                                                 "WHERE BookingsActivitiesId = @BookingsActivitiesId";

                        using (SqlCommand getBookingCommand = new SqlCommand(queryGetBooking, connection))
                        {
                            getBookingCommand.Parameters.AddWithValue("@BookingsActivitiesId", bookingId);

                            using (SqlDataReader reader = getBookingCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    bookings = new BookingsActivities
                                    {
                                        BookingsActivitiesId = (int)reader["BookingsActivitiesId"],
                                        UserId = (int)reader["UserId"],
                                        ActivityId = (int)reader["ActivityId"],
                                        StartHour = (TimeSpan)reader["StartHour"],
                                        StopHour = (TimeSpan)reader["StopHour"],
                                        CreatedAt = (DateTime)reader["CreatedAt"]
                                    };

                                    return RedirectToAction("ConfirmationActivity", new { bookingId = bookings.BookingsActivitiesId });
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("ConfirmationActivity", new { id = model.ActivityId });
        }

        public ActionResult ConfirmationActivity(int bookingId)
        {
            BookingsActivities booking;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT BookingsActivitiesId, UserId, ActivityId, StartHour, StopHour, CreatedAt FROM BookingsActivities WHERE BookingsActivitiesId = @BookingsActivitiesId";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@BookingsActivitiesId", bookingId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            booking = new BookingsActivities
                            {
                                BookingsActivitiesId = (int)reader["BookingsActivitiesId"],
                                UserId = (int)reader["UserId"],
                                ActivityId = (int)reader["ActivityId"],
                                StartHour = (TimeSpan)reader["StartHour"],
                                StopHour = (TimeSpan)reader["StopHour"],
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            };
                        }
                        else
                        {
                            return RedirectToAction("BookingNotFound");
                        }
                    }
                }
            }

            return View(booking);
        }
    }
}