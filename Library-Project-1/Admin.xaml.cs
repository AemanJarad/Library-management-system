using Faker;
using Faker.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Library_Project_1
{
    public partial class Admin : Window
    {
        // Declare instance variables
        DBAccess objDBAccess = new DBAccess();
        DataTable dtUsersBook = new DataTable();
        DataTable dtUsers = new DataTable();
        DataTable dtSettings = new DataTable();
        DataTable dtReturn = new DataTable();

        public Admin()
        {
            InitializeComponent();
        }

        // Event handler for the window's loaded event
        private void AdminPage_Load(object sender, RoutedEventArgs e)
        {
            // Load data from the "Books" table into dtUsersBook DataTable
            string queryBook = "Select * from Books";
            objDBAccess.readDatathroughAdapter(queryBook, dtUsersBook);
            BookData.ItemsSource = dtUsersBook.DefaultView;
            objDBAccess.closeConn();

            // Load data from the "settings" table into dtSettings DataTable
            string querySettings = "Select * from settings";
            objDBAccess.readDatathroughAdapter(querySettings, dtSettings);
            SettingsData.ItemsSource = dtSettings.DefaultView;
            objDBAccess.closeConn();

            string queryReturn = "Select * from BookApproval";
            objDBAccess.readDatathroughAdapter(queryReturn, dtReturn);
            ReturnData.ItemsSource = dtReturn.DefaultView;
            objDBAccess.closeConn();

        }

        // Event handler for the "Save Book" button click
        private void SaveBook_Click(object sender, RoutedEventArgs e)
        {
            // Save changes made to the dtUsersBook DataTable back to the "Books" table
            string query = "Select * from Books";
            objDBAccess.executeDataAdapter(dtUsersBook, query);
        }

        // Event handler for the "Save Users" button click
        private void SaveUsers_Click(object sender, RoutedEventArgs e)
        {
            // Save changes made to the dtUsers DataTable back to the "Users" table
            string query = "Select * from Users";
            objDBAccess.executeDataAdapter(dtUsers, query);
        }

        // Event handler for the selection changed event of the Rank ComboBox
        private void SelectedRank(object sender, SelectionChangedEventArgs e)
        {
            // Filter the user data based on the selected role (Student/Teacher)
            string selectedRole = ((ComboBoxItem)cmbSelectRank.SelectedItem).Content.ToString();
            string query = "Select * from Users";
             if (selectedRole == "Student")
            {
                query += " where Rank = 'Student'";
            }
            else if (selectedRole == "Teacher")
            {
                query += " where Rank = 'Teacher'";
            }

            dtUsers.Clear();
            objDBAccess.readDatathroughAdapter(query, dtUsers);
            UsersData.ItemsSource = dtUsers.DefaultView;
            objDBAccess.closeConn();
        }

        // Event handler for the "Save Settings" button click
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Save changes made to the dtSettings DataTable back to the "settings" table
            string query = "Select * from settings";
            objDBAccess.executeDataAdapter(dtSettings, query);
        }

        // Event handler for the "Log Out" button click
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Login login = new Login();
            login.Show();
        }

        // Event handler for the "Accept Return" button click
        private void AcceptReturn_Click(object sender, RoutedEventArgs e)
        {
            if (txtReturn.Text != string.Empty && int.TryParse(txtReturn.Text, out int id))
            {
                int bookID = Convert.ToInt32(txtReturn.Text);

                // Update the "Available" count for the book in the "Books" table
                string updateQuery = "UPDATE Books SET Available = Available + 1 WHERE BookID = @bookID";
                SqlCommand updateCommand = new SqlCommand(updateQuery);
                updateCommand.Parameters.AddWithValue("@bookID", bookID);

                int rowsAffected = objDBAccess.executeQuery(updateCommand);
                objDBAccess.closeConn();

                if (rowsAffected == 1)
                {
                    // Remove the book entry from the "BookApproval" table
                    string deleteQuery = "DELETE FROM BookApproval WHERE book_id = @bookID";
                    SqlCommand deleteCommand = new SqlCommand(deleteQuery);
                    deleteCommand.Parameters.AddWithValue("@bookID", bookID);

                    int deleteRowsAffected = objDBAccess.executeQuery(deleteCommand);
                    objDBAccess.closeConn();

                    if (deleteRowsAffected == 1)
                    {
                        lblReturnInfo.Content = "The book has been returned and removed from BookApproval successfully.";
                        lblReturnInfo.Foreground = Brushes.Green;
                    }
                    else
                    {
                        lblReturnInfo.Content = "The book has been returned successfully, but failed to remove from BookApproval.";
                        lblReturnInfo.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    lblReturnInfo.Content = "Failed to return the book.";
                    lblReturnInfo.Foreground = Brushes.Red;
                }

                dtReturn.Clear();
                string queryReturn = "SELECT * FROM BookApproval";
                objDBAccess.readDatathroughAdapter(queryReturn, dtReturn);
                ReturnData.ItemsSource = dtReturn.DefaultView;
                objDBAccess.closeConn();
            }
        }

        private void txtReturn_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
