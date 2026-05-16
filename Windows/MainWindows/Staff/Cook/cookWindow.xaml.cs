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
                        JOIN Restaurant_menu d ON od.dish_id = d.dish_id
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

                        foreach (var kvp in dishesByGuestAndDish)
                        {
                            int guestId = kvp.Key.guestId;

                            if (!dishesByGuest.ContainsKey(guestId))
                            {
                                dishesByGuest[guestId] = new List<DishViewModel>();
                                orderedDishIdsByGuest[guestId] = new List<int>();
                            }

                            dishesByGuest[guestId].Add(kvp.Value);
                            orderedDishIdsByGuest[guestId].AddRange(orderedDishIdsByGuestAndDish[kvp.Key]);
                        }

                        foreach (var kvp in dishesByGuest)
                        {
                            orders.Add(new OrderViewModel
                            {
                                GuestId = kvp.Key,
                                RoomNumber = roomByGuest[kvp.Key],
                                Dishes = kvp.Value,
                                OrderedDishIds = orderedDishIdsByGuest[kvp.Key]
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
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    // Используем транзакцию для атомарности операций
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Обновляем статус блюд на доставленные
                            string updateDishesQuery = @"
                        UPDATE Ordered_dishes
                        SET is_delivered = 1
                        WHERE guest_id = @guestId AND (is_delivered = 0 OR is_delivered IS NULL)";

                            using (var cmdDishes = new SqlCommand(updateDishesQuery, conn, transaction))
                            {
                                cmdDishes.Parameters.AddWithValue("@guestId", guestId);
                                await cmdDishes.ExecuteNonQueryAsync();
                            }

                            // 2. Обновляем статус услуги доставки и увеличиваем счетчик
                            string updateServicesQuery = @"
                        UPDATE Ordered_services
                        SET is_done = 1,
                            ordered_services_count = ISNULL(ordered_services_count, 0) + 1
                        WHERE guest_id = @guestId AND service_id = 4";

                            using (var cmdServices = new SqlCommand(updateServicesQuery, conn, transaction))
                            {
                                cmdServices.Parameters.AddWithValue("@guestId", guestId);
                                await cmdServices.ExecuteNonQueryAsync();
                            }

                            // Подтверждаем транзакцию
                            transaction.Commit();
                        }
                        catch
                        {
                            // Откатываем транзакцию в случае ошибки
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса доставки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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