using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для deleteUser.xaml
    /// </summary>
    public partial class deleteUser : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private DataRowView _selectedRow = null;

        public deleteUser()
        {
            InitializeComponent();
            LoadUsersData();
        }

        private void LoadUsersData()
        {
            string query = @"
                SELECT
                    user_id,
                    surname,
                    name,
                    patronymic,
                    phone,
                    email
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

        private void UsersDataGrid_SelectedRowChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem != null)
            {
                _selectedRow = UsersDataGrid.SelectedItem as DataRowView;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var userId = _selectedRow["user_id"];
            var userName = $"{_selectedRow["surname"]} {_selectedRow["name"]}";

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя \"{userName}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string query = "DELETE FROM Users WHERE user_id = @userId";

                try
                {
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Пользователь успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadUsersData(); // Обновить список
                                _selectedRow = null;
                            }
                            else
                            {
                                MessageBox.Show("Не удалось удалить пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}