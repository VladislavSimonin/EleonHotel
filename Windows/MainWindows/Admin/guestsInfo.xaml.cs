using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SqlClient;
using System.Data;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для guestsInfo.xaml
    /// </summary>
    public partial class guestsInfo : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public guestsInfo()
        {
            InitializeComponent();
            LoadGuestsData();
        }

        private void LoadGuestsData()
        {
            string query = @"
                SELECT
                    u.user_id,
                    u.surname,
                    u.name,
                    u.patronymic,
                    u.passport_series,
                    u.passport_number,
                    u.who_gave_passport,
                    u.when_gave_passport,
                    u.registration_address,
                    u.phone,
                    u.email,
                    u.birthsday,
                    r.number AS room_number,
                    ISNULL(STUFF((
                        SELECT ', ' + rm.dish_name
                        FROM Ordered_dishes od
                        INNER JOIN Restaurant_menu rm ON od.dish_id = rm.dish_id
                        WHERE od.guest_id = g.guest_id
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), 'Нет') AS ordered_dishes,
                    ISNULL(STUFF((
                        SELECT ', ' + asrv.service_name
                        FROM Ordered_services os
                        INNER JOIN Additional_services asrv ON os.service_id = asrv.service_id
                        WHERE os.guest_id = g.guest_id
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), 'Нет') AS ordered_services
                FROM Users u
                INNER JOIN Guests g ON u.user_id = g.user_id
                LEFT JOIN Rooms r ON g.room_id = r.room_id
                ORDER BY u.surname";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        GuestsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
