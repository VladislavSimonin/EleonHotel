using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EleonHotel.Models;
using Microsoft.EntityFrameworkCore;

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
                using (var context = new HotelDbContext())
                {
                    // Получаем все заказы блюд, которые еще не доставлены
                    var pendingDishes = context.OrderedDishes
                        .Include(od => od.Dish)
                        .Include(od => od.Guest)
                            .ThenInclude(g => g.Room)
                        .Where(od => od.IsDelivered == false || od.IsDelivered == null)
                        .ToList();

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
                using (var context = new HotelDbContext())
                {
                    // Находим все заказанные блюда этого гостя, которые еще не доставлены
                    var pendingDishes = context.OrderedDishes
                        .Where(od => od.GuestId == guestId && (od.IsDelivered == false || od.IsDelivered == null))
                        .ToList();

                    foreach (var dish in pendingDishes)
                    {
                        dish.IsDelivered = true;
                    }

                    context.SaveChanges();
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
