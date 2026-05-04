using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows
{
    public partial class GuestWindow : Window
    {

        private string ConnectionString = Properties.Settings.Default.ConnectionString;
        public int UserId { get; private set; }
        private Dictionary<int, List<string>> roomImages;
        private int currentCategoryIndex;

        // Словарь для хранения ID категорий номеров
        private readonly Dictionary<string, int> categoryIds = new Dictionary<string, int>
        {
            { "Econom", 1 },
            { "Standart", 2 },
            { "Comfort", 3 },
            { "Family", 4 },
            { "Business", 5 },
            { "Lux", 6 }
        };


        public GuestWindow(int userId)
        {
            InitializeComponent();
            UserId = userId;
            InitializeComboBox();
            LoadAllRoomData();
        }

        private void InitializeComboBox()
        {
            for (int i = 1; i <= 6; i++)
            {
                CbGuests.Items.Add(i);
            }
            CbGuests.SelectedIndex = 0;
        }

        private void LoadAllRoomData()
        {
            // Загружаем данные для всех категорий номеров
            // Используем оператор ! для указания компилятору, что элементы уже инициализированы через InitializeComponent()
            LoadRoomData("Econom", 1, TblEconomDesc!, TblEconomFacilities!, TblEconomCost!);
            LoadRoomData("Standart", 2, TblStandartDesc!, TblStandartFacilities!, TblStandartCost!);
            LoadRoomData("Comfort", 3, TblComfortDesc!, TblComfortFacilities!, TblComfortCost!);
            LoadRoomData("Family", 4, TblFamilyDesc!, TblFamilyFacilities!, TblFamilyCost!);
            LoadRoomData("Business", 5, TblBusinessDesc!, TblBusinessFacilities!, TblBusinessCost!);
            LoadRoomData("Lux", 6, TblLuxDesc!, TblLuxFacilities!, TblLuxCost!);

            // Инициализация списков изображений для всех категорий
            InitializeAllRoomImages();
        }

        private void LoadRoomData(string categoryName, int categoryId, TextBlock descBlock, TextBlock facilitiesBlock, TextBlock costBlock)
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
                WHERE r2.room_category_id = @CategoryId
            ) AS uf
            ORDER BY uf.room_facility_name
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS facilities
    FROM Room_categories rc
    WHERE rc.room_category_id = @CategoryId;";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        // Явно указываем тип параметра
                        cmd.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int).Value = categoryId;
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Обработка описания
                                object descObj = reader["description"];
                                descBlock.Text = (descObj != DBNull.Value) ? descObj.ToString() : "Нет описания";

                                // Обработка цены
                                object costObj = reader["cost"];
                                if (costObj != DBNull.Value && decimal.TryParse(costObj.ToString(), out decimal cost))
                                {
                                    costBlock.Text = $"{cost:#,##0} ₽";
                                }
                                else
                                {
                                    costBlock.Text = "Цена не указана";
                                }

                                // Обработка удобств
                                object facilitiesObj = reader["facilities"];
                                facilitiesBlock.Text = (facilitiesObj != DBNull.Value) ? facilitiesObj.ToString() : "Нет удобств";
                            }
                            else
                            {
                                // Если данных нет
                                descBlock.Text = "Информация о категории не найдена";
                                facilitiesBlock.Text = string.Empty;
                                costBlock.Text = "Цена не указана";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для {categoryName}: {ex.Message}\n\nДетали: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeAllRoomImages()
        {
            // Чтобы даты отображались с завтрашнего дня
            DtEnter.DisplayDateStart = DateTime.Today.AddDays(1);
            DtOut.DisplayDateStart = DateTime.Today.AddDays(1);

            roomImages = new Dictionary<int, List<string>>
            {
                { 1, new List<string> // Econom
                {
                    "pack://application:,,,/Data/Images/Econom_1.jpg",
                    "pack://application:,,,/Data/Images/Econom_2.jpg",
                    "pack://application:,,,/Data/Images/Econom_3.jpg",
                    "pack://application:,,,/Data/Images/Econom_4.jpg",
                    "pack://application:,,,/Data/Images/Econom_5.jpg"
                }},
                { 2, new List<string> // Standart
                {
                    "pack://application:,,,/Data/Images/Standart_1.jpg",
                    "pack://application:,,,/Data/Images/Standart_2.jpg",
                    "pack://application:,,,/Data/Images/Standart_3.jpg",
                    "pack://application:,,,/Data/Images/Standart_4.jpg",
                    "pack://application:,,,/Data/Images/Standart_5.jpg"
                }},
                { 3, new List<string> // Comfort
                {
                    "pack://application:,,,/Data/Images/Comfort_1.jpg",
                    "pack://application:,,,/Data/Images/Comfort_2.jpg",
                    "pack://application:,,,/Data/Images/Comfort_3.jpg",
                    "pack://application:,,,/Data/Images/Comfort_4.jpg",
                    "pack://application:,,,/Data/Images/Comfort_5.jpg",
                    "pack://application:,,,/Data/Images/Comfort_6.jpg",
                    "pack://application:,,,/Data/Images/Comfort_7.jpg",
                    "pack://application:,,,/Data/Images/Comfort_8.jpg",
                    "pack://application:,,,/Data/Images/Comfort_9.jpg"
                }},
                { 4, new List<string> // Family
                {
                    "pack://application:,,,/Data/Images/Family_1.jpg",
                    "pack://application:,,,/Data/Images/Family_2.jpg",
                    "pack://application:,,,/Data/Images/Family_3.jpg",
                    "pack://application:,,,/Data/Images/Family_4.jpg",
                    "pack://application:,,,/Data/Images/Family_5.jpg",
                    "pack://application:,,,/Data/Images/Family_6.jpg",
                    "pack://application:,,,/Data/Images/Family_7.jpg"
                }},
                { 5, new List<string> // Business
                {
                    "pack://application:,,,/Data/Images/Business_1.jpg",
                    "pack://application:,,,/Data/Images/Business_2.jpg",
                    "pack://application:,,,/Data/Images/Business_3.jpg",
                    "pack://application:,,,/Data/Images/Business_4.jpg",
                    "pack://application:,,,/Data/Images/Business_5.jpg",
                    "pack://application:,,,/Data/Images/Business_6.jpg",
                    "pack://application:,,,/Data/Images/Business_7.jpg",
                    "pack://application:,,,/Data/Images/Business_8.jpg",
                    "pack://application:,,,/Data/Images/Business_9.jpg",
                    "pack://application:,,,/Data/Images/Business_10.jpg"
                }},
                { 6, new List<string> // Lux
                {
                    "pack://application:,,,/Data/Images/Lux_1.jpg",
                    "pack://application:,,,/Data/Images/Lux_2.jpg",
                    "pack://application:,,,/Data/Images/Lux_3.jpg",
                    "pack://application:,,,/Data/Images/Lux_4.jpg",
                    "pack://application:,,,/Data/Images/Lux_5.jpg",
                    "pack://application:,,,/Data/Images/Lux_6.jpg",
                    "pack://application:,,,/Data/Images/Lux_7.jpg",
                    "pack://application:,,,/Data/Images/Lux_8.jpg",
                    "pack://application:,,,/Data/Images/Lux_9.jpg"
                }}
            };
        }

        private void OpenImageGallery(int categoryId, string categoryName)
        {
            if (roomImages.ContainsKey(categoryId) && roomImages[categoryId].Count > 0)
            {
                var galleryWindow = new ImageGalleryWindow(roomImages[categoryId], categoryName);
                galleryWindow.Owner = this;
                galleryWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Нет доступных изображений", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEconomImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageGallery(1, "Эконом");
        }

        private void BtnStandartImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageGallery(2, "Стандарт");
        }

        private void BtnComfortImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageGallery(3, "Комфорт");
        }

        private void BtnFamilyImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageGallery(4, "Семейный");
        }

        private void BtnBusinessImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageGallery(5, "Бизнес");
        }

        private void BtnLuxImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageGallery(6, "Люкс");
        }


        private void CbGuests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Получаем выбранное количество гостей
            if (CbGuests.SelectedItem == null || !int.TryParse(CbGuests.SelectedItem.ToString(), out int guestsCount))
                return;

            // Эконом: макс. 2 гостя
            if (guestsCount > 2)
            {
                BtnEconomBooking.IsEnabled = false;
                TbEconomBlock.Visibility = Visibility.Visible;
            }
            else
            {
                BtnEconomBooking.IsEnabled = true;
                TbEconomBlock.Visibility = Visibility.Collapsed;
            }

            // Стандарт: макс. 2 гостя
            if (guestsCount > 2)
            {
                BtnStandartBooking.IsEnabled = false;
                TbStandartBlock.Visibility = Visibility.Visible;
            }
            else
            {
                BtnStandartBooking.IsEnabled = true;
                TbStandartBlock.Visibility = Visibility.Collapsed;
            }

            // Комфорт: макс. 4 гостя
            if (guestsCount > 4)
            {
                BtnComfortBooking.IsEnabled = false;
                TbComfortBlock.Visibility = Visibility.Visible;
            }
            else
            {
                BtnComfortBooking.IsEnabled = true;
                TbComfortBlock.Visibility = Visibility.Collapsed;
            }

            // Семейный и Бизнес: макс. 5 гостей
            if (guestsCount > 5)
            {
                BtnFamilyBooking.IsEnabled = false;
                TbFamilyBlock.Visibility = Visibility.Visible;
                BtnBusinessBooking.IsEnabled = false;
                TbBusinessBlock.Visibility = Visibility.Visible;
            }
            else
            {
                BtnFamilyBooking.IsEnabled = true;
                TbFamilyBlock.Visibility = Visibility.Collapsed;
                BtnBusinessBooking.IsEnabled = true;
                TbBusinessBlock.Visibility = Visibility.Collapsed;
            }

            // Люкс: макс. 6 гостей
            if (guestsCount > 6)
            {
                BtnLuxBooking.IsEnabled = false;
                TbLuxBlock.Visibility = Visibility.Visible;
            }
            else
            {
                BtnLuxBooking.IsEnabled = true;
                TbLuxBlock.Visibility = Visibility.Collapsed;

            }

        }

        // Бронирование номеров
        private void BtnEconomBooking_Click(object sender, RoutedEventArgs e)
        {
            BookRoom(1, "Эконом");
        }

        private void BtnStandartBooking_Click(object sender, RoutedEventArgs e)
        {
            BookRoom(2, "Стандарт");
        }

        private void BtnComfortBooking_Click(object sender, RoutedEventArgs e)
        {
            BookRoom(3, "Комфорт");
        }

        private void BtnFamilyBooking_Click(object sender, RoutedEventArgs e)
        {
            BookRoom(4, "Семейный");
        }

        private void BtnBusinessBooking_Click(object sender, RoutedEventArgs e)
        {
            BookRoom(5, "Бизнес");
        }

        private void BtnLuxBooking_Click(object sender, RoutedEventArgs e)
        {
            BookRoom(6, "Люкс");
        }

        private void BookRoom(int categoryId, string categoryName)
        {
            int guestsCount = CbGuests.SelectedItem != null ? (int)CbGuests.SelectedItem : 1;
            
            MessageBox.Show(
                $"Вы выбрали бронирование номера категории \"{categoryName}\".\n" +
                $"Количество гостей: {guestsCount}\n\n" +
                $"В ближайшее время будет подключена оплата через ЮKassa.",
                "Бронирование номера",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
