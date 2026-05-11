using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EleonHotel.Data;
using EleonHotel.Models;
using Microsoft.EntityFrameworkCore;

namespace EleonHotel.Windows.MainWindows.Guest.AfterBooking
{
    /// <summary>
    /// Логика взаимодействия для aboutMe.xaml
    /// </summary>
    public partial class aboutMe : Window
    {
        private readonly int _userId;

        public aboutMe(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadGuestData();
        }

        private void LoadGuestData()
        {
            using (var db = new HotelDbContext())
            {
                // Получаем информацию о госте по userId с подгрузкой связанных данных
                var guest = db.Guests
                    .Include(g => g.Room)
                        .ThenInclude(r => r.RoomCategory)
                    .Include(g => g.Services)
                    .FirstOrDefault(g => g.UserId == _userId);

                if (guest != null)
                {
                    // Номер комнаты
                    RoomNumberText.Text = guest.Room?.Number.ToString() ?? "Не назначен";
                    
                    // Категория номера
                    RoomCategoryText.Text = guest.Room?.RoomCategory?.RoomCategoryName ?? "Нет данных";
                    
                    // Описание категории
                    RoomDescriptionText.Text = guest.Room?.RoomCategory?.Description ?? "Нет данных";

                    // Получаем дополнительные услуги с количеством заказов
                    var servicesList = new List<ServiceInfo>();
                    if (guest.Services.Any())
                    {
                        servicesList = guest.Services
                            .GroupBy(s => s.ServiceName)
                            .Select(g => new ServiceInfo
                            {
                                ServiceName = g.Key,
                                OrderCount = g.Count()
                            })
                            .ToList();
                    }

                    ServicesDataGrid.ItemsSource = servicesList;

                    // Получаем итоговый счет из Payment_invoices
                    var paymentInvoice = db.PaymentInvoices
                        .Where(p => p.GuestId == guest.GuestId)
                        .Select(p => p.Total)
                        .FirstOrDefault();

                    TotalAmountText.Text = paymentInvoice.ToString("0.00") + " ₽";
                }
                else
                {
                    RoomNumberText.Text = "Гость не найден";
                    RoomCategoryText.Text = "";
                    RoomDescriptionText.Text = "";
                    ServicesDataGrid.ItemsSource = new List<ServiceInfo>();
                    TotalAmountText.Text = "0.00 ₽";
                }
            }
        }
    }

    public class ServiceInfo
    {
        public string ServiceName { get; set; }
        public int OrderCount { get; set; }
    }
}
