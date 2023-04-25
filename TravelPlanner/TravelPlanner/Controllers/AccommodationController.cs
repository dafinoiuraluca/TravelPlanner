using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    public class AccommodationController : Controller
    {
        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";
        // GET: Accomodations
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NewAccomodation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccomodation(Accommodation accommodations)
        {
            CreateNewAccommodation(accommodations);

            return Content("Added accomodation"); ;
        }

        public void CreateNewAccommodation(Accommodation accommodations)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO Accommodations (AccommodationName, AccommodationType,AccommodationLocation, AccommodationDescription, Price, CreatedAt, UpdatedAt) " +
                                     "VALUES (@AccommodationName, @AccommodationType,@AccommodationLocation, @AccommodationDescription, @Price, @CreatedAt, @UpdatedAt)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccommodationName", accommodations.AccomodationName);
                    command.Parameters.AddWithValue("@AccommodationType", accommodations.AccommodationType);
                    command.Parameters.AddWithValue("@AccommodationLocation", accommodations.AccommodationLocation);
                    command.Parameters.AddWithValue("@AccommodationDescription", accommodations.AccomodationDescription);
                    command.Parameters.AddWithValue("@Price", accommodations.Price);
                    command.Parameters.AddWithValue("@CreatedAt", accommodations.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", accommodations.UpdatedAt);

                    command.ExecuteNonQuery();
                }
            }
        }

        // GET: Accommodation
        public ActionResult SearchAccommodation(string searchAccommodation)
        {
            List<Accommodation> accommodations = new List<Accommodation>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string queryToSearch = "SELECT * FROM Accommodation WHERE AccommodationName LIKE @searchAccommodation";
                using (SqlCommand command = new SqlCommand(queryToSearch, conn))
                {
                    command.Parameters.AddWithValue("@searchAccommodation", "%" + searchAccommodation + "%");
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Accommodation accommodation = new Accommodation
                        {
                            AccommodationId = (int)reader["AccommodationId"],
                            AccomodationName = reader["AccommodationName"].ToString(),
                            AccommodationType = reader["AccommodationType"].ToString(),
                            AccommodationLocation = reader["AccommodationLocation"].ToString(),
                            Price = (decimal)reader["Price"],
                            AccomodationDescription = reader["AccommodationDescription"].ToString(),
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            UpdatedAt = (DateTime)reader["UpdatedAt"]
                        };
                        accommodations.Add(accommodation);
                    }
                }
            }
            return View("ViewAccommodations", accommodations);
        }
    }
}