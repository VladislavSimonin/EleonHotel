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
                        int dishId = reader.GetInt32(reader.GetOrdinal("dish_id"));
                        string dishName = reader["dish_name"].ToString();
                        string dishComposition = reader["dish_composition"].ToString();
                        decimal cost = reader.GetDecimal(reader.GetOrdinal("cost"));

                        // Определяем путь к изображению (предполагаем, что изображения названы по dish_id)
                        string imagePath = $"/Data/Food/dish_{dishId}.jpg";

                        _dishesList.Add(new DishInfo
                        {
                            DishId = dishId,
                            DishName = dishName,
                            DishComposition = dishComposition,
                            Cost = cost,
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
        public int DishId { get; set; }
        public string DishName { get; set; }
        public string DishComposition { get; set; }
        public decimal Cost { get; set; }
        public string ImagePath { get; set; }
    }
}
