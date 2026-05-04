using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using EleonHotel.Properties;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для roomsCondition.xaml
    /// </summary>
    public partial class roomsCondition : Window
    {
        private const string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private List<string> _roomStatuses = new List<string>();

        public roomsCondition()
        {
            InitializeComponent();
            LoadRoomStatuses();
            LoadRoomsData();
        }

        private void LoadRoomStatuses()
        {
            string query = "SELECT room_status FROM Room_statuses";
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            _roomStatuses.Add(reader.GetString(0));
                        }
                    }
                }

                var statusColumn = RoomsDataGrid.Columns[5] as DataGridComboBoxColumn;
                if (statusColumn != null)
                {
                    statusColumn.ItemsSource = _roomStatuses;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRoomsData()
        {
            string query = @"
                SELECT 
                    r.room_id,
                    rc.room_category_name,
                    rc.max_occupancy,
                    rc.cost,
                    rc.description,
                    rs.room_status,
                    ISNULL(STUFF((
                        SELECT ', ' + fl.room_facility_name
                        FROM Room_facilities rf
                        INNER JOIN Facilities_list fl ON rf.room_facility_id = fl.room_facility_id
                        WHERE rf.room_id = r.room_id
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), 'Нет') AS room_facility_name
                FROM Rooms r
                INNER JOIN Room_categories rc ON r.room_category_id = rc.room_category_id
                INNER JOIN Room_statuses rs ON r.room_status_id = rs.room_status_id
                ORDER BY r.room_id";

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
                        RoomsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void RoomsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var row = e.Row.DataContext as DataRowView;
            if (row == null) return;

            int roomId = Convert.ToInt32(row["room_id"]);
            string newStatus = row["room_status"]?.ToString();
            decimal newCost;
            string newDescription = row["description"]?.ToString();

            if (!decimal.TryParse(row["cost"]?.ToString(), out newCost))
            {
                MessageBox.Show("Некорректное значение цены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
                return;
            }

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    if (!string.IsNullOrEmpty(newStatus))
                    {
                        string getStatusIdQuery = "SELECT room_status_id FROM Room_statuses WHERE room_status = @status";
                        using (var cmd = new SqlCommand(getStatusIdQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@status", newStatus);
                            var statusIdObj = cmd.ExecuteScalar();
                            if (statusIdObj != null)
                            {
                                int statusId = Convert.ToInt32(statusIdObj);
                                string updateStatusQuery = "UPDATE Rooms SET room_status_id = @statusId WHERE room_id = @roomId";
                                using (var updateCmd = new SqlCommand(updateStatusQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@statusId", statusId);
                                    updateCmd.Parameters.AddWithValue("@roomId", roomId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    string updateCostQuery = @"
                        UPDATE Room_categories 
                        SET cost = @cost, description = @description
                        WHERE room_category_id = (
                            SELECT room_category_id FROM Rooms WHERE room_id = @roomId
                        )";
                    using (var updateCmd = new SqlCommand(updateCostQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@cost", newCost);
                        updateCmd.Parameters.AddWithValue("@description", (object)newDescription ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@roomId", roomId);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Данные успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRoomsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRoomsData();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
