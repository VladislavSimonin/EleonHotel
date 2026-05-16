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
                            od.ordered_dish_id,
                            od.guest_id,
                            r.number AS room_number,
                            d.dish_id AS dish_id,
                            d.dish_name,
                            od.ordered_dishes_count
                        FROM Ordered_dishes od
                        JOIN Dishes d ON od.dish_id = d.dish_id
                        JOIN Guests g ON od.guest_id = g.guest_id
                        LEFT JOIN Rooms r ON g.room_id = r.room_id
                        WHERE od.is_delivered = 0 OR od.is_delivered IS NULL
                        ORDER BY r.number, d.dish_name";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Группируем по guest_id и dish_id для суммирования количества одинаковых блюд
                        var dishesByGuestAndDish = new Dictionary<(int guestId, int dishId), DishViewModel>();
                        var orderedDishIdsByGuestAndDish = new Dictionary<(int guestId, int dishId), List<int>>();
                        var roomByGuest = new Dictionary<int, int>();

                        while (reader.Read())
                        {
                            int orderedDishId = reader.GetInt32(reader.GetOrdinal("ordered_dish_id"));
                            int guestId = reader.GetInt32(reader.GetOrdinal("guest_id"));
                            int dishId = reader.GetInt32(reader.GetOrdinal("dish_id"));
                            int roomNumber = reader.IsDBNull(reader.GetOrdinal("room_number")) ? 0 : reader.GetInt32(reader.GetOrdinal("room_number"));
                            string dishName = reader.GetString(reader.GetOrdinal("dish_name"));
                            int quantity = reader.IsDBNull(reader.GetOrdinal("ordered_dishes_count")) ? 0 : reader.GetInt32(reader.GetOrdinal("ordered_dishes_count"));

                            if (!roomByGuest.ContainsKey(guestId))
                            {
                                roomByGuest[guestId] = roomNumber;
                            }

                            var key = (guestId, dishId);

                            if (!dishesByGuestAndDish.ContainsKey(key))
                            {
                                dishesByGuestAndDish[key] = new DishViewModel
                                {
                                    DishId = dishId,
                                    DishName = dishName,
                                    Quantity = 0
                                };
                                orderedDishIdsByGuestAndDish[key] = new List<int>();
                            }

                            dishesByGuestAndDish[key].Quantity += quantity;
                            orderedDishIdsByGuestAndDish[key].Add(orderedDishId);
                        }

                        // Группируем по guest_id для создания OrderViewModel
                        var dishesByGuest = new Dictionary<int, List<DishViewModel>>();
                        var orderedDishIdsByGuest = new Dictionary<int, List<int>>();

                    // Группируем по guest_id (комнате)
                    var groupedOrders = pendingDishes
                        .GroupBy(od => od.GuestId)
                        .Select(g => new OrderViewModel
                        {
                            GuestId = g.Key,
                            RoomNumber = g.First().Guest.Room != null ? g.First().Guest.Room.Number : 0,
                            Dishes = g.Select(od => new DishViewModel
                            {
                                DishId = od.DishId,
                                DishName = od.Dish.DishName,
                                Quantity = od.OrderedDishesCount ?? 0
                            }).ToList()
                        })
                        .ToList();

                    return groupedOrders;
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
                    // Находим все заказанные блюда этого гостя, которые еще не доставлены
                    var pendingDishes = context.OrderedDishes
                        .Where(od => od.GuestId == guestId && (od.IsDelivered == false || od.IsDelivered == null))
                        .ToList();

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
        public List<int> OrderedDishIds { get; set; } = new List<int>();
    }

    // ViewModel для отображения блюда
    public class DishViewModel
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public int Quantity { get; set; }
    }
}