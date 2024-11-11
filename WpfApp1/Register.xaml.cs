using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data.SQLite;
using System.Security.Cryptography;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Validate the form fields
            if (string.IsNullOrWhiteSpace(newfullnametxt.Text) ||
                string.IsNullOrWhiteSpace(newusertxt.Text) ||
                string.IsNullOrWhiteSpace(newpasswordtxt.Password)
                )
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Hash the password for secure storage
            string hashedPassword = HashPassword(newpasswordtxt.Password);

            // Connect to SQLite and insert the new user record
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @Username";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", newusertxt.Text);
                        long userExists = (long)checkCmd.ExecuteScalar();

                        if (userExists > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different username.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    string query = "INSERT INTO users (fullname, username, password) VALUES (@FullName, @Username, @Password)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", newfullnametxt.Text);
                        cmd.Parameters.AddWithValue("@Username", newusertxt.Text);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Close the register window
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during registration. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
