using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SqlClient;

namespace EleonHotel.Windows.MainWindows
{
    public partial class GuestWindow : Window
    {
        private const string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        public int UserId { get; private set; }
        private List<string> currentRoomImages;

        public GuestWindow(int userId)
        {
            InitializeComponent();
            UserId = userId;
            InitializeComboBox();
            LoadRoomDescription();
        }

        private void InitializeComboBox()
        {
            for (int i = 1; i <= 6; i++)
            {
                CbGuests.Items.Add(i);
            }
            CbGuests.SelectedIndex = 0;
        }

        private void LoadRoomDescription()
        {
            const string query = @"
        SELECT 
        rc.description,
        rc.cost,
        STUFF((
            SELECT ', ' + uf.room_facility_name
            FROM (
                SELECT DISTINCT fl.room_facility_name
                FROM Rooms r2
                JOIN Room_facilities rf2 ON r2.room_id = rf2.room_id
                JOIN Facilities_list fl ON rf2.room_facility_id = fl.room_facility_id
                WHERE r2.room_category_id = rc.room_category_id
            ) AS uf
            ORDER BY uf.room_facility_name
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS facilities
    FROM Room_categories rc
    WHERE rc.room_category_id = 1;";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            TblEconomDesc.Text = reader["description"]?.ToString() ?? "Нет описания";

                            var costVal = reader["cost"];
                            TblEconomCost.Text = costVal != DBNull.Value ?
                                $"{Convert.ToDecimal(costVal):#,##0} ₽" : "Цена не указана";

                            TblEconomFacilities.Text = reader["facilities"]?.ToString() ?? "Нет удобств";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Инициализация списка изображений
            InitializeRoomImages();
        }

        private void InitializeRoomImages()
        {
            currentRoomImages = new List<string>
            {
                "pack://application:,,,/Data/Images/Econom_1.jpg",
                "pack://application:,,,/Data/Images/Econom_2.jpg",
                "pack://application:,,,/Data/Images/Econom_3.jpg",
                "pack://application:,,,/Data/Images/Econom_4.jpg",
                "pack://application:,,,/Data/Images/Econom_5.jpg"
            };
        }

        private void BtnEconomImage_Click(object sender, RoutedEventArgs e)
        {
            if (currentRoomImages != null && currentRoomImages.Count > 0)
            {
                var galleryWindow = new ImageGalleryWindow(currentRoomImages, "Эконом");
                galleryWindow.Owner = this;
                galleryWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Нет доступных изображений", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
