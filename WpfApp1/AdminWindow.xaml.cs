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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            LoadEvents();
        }

        // Load all events into the dropdown
        private void LoadEvents()
        {
            cbEvents.Items.Clear();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT id, name FROM events";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cbEvents.Items.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
            }
            cbEvents.DisplayMemberPath = "Value";
            cbEvents.SelectedValuePath = "Key";
        }

        // Create a new event


        // Load reserved seats for a selected event
        private void LoadReservedSeats(int eventId)
        {
            lbReservedSeats.Items.Clear();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT seat_number, reserved_by FROM seats WHERE event_id = @EventId AND reserved_by IS NOT NULL";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EventId", eventId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lbReservedSeats.Items.Add($"Seat {reader.GetInt32(0)} - Reserved by {reader.GetString(1)}");
                        }
                    }
                }
            }
        }

        // Trigger loading reserved seats when an event is selected


        // Revoke a reserved seat


        // Delete an event


        // Logout button

        private void cbEvents_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (cbEvents.SelectedValue != null)
            {
                int eventId = (int)cbEvents.SelectedValue;
                LoadReservedSeats(eventId);
            }
        }

        private void btnLogout_Click_1(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void btnDeleteEvent_Click_1(object sender, RoutedEventArgs e)
        {
            if (cbEvents.SelectedValue == null) return;

            int eventId = (int)cbEvents.SelectedValue;
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string deleteSeatsQuery = "DELETE FROM seats WHERE event_id = @EventId";
                string deleteEventQuery = "DELETE FROM events WHERE id = @EventId";

                using (SQLiteCommand deleteSeatsCmd = new SQLiteCommand(deleteSeatsQuery, conn))
                using (SQLiteCommand deleteEventCmd = new SQLiteCommand(deleteEventQuery, conn))
                {
                    deleteSeatsCmd.Parameters.AddWithValue("@EventId", eventId);
                    deleteEventCmd.Parameters.AddWithValue("@EventId", eventId);

                    deleteSeatsCmd.ExecuteNonQuery();
                    deleteEventCmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Event deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadEvents();
            lbReservedSeats.Items.Clear();
        }

        private void btnRevokeSeat_Click_1(object sender, RoutedEventArgs e)
        {
            if (cbEvents.SelectedValue == null || lbReservedSeats.SelectedItem == null) return;

            int eventId = (int)cbEvents.SelectedValue;
            string selectedSeat = lbReservedSeats.SelectedItem.ToString();
            int seatNumber = int.Parse(selectedSeat.Split(' ')[1]); // Parse seat number from string

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "DELETE FROM seats WHERE event_id = @EventId AND seat_number = @SeatNumber";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EventId", eventId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show($"Seat {seatNumber} reservation revoked and entry deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadReservedSeats(eventId); // Refresh reserved seats list
        }

        private void btnCreateEvent_Click_1(object sender, RoutedEventArgs e)
        {
            string eventName = txtEventName.Text.Trim();
            if (string.IsNullOrEmpty(eventName))
            {
                MessageBox.Show("Event name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=app_data.db;Version=3;"))
            {
                conn.Open();
                string query = "INSERT INTO events (name) VALUES (@Name)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", eventName);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Event created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEvents();
                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show("Error creating event: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            txtEventName.Clear();
        }
    }
}
