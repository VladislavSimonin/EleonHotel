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
using System.Data.SqlClient;
using System.Data;

namespace EleonHotel.Windows.MainWindows.Guest.AfterBooking
{
    /// <summary>
    /// Логика взаимодействия для aboutMe.xaml
    /// </summary>
    public partial class aboutMe : Window
    {
        private readonly int _userId;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public aboutMe(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadGuestData();
        }

        private void LoadGuestData()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Получаем информацию о госте
                string guestQuery = @"
                    SELECT
                        g.guest_id,
                        r.number AS room_number,
                        rc.room_category_name,
                        rc.description
                    FROM Guests g
                    LEFT JOIN Rooms r ON g.room_id = r.room_id
                    LEFT JOIN Room_categories rc ON r.room_category_id = rc.room_category_id
                    WHERE g.user_id = @userId";

                int guestId = 0;
                string roomNumber = "Не назначен";
                string roomCategory = "Нет данных";
                string roomDescription = "Нет данных";

                using (var cmd = new SqlCommand(guestQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            guestId = reader.GetInt32(reader.GetOrdinal("guest_id"));
                            roomNumber = reader.IsDBNull(reader.GetOrdinal("room_number"))
                                ? "Не назначен"
                                : reader["room_number"].ToString();
                            roomCategory = reader.IsDBNull(reader.GetOrdinal("room_category_name"))
                                ? "Нет данных"
                                : reader["room_category_name"].ToString();
                            roomDescription = reader.IsDBNull(reader.GetOrdinal("description"))
                                ? "Нет данных"
                                : reader["description"].ToString();
                        }
                    }
                }

                RoomNumberText.Text = roomNumber;
                RoomCategoryText.Text = roomCategory;
                RoomDescriptionText.Text = roomDescription;

                // Получаем дополнительные услуги с количеством заказов
                var servicesList = new List<ServiceInfo>();
                if (guestId > 0)
                {
                    string servicesQuery = @"
                        SELECT
                            asrv.service_name,
                            COUNT(*) AS order_count
                        FROM Ordered_services os
                        INNER JOIN Additional_services asrv ON os.service_id = asrv.service_id
                        WHERE os.guest_id = @guestId
                        GROUP BY asrv.service_name";

                    using (var cmd = new SqlCommand(servicesQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@guestId", guestId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                servicesList.Add(new ServiceInfo
                                {
                                    ServiceName = reader["service_name"].ToString(),
                                    OrderCount = reader.GetInt32(reader.GetOrdinal("order_count"))
                                });
                            }
                        }
                    }
                }

                ServicesDataGrid.ItemsSource = servicesList;

                // Получаем итоговый счет из Payment_invoices
                decimal totalAmount = 0;
                if (guestId > 0)
                {
                    string paymentQuery = @"
                        SELECT ISNULL(SUM(total), 0)
                        FROM Payment_invoices
                        WHERE guest_id = @guestId";

                    using (var cmd = new SqlCommand(paymentQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@guestId", guestId);
                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            totalAmount = Convert.ToDecimal(result);
                        }
                    }
                }

                TotalAmountText.Text = totalAmount.ToString("0.00") + " ₽";

                if (guestId == 0)
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