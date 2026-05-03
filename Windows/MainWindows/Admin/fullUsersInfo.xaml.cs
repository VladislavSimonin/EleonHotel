using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для fullUsersInfo.xaml
    /// </summary>
    public partial class fullUsersInfo : Window
    {
        private const string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public fullUsersInfo()
        {
            InitializeComponent();
        }

        private void LoadUsersData()
        {
            string query = @"
                SELECT 
                    user_id AS [ID],
                    first_name AS [Имя],
                    last_name AS [Фамилия],
                    middle_name AS [Отчество],
                    passport_series AS [Серия паспорта],
                    passport_number AS [Номер паспорта],
                    address AS [Адрес],
                    phone AS [Телефон],
                    email AS [Email],
                    date_of_birth AS [Дата рождения]
                FROM Users";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        UsersDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

