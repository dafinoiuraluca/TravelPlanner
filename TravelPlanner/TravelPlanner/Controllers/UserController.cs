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
        public ActionResult SignIn()
        {
            return View("SignIn");
        }

        [HttpPost]
        public ActionResult SignUp(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO Users(LastName, FirstName, Email, UserPassword, UserType) VALUES (@LastName, @FirstName, @Email, @Password, @UserType)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    string userType = Request.Form["user-type"];
                    if (string.IsNullOrEmpty(user.UserType))
                    {
                        command.Parameters.AddWithValue("@UserType", "Customer");
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@UserType", user.UserType);
                    }


                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("SignedUp");
        }


        [HttpPost]
        public ActionResult SignIn(string email, string password)
        {
            User user = ValidateUser(email, password);
            if (user == null)
            {
                return Content("Wrong email or password! Please try again.");
            }
            else
            {
                FormsAuthentication.SetAuthCookie(user.UserId.ToString(), false);

                var ticket = new FormsAuthenticationTicket(1, user.UserId.ToString(), DateTime.Now, DateTime.Now.AddMinutes(20), false, user.UserType);
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);

                if (user.UserType == "Customer")
                {
                    return RedirectToAction("AccommodationView", "Accommodation");
                }
                else if (user.UserType == "Manager")
                {
                    return RedirectToAction("NewAccomodation", "Accommodation");
                }
                else if (user.UserType == "Local")
                {
                    return RedirectToAction("NewActivity", "Activity");
                }
                else
                {
                    return RedirectToAction("AccommodationView", "Accommodation");
                }
            }
        }

        private User ValidateUser(string email, string password)
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
                            };
                        }
                    }
                }
            }
            return user;
        }
    }
}