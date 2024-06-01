using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace UserManagementApp
{
    public partial class AddUserWindow : Window
    {
        private string connectionString = "Server=localhost;Database=UserManagement;Uid=root;Pwd=admin123;";

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password != confirmPasswordBox.Password)
            {
                MessageBox.Show("Error: Password does not match");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO users (first_name, surname, username, password) VALUES (@FirstName, @Surname, @Username, @Password)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@FirstName", firstNameTextBox.Text);
                    cmd.Parameters.AddWithValue("@Surname", surnameTextBox.Text);
                    cmd.Parameters.AddWithValue("@Username", usernameTextBox.Text);
                    cmd.Parameters.AddWithValue("@Password", passwordBox.Password); // Ideally, hash the password

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("User added successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding user: " + ex.Message);
            }
        }
    }
}
