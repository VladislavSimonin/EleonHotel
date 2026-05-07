using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для editEmployee.xaml
    /// </summary>
    public partial class editEmployee : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private int _employeeId;
        private DataRowView _selectedShift = null;
        private DataRowView _selectedPosition = null;

        public editEmployee(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            LoadEmployeeData();
            LoadShifts();
            LoadPositions();
        }

        private void LoadEmployeeData()
        {
            string query = @"
                SELECT
                    e.employee_id,
                    e.salary,
                    e.shift_id,
                    e.position_id,
                    u.surname,
                    u.name,
                    u.patronymic,
                    u.phone,
                    u.email
                FROM Employees e
                INNER JOIN Users u ON e.user_id = u.user_id
                WHERE e.employee_id = @employeeId";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@employeeId", _employeeId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];
                            
                            SalaryTextBox.Text = row["salary"] != DBNull.Value ? row["salary"].ToString() : "";
                            
                            SurnameTextBox.Text = row["surname"].ToString();
                            NameTextBox.Text = row["name"].ToString();
                            PatronymicTextBox.Text = row["patronymic"] != DBNull.Value ? row["patronymic"].ToString() : "";
                            PhoneTextBox.Text = row["phone"].ToString();
                            EmailTextBox.Text = row["email"].ToString();

                            // Загружаем смены и выбираем текущую
                            LoadShifts(row["shift_id"]);
                            
                            // Загружаем должности и выбираем текущую
                            LoadPositions(row["position_id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadShifts(object currentShiftId = null)
        {
            string query = "SELECT shift_id, shift_time_start, shift_time_end FROM Shifts ORDER BY shift_time_start";

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

                        // Добавляем столбец для отображения
                        dt.Columns.Add("DisplayText", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            var startTime = row["shift_time_start"];
                            var endTime = row["shift_time_end"];
                            row["DisplayText"] = $"{startTime} - {endTime}";
                        }

                        ShiftComboBox.ItemsSource = dt.DefaultView;
                        
                        if (currentShiftId != null && currentShiftId != DBNull.Value)
                        {
                            ShiftComboBox.SelectedValue = Convert.ToInt32(currentShiftId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке смен: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPositions(object currentPositionId = null)
        {
            string query = "SELECT position_id, position_name FROM Positions ORDER BY position_name";

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

                        // Добавляем столбец для отображения
                        dt.Columns.Add("DisplayText", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            row["DisplayText"] = row["position_name"].ToString();
                        }

                        PositionComboBox.ItemsSource = dt.DefaultView;
                        
                        if (currentPositionId != null && currentPositionId != DBNull.Value)
                        {
                            PositionComboBox.SelectedValue = Convert.ToInt32(currentPositionId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке должностей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            decimal? salary = null;
            if (!string.IsNullOrWhiteSpace(SalaryTextBox.Text))
            {
                if (!decimal.TryParse(SalaryTextBox.Text, out decimal parsedSalary))
                {
                    MessageBox.Show("Некорректное значение зарплаты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                salary = parsedSalary;
            }

            int? shiftId = null;
            if (ShiftComboBox.SelectedItem is DataRowView selectedShiftRow)
            {
                if (selectedShiftRow["shift_id"] != DBNull.Value)
                {
                    shiftId = Convert.ToInt32(selectedShiftRow["shift_id"]);
                }
            }

            int? positionId = null;
            if (PositionComboBox.SelectedItem is DataRowView selectedPositionRow)
            {
                if (selectedPositionRow["position_id"] != DBNull.Value)
                {
                    positionId = Convert.ToInt32(selectedPositionRow["position_id"]);
                }
            }

            string updateQuery = @"
                UPDATE Employees
                SET salary = @salary,
                    shift_id = @shiftId,
                    position_id = @positionId
                WHERE employee_id = @employeeId";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        if (salary.HasValue)
                            cmd.Parameters.AddWithValue("@salary", salary.Value);
                        else
                            cmd.Parameters.AddWithValue("@salary", DBNull.Value);

                        if (shiftId.HasValue)
                            cmd.Parameters.AddWithValue("@shiftId", shiftId.Value);
                        else
                            cmd.Parameters.AddWithValue("@shiftId", DBNull.Value);

                        if (positionId.HasValue)
                            cmd.Parameters.AddWithValue("@positionId", positionId.Value);
                        else
                            cmd.Parameters.AddWithValue("@positionId", DBNull.Value);

                        cmd.Parameters.AddWithValue("@employeeId", _employeeId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные сотрудника успешно обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
