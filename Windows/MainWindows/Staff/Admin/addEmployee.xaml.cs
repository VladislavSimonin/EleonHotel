using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для addEmployee.xaml
    /// </summary>
    public partial class addEmployee : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private DataRowView _selectedRow = null;

        public addEmployee()
        {
            InitializeComponent();
            LoadUsersData();
        }

        private void LoadUsersData()
        {
            // Выбираем пользователей, у которых есть employee_id и user_id, но нет значений в остальных полях Employees
            string query = @"
                SELECT
                    u.user_id,
                    u.surname,
                    u.name,
                    u.patronymic
                FROM Users u
                INNER JOIN Employees e ON u.user_id = e.user_id
                WHERE (e.shift_id IS NULL OR e.shift_id = 0)
                  AND (e.salary IS NULL OR e.salary = 0)
                  AND (e.position_id IS NULL OR e.position_id = 0)
                ORDER BY u.surname";

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

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem != null)
            {
                _selectedRow = UsersDataGrid.SelectedItem as DataRowView;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Выберите пользователя для добавления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int userId = Convert.ToInt32(_selectedRow["user_id"]);

            // Открываем диалоговое окно для назначения смены, зарплаты и должности
            var assignWindow = new assignEmployeeDetails(userId);
            bool? result = assignWindow.ShowDialog();

            if (result == true)
            {
                // Перезагружаем список пользователей после успешного добавления
                LoadUsersData();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}