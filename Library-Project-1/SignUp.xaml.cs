using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Library_Project_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBAccess objDBAccess = new DBAccess();
        DataTable dtUsers = new DataTable();
        public static string PublicEmail;
        public static bool FromSignUp = false;
        public static string UserRank;

        public MainWindow()
        {
            InitializeComponent();
        }

        private string EncryptPassword(string password) //Password encryption
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e) //Go to the login page
        {
            this.Hide();
            Login login = new Login();
            login.Show();
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.HasItems)
            {
                comboBox.SelectedIndex = (comboBox.Items.Count - 1) / 2;    // ComboBox SelectedIndex in Center
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            string account = AccountType.Text;
            string name = txtNameSignUp.Text;
            string email = txtEmailSignUp.Text;
            string password1 = txtPasswordSignUp1.Password;
            string password2 = txtPasswordSignUp2.Password;
            bool noError = true;

            if (name == string.Empty) //Verify the conditions for creating the account
            {
                NameError.Content = "Enter your Name";
                noError = false;
            }

            noError = ContainsOnlyEnglishLetters(name);
            if (ContainsOnlyEnglishLetters(name) == false)
            {
                NameError.Content = "Only English letters are allowed";
            }

            if (email == string.Empty)
            {
                EmailError.Content = "Enter your Email";
                noError = false;
            }
            else if (!IsEmailValid(email))
            {
                EmailError.Content = "Invalid Email format. Only Gmail accounts are allowed.";
                noError = false;
            }
            else
            {
                EmailError.Content = string.Empty;
            }

            if (password1 == string.Empty || password2 == string.Empty)
            {
                PasswordError1.Content = "Enter your Password";
                noError = false;
            }

            if (password1.Length < 8 || password1.Length > 20)
            {
                PasswordError1.Content = "Password must be between 8 and 20 characters.";
                noError = false;
            }

            if (password1 != password2 && password1 != string.Empty)
            {
                PasswordError1.Content = "Those passwords didn’t match. Try again.";
                noError = false;
            }

            if (noError)
            {

                // Check if the email already exists in the database
                SqlCommand checkEmailCommand = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email");
                checkEmailCommand.Parameters.AddWithValue("@Email", email);

                int existingEmailCount = (int)objDBAccess.ExecuteScalar(checkEmailCommand);

                if (existingEmailCount > 0)
                {
                    EmailError.Content = "This email is already registered. Please use a different email.";
                    return;
                }
                else { EmailError.Content = string.Empty; }

                // Proceed with creating the account
                string encryptedPassword = EncryptPassword(password1);

                SqlCommand insertCommand = new SqlCommand("INSERT INTO Users (Name, Email, Password, Rank) VALUES (@Name, @Email, @Password, @Rank)");
                insertCommand.Parameters.AddWithValue("@Name", name); //Record user information in the database
                insertCommand.Parameters.AddWithValue("@Email", email);
                insertCommand.Parameters.AddWithValue("@Password", encryptedPassword);
                insertCommand.Parameters.AddWithValue("@Rank", account);

                int rowsAffected = objDBAccess.executeQuery(insertCommand);

                // Check the result and display appropriate messages
                if (rowsAffected == 1)
                {
                    FromSignUp = true;
                    
                    PublicEmail = email;
                    // Perform any additional actions after successful account creation
                    if (account == "Admin")
                    {
                        this.Hide();
                        Admin admin = new Admin();
                        admin.Show();
                    }
                    else
                    {
                        this.Hide();
                        LibraryPage library = new LibraryPage ();
                        library.Show();
                        
                        if (AccountType.SelectedIndex == 0)
                        {
                            UserRank = "Student";
                        }

                        else if (AccountType.SelectedIndex == 1)
                        {
                            UserRank = "Teacher";
                        }
                    }
                   
                }
                else
                {
                    MessageBox.Show("Error Occurred. Try Again.");
                }


            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e) //Clean all fields
        {
            txtNameSignUp.Text = txtPasswordSignUp2.Password = txtEmailSignUp.Text = txtPasswordSignUp1.Password = string.Empty;
        }

        public static bool ContainsOnlyEnglishLetters(string word) //Only English characters are allowed
        {
            Regex regex = new Regex("^[a-zA-Z]+$");
            return regex.IsMatch(word);
        }

        
        private bool IsEmailValid(string email) //Email verification
        {
            string pattern = @"^[a-zA-Z0-9_.+-]+@gmail.com$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }


    }
}
