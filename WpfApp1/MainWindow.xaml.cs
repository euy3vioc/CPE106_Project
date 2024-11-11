using System.Data.SQLite;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EnsureDatabaseTables();
        }

        private void EnsureDatabaseTables()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();

                string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                password TEXT NOT NULL,
                fullname TEXT NOT NULL
            );";

                string createEventsTable = @"
            CREATE TABLE IF NOT EXISTS events (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE
            );";

                string createSeatsTable = @"
            CREATE TABLE IF NOT EXISTS seats (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                event_id INTEGER,
                seat_number INTEGER,
                reserved_by TEXT,
                FOREIGN KEY(event_id) REFERENCES events(id)
            );";

                using (SQLiteCommand cmd = new SQLiteCommand(createUsersTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SQLiteCommand cmd = new SQLiteCommand(createEventsTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SQLiteCommand cmd = new SQLiteCommand(createSeatsTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameText.Text;
            string password = PasswordText.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
                {
                    conn.Open();
                    string query = "SELECT password FROM users WHERE username = @Username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        object result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            MessageBox.Show("User not found.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string storedPassword = result.ToString();
                        string hashedInputPassword = HashPassword(password);

                        if (storedPassword == hashedInputPassword)
                        {
                            MessageBox.Show("Login successful!", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);

                            if (username == "admin")
                            {
                                AdminWindow adminWindow = new AdminWindow();
                                adminWindow.Show();
                            }
                            else
                            {
                                UserWindow userWindow = new UserWindow(username);
                                userWindow.Show();
                            }

                            this.Close(); // Close the login window
                        }
                        else
                        {
                            MessageBox.Show("Incorrect password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during login. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Register register = new Register();
            register.Show();
        }
    }
}