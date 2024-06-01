using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace UserManagementApp
{
    public partial class UserWindow : Window
    {
        private string connectionString = "Server=localhost;Database=UserManagement;Uid=root;Pwd=admin123;";
        private int userId;
        private bool isLoggedIn;

        public ObservableCollection<UserLog> UserLogs { get; set; }

        public UserWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            UserLogs = new ObservableCollection<UserLog>();
            userLogsDataGrid.ItemsSource = UserLogs;
            LoadUserStatus();
            LoadUserLogs();
        }

        private void LoadUserStatus()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT login_time, logout_time FROM users WHERE id = @userId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isLoggedIn = !reader.IsDBNull(reader.GetOrdinal("login_time")) && reader.IsDBNull(reader.GetOrdinal("logout_time"));
                            UpdateButtonState();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user status: " + ex.Message);
            }
        }

        private void LoadUserLogs()
        {
            UserLogs.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        (SELECT login_time, logout_time, rendered_time FROM users WHERE id = @userId)
                        UNION ALL
                        (SELECT login_time, logout_time, rendered_time FROM user_logs WHERE user_id = @userId)
                        ORDER BY login_time DESC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserLogs.Add(new UserLog
                            {
                                LoginTime = reader.GetDateTime("login_time"),
                                LogoutTime = reader.IsDBNull(reader.GetOrdinal("logout_time")) ? (DateTime?)null : reader.GetDateTime("logout_time"),
                                RenderedTime = reader.IsDBNull(reader.GetOrdinal("rendered_time")) ? (TimeSpan?)null : reader.GetTimeSpan("rendered_time")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user logs: " + ex.Message);
            }
        }

        private void UpdateButtonState()
        {
            loginButton.Content = isLoggedIn ? "Logout" : "Login";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    if (isLoggedIn)
                    {
                        // Logout
                        string query = "UPDATE users SET logout_time = NOW(), rendered_time = TIMEDIFF(NOW(), login_time) WHERE id = @userId";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();

                        // Insert into user_logs
                        string insertLogQuery = "INSERT INTO user_logs (user_id, login_time, logout_time, rendered_time) SELECT id, login_time, NOW(), TIMEDIFF(NOW(), login_time) FROM users WHERE id = @userId";
                        MySqlCommand insertCmd = new MySqlCommand(insertLogQuery, conn);
                        insertCmd.Parameters.AddWithValue("@userId", userId);
                        insertCmd.ExecuteNonQuery();

                        // Close the UserWindow and open the LoginWindow
                        LoginWindow loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        // Login
                        string query = "UPDATE users SET login_time = NOW(), logout_time = NULL, rendered_time = NULL WHERE id = @userId";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();

                        // Update login status
                        isLoggedIn = true;

                        // Reload user status and logs
                        LoadUserStatus();
                        LoadUserLogs();
                        UpdateButtonState();

                        // Refresh the DataGrid to show the updated logs
                        userLogsDataGrid.Items.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user status: " + ex.Message);
            }
        }
    }

    public class UserLog
    {
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public TimeSpan? RenderedTime { get; set; }
    }
}
