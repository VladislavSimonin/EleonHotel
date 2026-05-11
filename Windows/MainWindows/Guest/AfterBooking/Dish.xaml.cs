using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EleonHotel.Windows.MainWindows.Guest.AfterBooking
{
    /// <summary>
    /// Логика взаимодействия для Dish.xaml
    /// </summary>
    public partial class Dish : Window
    {
        private readonly int _userId;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private List<DishInfo> _dishesList;

        public Dish(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadDishes();
            CbCutlery.SelectedIndex = 0;
        }

        private void LoadDishes()
        {
            _dishesList = new List<DishInfo>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string dishesQuery = @"
                    SELECT 
                        dish_id,
                        dish_name,
                        dish_composition,
                        cost
                    FROM Restaurant_menu
                    ORDER BY dish_name";

                using (var cmd = new SqlCommand(dishesQuery, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int dish_id = reader.GetInt32(reader.GetOrdinal("dish_id"));
                        string dish_name = reader["dish_name"].ToString();
                        string dish_composition = reader["dish_composition"].ToString();
                        decimal cost = reader.GetDecimal(reader.GetOrdinal("cost"));

                        // Определяем путь к изображению 
                        string imagePath = $"/Data/Food/dish_{dish_id}.jpg";

                        _dishesList.Add(new DishInfo
                        {
                            dish_id = dish_id,
                            dish_name = dish_name,
                            dish_composition = dish_composition,
                            cost = cost,
                            ImagePath = imagePath
                        });
                    }
                }
            }

            DishesItemsControl.ItemsSource = _dishesList;
        }

        private void CbCutlery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора количества столовых приборов (пока пусто)
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            // Обработчик кнопки "Заказать" будет реализован позже
            // Здесь нужно собрать все выбранные блюда с количеством и столовые приборы
        }
    }

    public class DishInfo
    {
        public int dish_id { get; set; }
        public string dish_name { get; set; }
        public string dish_composition { get; set; }
        public decimal cost { get; set; }
        public string ImagePath { get; set; }
    }
}
