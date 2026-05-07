using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для AddPenaltyWindow.xaml
    /// </summary>
    public partial class AddPenaltyWindow : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private DataRowView _selectedRow = null;

        public AddPenaltyWindow()
        {
            InitializeComponent();
            LoadUsersData();
            IssueDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadUsersData()
        {
            string query = @"
                SELECT
                    u.user_id,
                    u.surname,
                    u.name,
                    u.patronymic
                FROM Users u
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Выберите пользователя для штрафа.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Введите корректную сумму штрафа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
            {
                MessageBox.Show("Введите причину штрафа.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IssueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату выписки штрафа.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int userId = Convert.ToInt32(_selectedRow["user_id"]);
            DateOnly issueDate = DateOnly.FromDateTime(IssueDatePicker.SelectedDate.Value);
            bool isPaid = IsPaidCheckBox.IsChecked == true;

            string insertQuery = @"
                INSERT INTO Penalties (user_id, amount, reason, issue_date, is_paid)
                VALUES (@userId, @amount, @reason, @issueDate, @isPaid)";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.Parameters.AddWithValue("@reason", ReasonTextBox.Text);
                        cmd.Parameters.AddWithValue("@issueDate", issueDate.ToDateTime(TimeOnly.MinValue));
                        cmd.Parameters.AddWithValue("@isPaid", isPaid);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Штраф успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить штраф.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
