using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для EditPenaltyWindow.xaml
    /// </summary>
    public partial class EditPenaltyWindow : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private int _penaltyId;

        public EditPenaltyWindow(int penaltyId)
        {
            InitializeComponent();
            _penaltyId = penaltyId;
            LoadPenaltyData();
        }

        private void LoadPenaltyData()
        {
            string query = @"
                SELECT
                    p.penalty_id,
                    p.user_id,
                    u.surname,
                    u.name,
                    u.patronymic,
                    p.amount,
                    p.reason,
                    p.issue_date,
                    p.is_paid
                FROM Penalties p
                INNER JOIN Users u ON p.user_id = u.user_id
                WHERE p.penalty_id = @penaltyId";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@penaltyId", _penaltyId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];

                            string fullName = $"{row["surname"]} {row["name"]}";
                            if (row["patronymic"] != DBNull.Value && !string.IsNullOrEmpty(row["patronymic"].ToString()))
                            {
                                fullName += $" {row["patronymic"]}";
                            }
                            UserFullNameTextBlock.Text = fullName;

                            AmountTextBox.Text = row["amount"].ToString();
                            ReasonTextBox.Text = row["reason"].ToString();
                            
                            if (row["issue_date"] != DBNull.Value)
                            {
                                IssueDatePicker.SelectedDate = Convert.ToDateTime(row["issue_date"]);
                            }

                            IsPaidCheckBox.IsChecked = Convert.ToBoolean(row["is_paid"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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

            DateOnly issueDate = DateOnly.FromDateTime(IssueDatePicker.SelectedDate.Value);
            bool isPaid = IsPaidCheckBox.IsChecked == true;

            string updateQuery = @"
                UPDATE Penalties
                SET amount = @amount,
                    reason = @reason,
                    issue_date = @issueDate,
                    is_paid = @isPaid
                WHERE penalty_id = @penaltyId";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.Parameters.AddWithValue("@reason", ReasonTextBox.Text);
                        cmd.Parameters.AddWithValue("@issueDate", issueDate.ToDateTime(TimeOnly.MinValue));
                        cmd.Parameters.AddWithValue("@isPaid", isPaid);
                        cmd.Parameters.AddWithValue("@penaltyId", _penaltyId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные штрафа успешно обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные штрафа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
