using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для staffInfo.xaml
    /// </summary>
    public partial class staffInfo : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private DataRowView _selectedRow = null;

        public staffInfo()
        {
            InitializeComponent();
            LoadStaffData();
        }

        private void LoadStaffData()
        {
            string query = @"
                SELECT
                    e.employee_id,
                    u.surname,
                    u.name,
                    u.patronymic,
                    u.phone,
                    u.email,
                    s.shift_time_start,
                    s.shift_time_end,
                    p.position_name,
                    e.salary
                FROM Employees e
                INNER JOIN Users u ON e.user_id = u.user_id
                LEFT JOIN Shifts s ON e.shift_id = s.shift_id
                LEFT JOIN Positions p ON e.position_id = p.position_id
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

                        StaffDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StaffDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StaffDataGrid.SelectedItem != null)
            {
                _selectedRow = StaffDataGrid.SelectedItem as DataRowView;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new addEmployee();
            addWindow.ShowDialog();
            if (addWindow.DialogResult == true)
            {
                LoadStaffData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int employeeId = Convert.ToInt32(_selectedRow["employee_id"]);
            var editWindow = new editEmployee(employeeId);
            editWindow.ShowDialog();
            if (editWindow.DialogResult == true)
            {
                LoadStaffData();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var employeeId = _selectedRow["employee_id"];
            var fullName = $"{_selectedRow["surname"]} {_selectedRow["name"]} {_selectedRow["patronymic"]}".Trim();

            var result1 = MessageBox.Show($"Действительно ли вы хотите удалить данного сотрудника: {fullName}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result1 == MessageBoxResult.Yes)
            {
                var result2 = MessageBox.Show("Данное действие нельзя будет отменить. Вы уверены?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result2 == MessageBoxResult.Yes)
                {
                    string query = "DELETE FROM Employees WHERE employee_id = @employeeId";

                    try
                    {
                        using (var conn = new SqlConnection(ConnectionString))
                        {
                            conn.Open();
                            using (var cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Сотрудник успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadStaffData();
                                    _selectedRow = null;
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось удалить сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
