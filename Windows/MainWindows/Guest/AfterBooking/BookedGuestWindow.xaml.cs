using EleonHotel.Windows.MainWindows.Guest.AfterBooking;
using EleonHotel.Windows.MainWindows.Guest.AfterBooking.Payments;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EleonHotel.Windows.MainWindows.Guest
{
    /// <summary>
    /// Логика взаимодействия для BookedGuestWindow.xaml
    /// </summary>
    public partial class BookedGuestWindow : Window
    {
        private readonly int _userId;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public BookedGuestWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void aboutMe_Click(object sender, RoutedEventArgs e)
        {
            var aboutMeWindow = new aboutMe(_userId);
            aboutMeWindow.ShowDialog();
        }

        private void orderDish_Click(object sender, RoutedEventArgs e)
        {
            var dishWindow = new Dish(_userId);
            dishWindow.ShowDialog();
        }

        private void orderLaundry_Click(object sender, RoutedEventArgs e)
        {
            OpenServicePaymentWindow(3, "Прачечная");
        }

        private void orderCleaning_Click(object sender, RoutedEventArgs e)
        {
            OpenServicePaymentWindow(1, "Уборка");
        }

        private void orderTransfer_Click(object sender, RoutedEventArgs e)
        {
            OpenServicePaymentWindow(2, "Трансфер");
        }

        private void inviteDoctor_Click(object sender, RoutedEventArgs e)
        {
            OpenServicePaymentWindow(5, "Вызов врача");
        }

        private void OpenServicePaymentWindow(int serviceId, string serviceName)
        {
            try
            {
                decimal serviceCost = 0;
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT cost FROM Additional_services WHERE service_id = @serviceId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@serviceId", serviceId);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            serviceCost = (decimal)result;
                        }
                    }
                }

                Window paymentWindow = serviceId switch
                {
                    1 => new LaundryPayment(_userId, serviceId, serviceName, serviceCost),
                    2 => new CleaningPayment(_userId, serviceId, serviceName, serviceCost),
                    3 => new TransferPayment(_userId, serviceId, serviceName, serviceCost),
                    5 => new DoctorPayment(_userId, serviceId, serviceName, serviceCost),
                    _ => null
                };

                if (paymentWindow != null)
                {
                    paymentWindow.Owner = this;
                    paymentWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении информации об услуге: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}