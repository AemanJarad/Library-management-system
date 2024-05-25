using Faker;
using Faker.Resources;
using Microsoft.VisualBasic;
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
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class LibraryPage : Window
    {

        DBAccess objDBAccess = new DBAccess();
        DataTable dtUsers = new DataTable();
        DataTable dtBooks = new DataTable();
        DataTable availabilityTable = new DataTable();
        DataTable dtMyBook = new DataTable();
        DataTable dtTitle = new DataTable();
        DataTable dtBorrow = new DataTable();
        DBAccess dbAccess = new DBAccess();

        DataTable studentDays = new DataTable();
        DataTable teacherDays = new DataTable();

        DataTable studentBooks = new DataTable();
        DataTable teacherBooks = new DataTable();

        DataTable dtCount = new DataTable();
        public static int bookID;
        public LibraryPage()
        {
            InitializeComponent();
        }

        private void CheckLoanExpiration() //View notices of books whose loan period has expired
        {
            lblNotifications.Content = string.Empty;
            dtMyBook.Clear();
            string queryMyBook = "SELECT * FROM BookLoans WHERE user_id = '" + txtID.Text + "' ";
            objDBAccess.readDatathroughAdapter(queryMyBook, dtMyBook);

            MyBookData.ItemsSource = dtMyBook.DefaultView;
            objDBAccess.closeConn();
            foreach (DataRow row in dtMyBook.Rows)
            {
                DateTime dueDate = Convert.ToDateTime(row["due_date"]);
                if (dueDate.Date < DateTime.Today)
                {
                    string bookTitle = row["Title"].ToString();
                    lblNotifications.Content += "The loan for the book '" + bookTitle + "' has expired. Please return the book.\n";
                    
                }
            }
        }

        public (int, int) Rank() //Determine the user's rank and powers
        {
            studentDays.Clear();
            teacherDays.Clear();
            studentBooks.Clear();
            teacherBooks.Clear();
            if (Login.UserRank == "Student" || MainWindow.UserRank == "Student")
            {
                int maxDaysForStudent;
                int maxBooksForStudent;

                string studentDaysQuery = "SELECT setting_value FROM settings WHERE setting_name = 'Maximum Days to Borrow Books (Students)'";
                string studentBooksQuery = "SELECT setting_value FROM settings WHERE setting_name = 'Maximum Number of Books for student '";
                
                dbAccess.readDatathroughAdapter(studentDaysQuery, studentDays);
                dbAccess.readDatathroughAdapter(studentBooksQuery, studentBooks);
                maxDaysForStudent = Convert.ToInt32(studentDays.Rows[0]["setting_value"]);
                maxBooksForStudent = Convert.ToInt32(studentBooks.Rows[0]["setting_value"]);
                objDBAccess.closeConn();

                return (maxDaysForStudent, maxBooksForStudent);
            }
            else if (Login.UserRank == "Teacher" || MainWindow.UserRank == "Teacher")
            {
                dtUsers.Clear();
                string IsTeacherQuery = "SELECT IsTeacher FROM Users WHERE ID = '" + txtID.Text + "'  ";
                objDBAccess.readDatathroughAdapter(IsTeacherQuery, dtUsers);
                int IsTeacher = Convert.ToInt32(dtUsers.Rows[0]["IsTeacher"]);

                if (IsTeacher == 0) //If the professor is not approved, he will obtain the powers of the student
                {
                    int maxDaysForStudent;
                    int maxBooksForStudent;

                    string studentDaysQuery = "SELECT setting_value FROM settings WHERE setting_name = 'Maximum Days to Borrow Books (Students)'";
                    string studentBooksQuery = "SELECT setting_value FROM settings WHERE setting_name = 'Maximum Number of Books for student '";
                    dbAccess.readDatathroughAdapter(studentDaysQuery, studentDays);
                    dbAccess.readDatathroughAdapter(studentBooksQuery, studentBooks);
                    maxDaysForStudent = Convert.ToInt32(studentDays.Rows[0]["setting_value"]);
                    maxBooksForStudent = Convert.ToInt32(studentBooks.Rows[0]["setting_value"]);
                    objDBAccess.closeConn();

                    return (maxDaysForStudent, maxBooksForStudent);
                }
                else if (IsTeacher == 1)
                {
                    int maxDaysForTeacher;
                    int maxBooksForTeacher;

                    string teacherDaysQuery = "SELECT setting_value FROM settings WHERE setting_name = 'Maximum Days to Borrow Books (Teachers)'";
                    string teacherBooksQuery = "SELECT setting_value FROM settings WHERE setting_name = 'Maximum Number of Books for Teachers'";

                    dbAccess.readDatathroughAdapter(teacherDaysQuery, teacherDays);
                    dbAccess.readDatathroughAdapter(teacherBooksQuery, teacherBooks);

                    maxDaysForTeacher = Convert.ToInt32(teacherDays.Rows[0]["setting_value"]);
                    maxBooksForTeacher = Convert.ToInt32(teacherBooks.Rows[0]["setting_value"]);
                    objDBAccess.closeConn();

                    return (maxDaysForTeacher, maxBooksForTeacher);
                }
            }

            return (0, 0);
        }

        private void LibraryPage_Load(object sender, RoutedEventArgs e)//Show schedules and notifications when the page loads 
        {
            string id, name, email,queryMyBook;
            string queryBook = "Select * from Books where Available > 0 ";
            objDBAccess.readDatathroughAdapter(queryBook, dtBooks);
            BookData.ItemsSource = dtBooks.DefaultView;
            objDBAccess.closeConn();
            // 
            if (MainWindow.FromSignUp == true)
            {
                //string queryUsers = "Select * from Users where Email = '" + MainWindow.PublicEmail + "' ";
                // objDBAccess.readDatathroughAdapter(queryUsers, dtUsers);

                string queryUsers = "";
                if (int.TryParse(MainWindow.PublicEmail, out int userId))
                {
                    queryUsers = "SELECT * FROM Users WHERE ID = '" + userId + "' ";
                }
                else
                {
                    queryUsers = "SELECT * FROM Users WHERE Email = '" + MainWindow.PublicEmail + "' ";
                }
                objDBAccess.readDatathroughAdapter(queryUsers, dtUsers);

            }
            else if (Login.FromLogin == true)
            {

                string queryUsers = "";
                if (int.TryParse(Login.PublicEmail, out int userId))
                {
                    queryUsers = "SELECT * FROM Users WHERE ID = '" + userId + "' ";
                }
                else
                {
                    queryUsers = "SELECT * FROM Users WHERE Email = '" + Login.PublicEmail + "' ";
                }
                objDBAccess.readDatathroughAdapter(queryUsers, dtUsers);
            }

            if (dtUsers.Rows.Count == 1)
            {
                
                id = dtUsers.Rows[0]["ID"].ToString();
                name = dtUsers.Rows[0]["Name"].ToString();
                email = dtUsers.Rows[0]["Email"].ToString();
                objDBAccess.closeConn();
                lblWelcomeName.Content = "Welcome " + name;
                txtNameHome.Text = name;
                txtEmailHome.Text = email;
                txtID.Text = id;
                queryMyBook = "SELECT * FROM BookLoans WHERE user_id = '" + txtID.Text + "' ";
                objDBAccess.readDatathroughAdapter(queryMyBook, dtMyBook);

                CheckLoanExpiration();

            }
            dtMyBook.Clear();
            queryMyBook = "SELECT * FROM BookLoans WHERE user_id = '" + txtID.Text + "' ";
            objDBAccess.readDatathroughAdapter(queryMyBook, dtMyBook);
            
            MyBookData.ItemsSource = dtMyBook.DefaultView;
            objDBAccess.closeConn();
             
        }


        private bool IsBookAlreadyBorrowed(int bookID) //Prevent taking the book twice
        {
            dtCount.Clear();
            string query = "SELECT * FROM BookLoans WHERE book_id = '" + bookID + "' AND user_id = '"+ txtID.Text +"' ";
            objDBAccess.readDatathroughAdapter(query, dtCount);
            objDBAccess.closeConn();
            if (dtCount.Rows.Count > 0)
            {
                return true;
            }
            else { return false; }
        }

        private void AddBook_Click(object sender, RoutedEventArgs e) //Borrow the book
        {
            var result = Rank(); //Determine the rank and its powers
            int MaxDay = result.Item1; // MaxDay
            int MaxBooks = result.Item2; // MaxBooks

            if (txtBookID.Text != string.Empty)
            {
                int bookID = Convert.ToInt32(txtBookID.Text);
                int userID = Convert.ToInt32(txtID.Text);
                dtTitle.Clear();
                string borrowedBooksQuery = "SELECT COUNT(*) FROM BookLoans WHERE user_id = '" + userID + "'";
                int borrowedBooksCount = Convert.ToInt32(objDBAccess.executeScalar(borrowedBooksQuery));

                objDBAccess.closeConn();

                if (MaxDay > 0)
                {
                    
                    if (txtBookID.Text != string.Empty)
                    {
                        if (borrowedBooksCount < MaxBooks) //The student did not exceed the maximum number of borrowings
                        {
                            if (IsBookAlreadyBorrowed(bookID)) //Prevent taking the same book twice at the same time
                            {
                                BorrowingInformation.Content = "This book is already borrowed.";
                                BorrowingInformation.Foreground = Brushes.Red;
                                objDBAccess.closeConn();
                            }
                            else //Allow book to be borrowed
                            {
                                string bookTitleQuery = "SELECT Title FROM Books WHERE BookID = '" + bookID + "' ";
                                objDBAccess.readDatathroughAdapter(bookTitleQuery, dtTitle);

                                // Check if the book is available for loan
                                string availabilityQuery = "SELECT Available FROM Books WHERE BookID = '" + bookID + "'  ";
                                objDBAccess.readDatathroughAdapter(availabilityQuery, availabilityTable);
                                objDBAccess.closeConn();

                                if (dtTitle.Rows.Count > 0)
                                {
                                    int Row = dtTitle.Rows.Count;
                                    string bookTitle = dtTitle.Rows[0]["Title"].ToString();
                                    int availableCopies = Convert.ToInt32(availabilityTable.Rows[0]["Available"]);

                                    if (availableCopies > 0)
                                    {
                                        DateTime loanDate = DateTime.Today; //Determine the borrowing time
                                        DateTime dueDate = DateTime.MinValue;

                                        if (Login.UserRank == "Student" || MainWindow.UserRank == "Student") //The end date of the loan by rank
                                        {
                                            dueDate = loanDate.AddDays(MaxBooks);
                                        }
                                        else if (Login.UserRank == "Teacher" || MainWindow.UserRank == "Teacher")
                                        {
                                            dueDate = loanDate.AddDays(MaxBooks);
                                        }

                                        if (dueDate != DateTime.MinValue)
                                        {
                                            string query = "INSERT INTO BookLoans (book_id, user_id, loan_date, due_date, Title) VALUES (@bookID, @userID, @loanDate, @dueDate, @Title)";
                                            SqlCommand command = new SqlCommand(query);
                                            command.Parameters.AddWithValue("@bookID", bookID);
                                            command.Parameters.AddWithValue("@userID", userID);
                                            command.Parameters.AddWithValue("@loanDate", loanDate);
                                            command.Parameters.AddWithValue("@dueDate", dueDate);
                                            command.Parameters.AddWithValue("@Title", bookTitle);
                                            int rowsAffected = objDBAccess.executeQuery(command);

                                            if (rowsAffected == 1) //Adding the book to the student schedule and deleting a copy from the library schedule
                                            {
                                                string updateQuery = "UPDATE Books SET Available = Available - 1 WHERE BookID = @bookID";
                                                SqlCommand updateCommand = new SqlCommand(updateQuery);
                                                updateCommand.Parameters.AddWithValue("@bookID", bookID);
                                                objDBAccess.executeQuery(updateCommand);

                                                BorrowingInformation.Content = "Book loan recorded successfully.";
                                                BorrowingInformation.Foreground = Brushes.Green; //Set the text color to green

                                                dtMyBook.Clear();
                                                dtTitle.Clear();
                                                availabilityTable.Clear();
                                                string queryMyBook = "SELECT * FROM BookLoans WHERE user_id = '" + userID + "' ";
                                                objDBAccess.readDatathroughAdapter(queryMyBook, dtMyBook);
                                                MyBookData.ItemsSource = dtMyBook.DefaultView;
                                                objDBAccess.closeConn();
                                            }
                                            else //Error messages
                                            {
                                                BorrowingInformation.Content = "Failed to record the book loan.";
                                                BorrowingInformation.Foreground = Brushes.Red;
                                                objDBAccess.closeConn();
                                            }
                                        }
                                        else
                                        {
                                            BorrowingInformation.Content = "Invalid due date.";
                                            BorrowingInformation.Foreground = Brushes.Red;
                                            objDBAccess.closeConn();
                                        }
                                    }
                                    else
                                    {
                                        BorrowingInformation.Content = "This book is not available for loan.";
                                        BorrowingInformation.Foreground = Brushes.Red;
                                        objDBAccess.closeConn();
                                    }
                                }
                            }
                        }
                        else
                        {
                            BorrowingInformation.Content = "You have reached the maximum limit for borrowed books.";
                            BorrowingInformation.Foreground = Brushes.Red;
                            objDBAccess.closeConn();
                        }
                    }
                    else
                    {
                        BorrowingInformation.Content = "Please enter a book ID.";
                        BorrowingInformation.Foreground = Brushes.Red;
                        objDBAccess.closeConn();
                    }
                }

                else
                {
                    BorrowingInformation.Content = "Please enter a book ID.";
                    BorrowingInformation.Foreground = Brushes.Red;
                }
            }
        }




        private void Update_Button_Click(object sender, RoutedEventArgs e) //Update account information
        {
            string newUserName = txtNameHome.Text;
            string newEmail = txtEmailHome.Text;
            int ID = Convert.ToInt32(txtID.Text);

            if (newUserName == string.Empty)
            {
                lblUpdateError.Content = "Please write your name.";               
            }
            else if (newEmail == string.Empty)
            {
                lblUpdateError.Content = "Please write your Email.";
            }


            else
            {
                lblUpdateError.Content = string.Empty;

                string query = "UPDATE Users SET Name = @userName, Email = @userEmail WHERE ID = @userId";
                SqlCommand updateCommand = new SqlCommand(query);
                updateCommand.Parameters.AddWithValue("@userName", newUserName);
                updateCommand.Parameters.AddWithValue("@userEmail", newEmail);
                updateCommand.Parameters.AddWithValue("@userId", ID);



                int row = objDBAccess.executeQuery(updateCommand);

                if (row == 1) //Return to the login page
                {
                    lblUpdateError.Content= "Account Information Updated Successfully.";

                    this.Close();
                    Login login = new Login();
                    login.Show();
                }
                else
                {
                    lblUpdateError.Content = "Error Occured. Try Again.";
                }
            }
        }

        private void DeletAccount_Click(object sender, RoutedEventArgs e)//delete account
        {
            int ID = Convert.ToInt32(txtID.Text);
            MessageBoxResult dialog = MessageBox.Show("Are you sure?", "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (dialog == MessageBoxResult.Yes)
            {
                string query = "DELETE from Users Where ID = '" + ID + "' ";
                SqlCommand deleteCommand = new SqlCommand(query);

                int row = objDBAccess.executeQuery(deleteCommand);

                if (row == 1)
                {
                    MessageBox.Show("Account Deleted Successfully.");

                    this.Hide();
                    MainWindow signUp = new MainWindow();
                    signUp.Show();
                }
                else
                {
                    lblUpdateError.Content = "Error Occured. Try Again.";
                }
            }
        }
        private void SelectBook_TextChanged(object sender, TextChangedEventArgs e) //The book that has been selected for borrowing
        {
            if (int.TryParse(txtBookID.Text, out int ID)) //Only numbers are allowed
            {
                DataView filteredView1 = dtBooks.DefaultView;
                filteredView1.RowFilter = "BookID >= " + ID.ToString();
                filteredView1.Sort = "BookID ASC";// Arrange the numbers in ascending order
                BookData.ItemsSource = filteredView1;
                
            }
            else if (txtBookID.Text == string.Empty)
            {
                dtBooks.Clear();
                string queryBook = "SELECT * FROM Books WHERE Available > 0";
                objDBAccess.readDatathroughAdapter(queryBook, dtBooks);
                BookData.ItemsSource = dtBooks.DefaultView;
                objDBAccess.closeConn();
            }
            else
            {
                BorrowingInformation.Content = "Enter a valid Book ID (numeric value).";
            }
        }


        private void Return_TextChanged(object sender, TextChangedEventArgs e) //Rearrange books according to the TextBox
        {
           /* string queryBook;
            if (int.TryParse(txtReturn.Text, out int ID))
            {
                int userID = Convert.ToInt32(txtID.Text);

                if (txtReturn.Text != string.Empty)
                {
                    dtMyBook.Clear();
                    queryBook = "SELECT * FROM BookLoans WHERE id = '" + ID + "' AND user_id = '" + userID + "'";
                    objDBAccess.readDatathroughAdapter(queryBook, dtMyBook);
                    if (dtMyBook.Rows.Count > 0)
                    {
                        MyBookData.ItemsSource = dtMyBook.DefaultView;
                        objDBAccess.closeConn();
                    }
                    else
                    {
                        dtMyBook.Clear();
                        queryBook = "SELECT * FROM BookLoans WHERE user_id = '" + userID + "'";
                        objDBAccess.readDatathroughAdapter(queryBook, dtMyBook);
                        MyBookData.ItemsSource = dtMyBook.DefaultView;
                        objDBAccess.closeConn();
                    }

                }
                else
                {
                    dtMyBook.Clear();
                    queryBook = "SELECT * FROM BookLoans WHERE user_id = '" + userID + "'";
                    objDBAccess.readDatathroughAdapter(queryBook, dtMyBook);
                    MyBookData.ItemsSource = dtMyBook.DefaultView;
                    objDBAccess.closeConn();
                }

            }
            else if (txtReturn.Text != string.Empty)
            {
                MessageBox.Show("Enter ID book");
            }*/
        }


        int RowCountReturn = 0;
        private void Return_Click(object sender, RoutedEventArgs e) //Return Books button
        {
            if (txtReturn.Text != string.Empty)
            {
                int BorrowID = Convert.ToInt32(txtReturn.Text);
                string bookIdQuery = "SELECT book_id FROM BookLoans WHERE id = '" + BorrowID + "' ";
                objDBAccess.readDatathroughAdapter(bookIdQuery, dtBorrow);

                try
                {
                    int bookID = Convert.ToInt32(dtBorrow.Rows[RowCountReturn]["book_id"]);
                    int UserID = Convert.ToInt32(txtID.Text);
                    RowCountReturn++;

                    string returnQuery = "DELETE FROM BookLoans WHERE id = '" + BorrowID + "'";
                    SqlCommand deleteCommand = new SqlCommand(returnQuery);

                    int row = objDBAccess.executeQuery(deleteCommand);
                    objDBAccess.closeConn();

                    if (row == 1)
                    {
                        CheckLoanExpiration();
                        lblReturnInformation.Content = "The borrowed book has been successfully deleted.";
                        lblReturnInformation.Foreground = Brushes.Green;

                        SqlCommand insertCommand = new SqlCommand("INSERT INTO BookApproval (book_id, user_id) VALUES (@BookID, @UserID)");
                        insertCommand.Parameters.AddWithValue("@BookID", bookID);
                        insertCommand.Parameters.AddWithValue("@UserID", UserID);
               
                        int rowsAffected = objDBAccess.executeQuery(insertCommand);

                        if (rowsAffected == 1)
                        {
                           //MessageBox.Show("True.");
                        }
                        else
                        {
                            MessageBox.Show("Error Occurred. Try Again.");
                            // Perform any error handling or display appropriate messages
                        }

                        dtMyBook.Clear();
                        string queryMyBook = "SELECT * FROM BookLoans WHERE user_id = '" + txtID.Text + "' ";
                        objDBAccess.readDatathroughAdapter(queryMyBook, dtMyBook);

                        MyBookData.ItemsSource = dtMyBook.DefaultView;
                        objDBAccess.closeConn();

                    }
                    else
                    {
                        lblReturnInformation.Content = "Error Occured. Try Again.";
                        lblReturnInformation.Foreground = Brushes.Red;
                        objDBAccess.closeConn();

                    }
                }
                catch (Exception ex)
                {
                    lblReturnInformation.Content = "The ID you entered does not exist";
                    lblReturnInformation.Foreground = Brushes.Red;
                    objDBAccess.closeConn();

                }
            }
            else
            {
                lblReturnInformation.Content = "The ID you entered does not exist";
                lblReturnInformation.Foreground = Brushes.Red;
                objDBAccess.closeConn();

            }
        }


        private void SearchAllBooks_TextChanged(object sender, TextChangedEventArgs e) //Search in the library
        {
            string searchQuery = txtAllBookSearch.Text;
            string selectedSearchType = ((ComboBoxItem)comboSearch.SelectedItem).Content.ToString();
            string orderBy = selectedSearchType +" ASC"; // Arrange the names in ascending order

            if (searchQuery != string.Empty)
            {
                dtBooks.Clear();
                string queryMyBook = "SELECT * FROM Books WHERE " + selectedSearchType + " LIKE '" + searchQuery + "%' ORDER BY " + orderBy;
                objDBAccess.readDatathroughAdapter(queryMyBook, dtBooks);
                BookData.ItemsSource = dtBooks.DefaultView;
                objDBAccess.closeConn();
            }
            else
            {
                dtBooks.Clear(); //Reload the table
                string queryMyBook = "SELECT * FROM Books ORDER BY " + orderBy;
                objDBAccess.readDatathroughAdapter(queryMyBook, dtBooks);
                BookData.ItemsSource = dtBooks.DefaultView;
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e) //log out
        {
            this.Close();
            Login login = new Login();
            login.Show();
        }
    }
}
