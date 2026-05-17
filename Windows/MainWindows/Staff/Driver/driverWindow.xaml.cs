using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Staff.Driver
{
    /// <summary>
    /// Логика взаимодействия для driverWindow.xaml
    /// </summary>
    public partial class driverWindow : Window
    {
        private readonly int _userId;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public driverWindow()
        {
            InitializeComponent();
        }

        public driverWindow(int userId) : this()
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

                    // Получаем все заказы на трансфер (service_id = 2), которые еще не выполнены
                    string query = @"
                        SELECT
                            os.guest_id,
                            u.surname as surname,
                            u.name,
                            u.patronymic
                        FROM Ordered_services os
                        JOIN Guests g ON os.guest_id = g.guest_id
                        join Users u on g.user_id = u.user_id
                        WHERE os.service_id = 2
                          AND (os.is_done = 0 OR os.is_done IS NULL)";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int guestId = reader.GetInt32(reader.GetOrdinal("guest_id"));
                            string lastName = reader.IsDBNull(reader.GetOrdinal("surname")) ? "" : reader.GetString(reader.GetOrdinal("surname"));
                            string firstName = reader.IsDBNull(reader.GetOrdinal("name")) ? "" : reader.GetString(reader.GetOrdinal("name"));
                            string middleName = reader.IsDBNull(reader.GetOrdinal("patronymic")) ? "" : reader.GetString(reader.GetOrdinal("patronymic"));

                            string fullName = $"{lastName} {firstName} {middleName}".Trim();

                            orders.Add(new OrderViewModel
                            {
                                GuestId = guestId,
                                GuestFullName = fullName
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

                    // Обновляем статус услуги на выполненный
                    string updateServicesQuery = @"
                        UPDATE Ordered_services
                        SET is_done = 1
                        WHERE guest_id = @guestId AND service_id = 2
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
        public string GuestFullName { get; set; }
    }
}