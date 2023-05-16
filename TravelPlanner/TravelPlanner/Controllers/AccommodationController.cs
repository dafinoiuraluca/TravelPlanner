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

                string insertQuery = "INSERT INTO Accommodations (AccommodationName, AccommodationType, AccommodationLocation, AccommodationDescription, Price, CreatedAt) " +
                                     "VALUES (@AccommodationName, @AccommodationType, @AccommodationLocation, @AccommodationDescription, @Price, @CreatedAt)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccommodationName", accommodations.AccomodationName);
                    command.Parameters.AddWithValue("@AccommodationType", accommodations.AccommodationType);
                    command.Parameters.AddWithValue("@AccommodationLocation", accommodations.AccommodationLocation);
                    command.Parameters.AddWithValue("@AccommodationDescription", accommodations.AccomodationDescription);
                    command.Parameters.AddWithValue("@Price", accommodations.Price);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

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
                conn.Open();
                string queryToSearch = "SELECT * FROM Accommodations WHERE AccommodationLocation LIKE @searchAccommodation";
                using (SqlCommand command = new SqlCommand(queryToSearch, conn))
                {
                    command.Parameters.AddWithValue("@searchAccommodation", "%" + searchAccommodation + "%");
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
                        };
                        accommodations.Add(accommodation);
                    }
                }
            }
            return View("AccommodationView", accommodations);
        }


        // View all accommodations

        [HttpGet]
        public ActionResult AccommodationView()
        {
            List<Accommodation> accommodations = GetAllAccommodations();
            return View(accommodations);
        }


        public List<Accommodation> GetAllAccommodations()
        {
            List<Accommodation> accommodations = new List<Accommodation>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM Accommodations";

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Accommodation accommodation = new Accommodation();

                        accommodation.AccommodationId = (int)reader["AccommodationId"];
                        accommodation.AccomodationName = (string)reader["AccommodationName"];
                        accommodation.AccommodationType = (string)reader["AccommodationType"];
                        accommodation.AccommodationLocation = (string)reader["AccommodationLocation"];
                        accommodation.AccomodationDescription = (string)reader["AccommodationDescription"];
                        accommodation.Price = (decimal)reader["Price"];
                      
                        accommodations.Add(accommodation);
                    }
                }
            }

            return accommodations;
        }
    }
}