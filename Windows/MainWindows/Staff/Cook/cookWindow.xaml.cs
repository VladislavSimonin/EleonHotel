using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Staff.Cook
{
    /// <summary>
    /// Логика взаимодействия для cookWindow.xaml
    /// </summary>
    public partial class cookWindow : Window
    {
        private readonly int _userId;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public cookWindow()
        {
            InitializeComponent();
        }

        public cookWindow(int userId) : this()
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

                    // Получаем все заказы блюд, которые еще не доставлены
                    string query = @"
                        SELECT
                            od.guest_id,
                            r.number AS room_number,
                            rm.dish_name,
                            od.ordered_dishes_count
                        FROM Ordered_dishes od
                        JOIN Restaurant_menu rm ON od.dish_id = rm.dish_id
                        JOIN Guests g ON od.guest_id = g.guest_id
                        LEFT JOIN Rooms r ON g.room_id = r.room_id
                        WHERE od.is_delivered = 0 OR od.is_delivered IS NULL
                        ORDER BY r.number, rm.dish_name";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var dishesByGuest = new Dictionary<int, List<DishViewModel>>();
                        var roomByGuest = new Dictionary<int, int>();

                        while (reader.Read())
                        {
                            int guestId = reader.GetInt32(reader.GetOrdinal("guest_id"));
                            int roomNumber = reader.IsDBNull(reader.GetOrdinal("room_number")) ? 0 : reader.GetInt32(reader.GetOrdinal("room_number"));
                            string dishName = reader.GetString(reader.GetOrdinal("dish_name"));
                            int quantity = reader.IsDBNull(reader.GetOrdinal("ordered_dishes_count")) ? 0 : reader.GetInt32(reader.GetOrdinal("ordered_dishes_count"));

                            if (!dishesByGuest.ContainsKey(guestId))
                            {
                                dishesByGuest[guestId] = new List<DishViewModel>();
                                roomByGuest[guestId] = roomNumber;
                            }

                            dishesByGuest[guestId].Add(new DishViewModel
                            {
                                DishId = 0, // Не используется в отображении
                                DishName = dishName,
                                Quantity = quantity
                            });
                        }

                        foreach (var kvp in dishesByGuest)
                        {
                            orders.Add(new OrderViewModel
                            {
                                GuestId = kvp.Key,
                                RoomNumber = roomByGuest[kvp.Key],
                                Dishes = kvp.Value
                            });
                        }
                    }
                }

                return orders;
            });
        }

        private async void BtnDelivered_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int guestId)
            {
                try
                {
                    await MarkOrderAsDeliveredAsync(guestId);
                    MessageBox.Show("Заказ отмечен как доставленный!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders(); // Перезагружаем список заказов
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task MarkOrderAsDeliveredAsync(int guestId)
        {
            await Task.Run(() =>
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string updateQuery = @"
                        UPDATE Ordered_dishes
                        SET is_delivered = 1
                        WHERE guest_id = @guest_id AND (is_delivered = 0 OR is_delivered IS NULL)

                        UPDATE Ordered_services
                        SET is_done = 1
                        WHERE guest_id = @guest_id AND (is_done = 0 OR is_done IS NULL)";

                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@guest_id", guestId);
                        cmd.ExecuteNonQuery();
                    }
                }
            });
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
        public List<DishViewModel> Dishes { get; set; } = new List<DishViewModel>();
    }

    // ViewModel для отображения блюда
    public class DishViewModel
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public int Quantity { get; set; }
    }
}