using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для assignEmployeeDetails.xaml
    /// </summary>
    public partial class assignEmployeeDetails : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private int _userId;
        private int? _employeeId;

        public assignEmployeeDetails(int userId, int? employeeId = null)
        {
            InitializeComponent();
            _userId = userId;
            _employeeId = employeeId;
            LoadUserData();
            LoadShifts();
            LoadPositions();
        }

        private void LoadUserData()
        {
            string query = @"
                SELECT
                    u.surname,
                    u.name,
                    u.patronymic,
                    u.phone,
                    u.email,
                    e.salary
                FROM Users u
                LEFT JOIN Employees e ON u.user_id = e.user_id
                WHERE u.user_id = @userId";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", _userId);
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

                            // Если это редактирование существующего сотрудника, загружаем текущие значения
                            if (_employeeId.HasValue)
                            {
                                LoadCurrentShiftAndPosition(_employeeId.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCurrentShiftAndPosition(int employeeId)
        {
            string query = @"
                SELECT shift_id, position_id
                FROM Employees
                WHERE employee_id = @employeeId";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@employeeId", employeeId);
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            object currentShiftId = reader["shift_id"];
                            object currentPositionId = reader["position_id"];

                            // Устанавлием выбранные значения после загрузки списков
                            if (currentShiftId != DBNull.Value)
                            {
                                ShiftComboBox.SelectedValue = Convert.ToInt32(currentShiftId);
                            }

                            if (currentPositionId != DBNull.Value)
                            {
                                PositionComboBox.SelectedValue = Convert.ToInt32(currentPositionId);
                            }
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке текущих значений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadShifts()
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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке смен: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPositions()
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

            // Получаем значения из ComboBox через SelectedItem
            int? shiftId = null;
            int? positionId = null;

            if (ShiftComboBox.SelectedItem is DataRowView shiftRow)
            {
                shiftId = Convert.ToInt32(shiftRow["shift_id"]);
            }

            if (PositionComboBox.SelectedItem is DataRowView positionRow)
            {
                positionId = Convert.ToInt32(positionRow["position_id"]);
            }

            // Проверяем, существует ли уже запись в Employees для этого пользователя
            string checkQuery = "SELECT employee_id FROM Employees WHERE user_id = @userId";
            int? employeeId = null;

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", _userId);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            employeeId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке существования сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string query;
            if (employeeId.HasValue)
            {
                // Обновляем существующую запись
                query = @"
                    UPDATE Employees
                    SET salary = @salary,
                        shift_id = @shiftId,
                        position_id = @positionId
                    WHERE employee_id = @employeeId";
            }
            else
            {
                // Создаем новую запись
                query = @"
                    DECLARE @newEmployeeId INT;
                    SELECT @newEmployeeId = ISNULL(MAX(employee_id), 0) + 1 FROM Employees;

                    INSERT INTO Employees (employee_id, user_id, salary, shift_id, position_id)
                    VALUES (@newEmployeeId, @userId, @salary, @shiftId, @positionId)";
            }

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
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

                        cmd.Parameters.AddWithValue("@userId", _userId);

                        if (employeeId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@employeeId", employeeId.Value);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные сотрудника успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось сохранить данные сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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