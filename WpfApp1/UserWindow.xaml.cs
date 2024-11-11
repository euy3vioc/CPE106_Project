using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private string currentUsername;
        public UserWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            LoadUserDetails(); // Load the full name and display welcome message
            LoadEvents(); // Load events when the form is initialized
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void btnReserveSeat_Click(object sender, RoutedEventArgs e)
        {
            if (cbEvents.SelectedItem != null && cbAvailableSeats.SelectedItem != null)
            {
                var selectedEvent = cbEvents.SelectedItem as dynamic;
                int eventId = Convert.ToInt32(selectedEvent.Value);
                int seatNumber = Convert.ToInt32(cbAvailableSeats.SelectedItem.ToString());

                using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
                {
                    conn.Open();
                    string query = "INSERT INTO seats (event_id, seat_number, reserved_by) VALUES (@EventId, @SeatNumber, @ReservedBy)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventId", eventId);
                        cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                        cmd.Parameters.AddWithValue("@ReservedBy", currentUsername);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Seat {seatNumber} reserved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadReservedSeats(eventId); // Refresh reserved seats list
                LoadAvailableSeats(eventId); // Refresh available seats list
            }
            else
            {
                MessageBox.Show("Please select an event and a seat.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadAvailableSeats(int eventId)
        {
            cbAvailableSeats.Items.Clear();

            // Seat numbers are from 1 to 24
            List<int> reservedSeats = new List<int>();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT seat_number FROM seats WHERE event_id = @EventId AND reserved_by IS NOT NULL";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EventId", eventId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservedSeats.Add(Convert.ToInt32(reader["seat_number"]));
                        }
                    }
                }
            }

            // Add available seats (1-24) excluding reserved ones
            for (int i = 1; i <= 24; i++)
            {
                if (!reservedSeats.Contains(i))
                {
                    cbAvailableSeats.Items.Add(i.ToString());
                }
            }
        }

        private void LoadReservedSeats(int eventId)
        {
            lbReservedSeats.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                // Modify the query to filter by reserved_by and the logged-in username
                string query = "SELECT seat_number FROM seats WHERE event_id = @EventId AND reserved_by = @ReservedBy";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EventId", eventId);
                    cmd.Parameters.AddWithValue("@ReservedBy", currentUsername); // Use the logged-in username

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lbReservedSeats.Items.Add("Seat " + reader["seat_number"]);
                        }
                    }
                }
            }
        }

        private void cbEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbEvents.SelectedItem != null)
            {
                var selectedEvent = cbEvents.SelectedItem as dynamic;
                int eventId = Convert.ToInt32(selectedEvent.Value);  // Corrected the conversion
                LoadReservedSeats(eventId); // Load reserved seats
                LoadAvailableSeats(eventId); // Load available seats
            }
        }
        private void LoadEvents()
        {
            cbEvents.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT id, name FROM events";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var eventItem = new KeyValuePair<string, object>(reader["name"].ToString(), reader["id"]);
                            cbEvents.Items.Add(eventItem);
                        }
                    }
                }
            }
        }
        private void LoadUserDetails()
        {
            string fullName = string.Empty;

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT fullname FROM users WHERE username = @Username";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", currentUsername);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fullName = reader["fullname"].ToString();
                        }
                    }
                }
            }

            // Display the welcome message with the user's full name
            lblWelcome.Content = $"Welcome, {fullName}!";
        }
    }
}
