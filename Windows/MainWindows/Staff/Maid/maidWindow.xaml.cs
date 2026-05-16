using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Staff.Maid
{
    /// <summary>
    /// Логика взаимодействия для maidWindow.xaml
    /// </summary>
    public partial class maidWindow : Window
    {
        private readonly int _userId;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public maidWindow()
        {
            InitializeComponent();
        }

        public maidWindow(int userId) : this()
        {
            _userId = userId;
            LoadOrders();
        }

        private async void LoadOrders()
        {
            var orders = await GetPendingOrdersAsync();

            if (orders.Count == 0)
            {
                NoOrdersTextBlock.Visibility = Visibility.Visible;
                OrdersItemsControl.ItemsSource = null;
            }
            else
            {
                NoOrdersTextBlock.Visibility = Visibility.Hidden;
                OrdersItemsControl.ItemsSource = orders;
            }
        }

        private async Task<List<OrderViewModel>> GetPendingOrdersAsync()
        {
            return await Task.Run(() =>
            {
                var orders = new List<OrderViewModel>();

                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Получаем все заказы услуг (service_id = 1 - уборка, service_id = 3 - прачечная), которые еще не выполнены
                    string query = @"
                        SELECT
                            os.guest_id,
                            r.number AS room_number,
                            asrv.service_name
                        FROM Ordered_services os
                        JOIN Additional_services asrv ON os.service_id = asrv.service_id
                        JOIN Guests g ON os.guest_id = g.guest_id
                        LEFT JOIN Rooms r ON g.room_id = r.room_id
                        WHERE (os.service_id = 1 OR os.service_id = 3)
                          AND (os.is_done = 0 OR os.is_done IS NULL)
                        ORDER BY r.number, asrv.service_name";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int guestId = reader.GetInt32(reader.GetOrdinal("guest_id"));
                            int roomNumber = reader.IsDBNull(reader.GetOrdinal("room_number")) ? 0 : reader.GetInt32(reader.GetOrdinal("room_number"));
                            string serviceName = reader.GetString(reader.GetOrdinal("service_name"));

                            orders.Add(new OrderViewModel
                            {
                                GuestId = guestId,
                                RoomNumber = roomNumber,
                                ServiceName = serviceName
                            });
                        }
                    }
                }

                return orders;
            });
        }

        private async void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int guestId)
            {
                try
                {
                    await MarkOrderAsDoneAsync(guestId);
                    MessageBox.Show("Услуга отмечена выполненной!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders(); // Перезагружаем список заказов
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task MarkOrderAsDoneAsync(int guestId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    // Обновляем статус услуги на выполненный и увеличиваем счетчик
                    string updateServicesQuery = @"
                        UPDATE Ordered_services
                        SET is_done = 1,
                            ordered_services_count = ISNULL(ordered_services_count, 0) + 1
                        WHERE guest_id = @guestId AND (service_id = 1 OR service_id = 3)
                          AND (is_done = 0 OR is_done IS NULL)";

                    using (var cmd = new SqlCommand(updateServicesQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@guestId", guestId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса услуги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // ViewModel для отображения заказа
    public class OrderViewModel
    {
        public int GuestId { get; set; }
        public int RoomNumber { get; set; }
        public string ServiceName { get; set; }
    }
}
