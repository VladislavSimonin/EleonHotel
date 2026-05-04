using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using EleonHotel.Properties;

namespace EleonHotel.Windows.AdminWindows
{
    public partial class roomsCondition : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private DataTable _roomsTable;
        private DataTable _statusesTable; 

        public roomsCondition()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {

            string query = @"
                SELECT 
                    r.room_id,
                    rc.room_category_name,
                    rc.max_occupancy,
                    r.number,
                    rs.room_status_id,
                    rs.room_status,
                    rc.cost,
                    rc.description,
                    COALESCE(
                        STUFF((
                            SELECT ', ' + f.room_facility_name
                            FROM Room_facilities rf_link
                            JOIN Facilities_list f ON rf_link.room_facility_id = f.room_facility_id
                            WHERE rf_link.room_id = r.room_id
                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 
                        1, 2, ''), 
                    'Нет удобств') as facilities_list
                FROM Rooms r
                JOIN Room_categories rc ON r.room_category_id = rc.room_category_id
                JOIN Room_statuses rs ON r.room_status_id = rs.room_status_id
                ORDER BY r.number";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        _roomsTable = new DataTable();
                        adapter.Fill(_roomsTable);

                        // Загружаем список статусов отдельно для ComboBox
                        LoadStatusesList();

                        RoomsDataGrid.ItemsSource = _roomsTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatusesList()
        {
            string query = "SELECT room_status_id, room_status FROM Room_statuses ORDER BY room_status_id";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    _statusesTable = new DataTable();
                    adapter.Fill(_statusesTable);

                    // Присваиваем список статусов контексту данных окна
                    this.DataContext = new { StatusesList = _statusesTable };
                }
            }
        }

        private void RoomsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                DataRowView rowView = e.Row.Item as DataRowView;
                if (rowView != null)
                {
                    SaveRoomChanges(rowView);
                }
            }
        }

        private void SaveRoomChanges(DataRowView rowView)
        {
            int roomId = Convert.ToInt32(rowView["room_id"]);
            int newStatusId = Convert.ToInt32(rowView["room_status_id"]);

            // Проверка на случай если цена пустая или некорректная
            decimal newcost = 0;
            if (!decimal.TryParse(rowView["cost"]?.ToString(), out newcost))
            {
                MessageBox.Show("Некорректное значение цены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                rowView.Row.RejectChanges();
                return;
            }

            string newDescription = rowView["description"]?.ToString() ?? "";

            string updateQuery = @"
                BEGIN TRANSACTION;

BEGIN TRY
    UPDATE rc
    SET rc.cost = @cost, 
        rc.description = @description
    FROM Room_categories AS rc
    INNER JOIN Rooms AS r ON r.room_category_id = rc.room_category_id
    WHERE r.room_id = @room_id;

    UPDATE Rooms 
    SET room_status_id = @status_id 
    WHERE room_id = @room_id;

    COMMIT TRANSACTION;
    PRINT 'Данные успешно обновлены в обеих таблицах.';
END TRY
BEGIN CATCH
    -- Если произошла ошибка, откатываем все изменения
    ROLLBACK TRANSACTION;
    PRINT 'Произошла ошибка. Изменения отменены.';
    THROW; -- Выводит детали ошибки
END CATCH;";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@status_id", newStatusId);
                        cmd.Parameters.AddWithValue("@cost", newcost);
                        cmd.Parameters.AddWithValue("@description", newDescription);
                        cmd.Parameters.AddWithValue("@room_id", roomId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Обновляем отображаемое имя статуса в таблице после сохранения
                            UpdateStatusNameInTable(rowView, newStatusId);

                            // Принимаем изменения в DataTable, чтобы они считались сохраненными
                            rowView.Row.AcceptChanges();

                            MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить запись в БД.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            rowView.Row.RejectChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                rowView.Row.RejectChanges();
            }
        }

        private void UpdateStatusNameInTable(DataRowView rowView, int newStatusId)
        {
            if (_statusesTable != null)
            {
                DataRow[] foundRows = _statusesTable.Select($"room_status_id = {newStatusId}");
                if (foundRows.Length > 0)
                {
                    // Обновляем только имя статуса, чтобы интерфейс отрисовал новое значение
                    rowView["room_status"] = foundRows[0]["room_status"].ToString();
                }
            }
        }
    }
}