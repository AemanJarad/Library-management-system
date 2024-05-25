using Faker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Library_Project_1
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        DBAccess objDBAccess = new DBAccess();
        DataTable dtUsers = new DataTable();
        public static string PublicEmail;
        public static bool FromLogin = false;
        public static string UserRank;
        public Login()
        {
            InitializeComponent();
        }

        //Go to the account creation page
        private void GOToSignUp_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainWindow signUp = new MainWindow();
            signUp.Show();
        }
        // Login button
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string userEmail = txtEmailName.Text;
            string userPassword = txtPassword.Password;

            if (string.IsNullOrEmpty(userEmail))
            {
                EmailError.Content = "Please enter your email or name...";
            }
            else if (string.IsNullOrEmpty(userPassword))
            {
                PasswordError.Content = "Please enter password...";
            }
            else
            {
                //Check ID or email
                string query = "";
                if (int.TryParse(userEmail, out int userId))
                {
                    query = "SELECT * FROM Users WHERE ID = '" + userId + "' ";
                }
                else
                {
                    query = "SELECT * FROM Users WHERE Email = '" + userEmail + "' ";
                }

                objDBAccess.readDatathroughAdapter(query, dtUsers);


                if (dtUsers.Rows.Count == 1)
                {
                    string storedPassword = dtUsers.Rows[0]["Password"].ToString();
                    string decryptedPassword = DecryptPassword(userPassword);
                    

                    if (decryptedPassword == storedPassword) //Verify user information
                    {
                        
                        objDBAccess.closeConn();
                        FromLogin = true;
                        PublicEmail = userEmail;
                        //Close the login page and open the other page according to the rank
                        if ("Student" == dtUsers.Rows[0]["Rank"].ToString())
                        {
                            this.Hide();
                            LibraryPage library = new LibraryPage();
                            library.Show();
                            UserRank = dtUsers.Rows[0]["Rank"].ToString();
                            dtUsers.Clear();
                        }
                        else if ("Teacher" == dtUsers.Rows[0]["Rank"].ToString())
                        {
                            this.Hide();
                            LibraryPage library = new LibraryPage();
                            library.Show();
                            UserRank = dtUsers.Rows[0]["Rank"].ToString();
                            dtUsers.Clear();
                        }
                        else if ("Admin" == dtUsers.Rows[0]["Rank"].ToString())
                        {
                            this.Hide();
                            Admin admin = new Admin();
                            admin.Show();
                            dtUsers.Clear();
                        }
                    }
                    else
                    {
                        //Error messages
                        PasswordError.Content = "Invalid email/ID or password. Please try again.";
                        dtUsers.Clear();    
                    }
                }
                else
                {
                    PasswordError.Content = "Invalid email/ID or password. Please try again.";
                    dtUsers.Clear();
                }
            }
        }

        private string DecryptPassword(string encryptedPassword) //Encrypt the password to compare it with the database
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(encryptedPassword));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}


//123456

//sdasdadad145
