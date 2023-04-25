using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelPlanner.Models;

namespace TravelPlanner.Controllers
{
    public class UserController : Controller
    {
        string connectionString = "Data Source=DESKTOP-LT7G6FF\\SQLEXPRESS;Initial Catalog=TravelPlanner;Integrated Security=True";

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }
        public ActionResult SignedUp()
        {
            return View("SignedUp");
        }

        [HttpPost]
        public ActionResult CreateUser(User user)
        {
            // Call the SignUp method here
            SignUp(user);

            return RedirectToAction("SignedUp");
        }


        public void SignUp(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO Users (LastName, FirstName, Email, UserPassword, UserType) VALUES (@LastName, @FirstName, @Email, @Password, @UserType)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@UserType", user.UserType);
                    //command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                    command.ExecuteNonQuery();
                }
            }
        }


        [HttpPost]
        public ActionResult CheckCredentials(string email, string password)
        {
            // Call the SignUp method here

            if (SignIn(email, password) == null)

                return Content("Wrong email or password! Try again");
            else
            {
                if (SignIn(email, password).UserType == "admin")
                    return RedirectToAction("NewAccomodation", "Accomodations");
                else if (SignIn(email, password).UserType == "tourguide")
                    return RedirectToAction("NewActivity", "Activities");
                else return Content("signed in as user");
            }
        }
        [HttpGet]
        public User SignIn(string email, string password)
        {
            User user = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM Users WHERE Email = @Email AND UserPassword = @Password";

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserId = (int)reader["UserId"],
                                LastName = (string)reader["LastName"],
                                FirstName = (string)reader["FirstName"],
                                Email = (string)reader["Email"],
                                Password = (string)reader["UserPassword"],
                                UserType = (string)reader["UserType"],
                                //CreatedAt = (DateTime)reader["CreatedAt"],
                            };
                        }
                    }
                }
            }

            return user;
        }
    }
}