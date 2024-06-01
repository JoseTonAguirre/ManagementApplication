using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace UserManagementApp
{
    public partial class SuperUserWindow : Window
    {
        private string connectionString = "Server=localhost;Database=UserManagement;Uid=root;Pwd=admin123;";
        public ObservableCollection<User> Users { get; set; }

        public SuperUserWindow()
        {
            InitializeComponent();
            Users = new ObservableCollection<User>();
            usersDataGrid.ItemsSource = Users;
            LoadUsers();
        }

        private void LoadUsers()
        {
            Users.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, first_name, surname, username, login_time, logout_time, rendered_time FROM users";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Users.Add(new User
                            {
                                Id = reader.GetInt32("id"),
                                FirstName = reader.GetString("first_name"),
                                Surname = reader.GetString("surname"),
                                Username = reader.GetString("username"),
                                LoginTime = reader.IsDBNull(reader.GetOrdinal("login_time")) ? (DateTime?)null : reader.GetDateTime("login_time"),
                                LogoutTime = reader.IsDBNull(reader.GetOrdinal("logout_time")) ? (DateTime?)null : reader.GetDateTime("logout_time"),
                                RenderedTime = reader.IsDBNull(reader.GetOrdinal("rendered_time")) ? (TimeSpan?)null : reader.GetTimeSpan("rendered_time")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.Closed += (s, args) => LoadUsers(); // Refresh the user list when AddUserWindow is closed
            addUserWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the LoginWindow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public TimeSpan? RenderedTime { get; set; }
    }
}
