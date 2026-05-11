using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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
            // Собираем все выбранные блюда с количеством
            var orderedDishes = new List<OrderedDishInfo>();
            
            foreach (var dish in _dishesList)
            {
                // Находим ComboBox для этого блюда
                var comboBox = FindComboBoxForDish(dish.dish_id);
                if (comboBox != null && comboBox.SelectedIndex > 0)
                {
                    int quantity = comboBox.SelectedIndex; // Индекс соответствует количеству (0 = не выбрано)
                    if (quantity > 0)
                    {
                        orderedDishes.Add(new OrderedDishInfo
                        {
                            dish_id = dish.dish_id,
                            dish_name = dish.dish_name,
                            cost = dish.cost,
                            Quantity = quantity
                        });
                    }
                }
            }

            // Если ничего не выбрано
            if (orderedDishes.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы одно блюдо", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем количество столовых приборов
            int cutleryCount = 1;
            if (CbCutlery.SelectedItem is ComboBoxItem selectedItem)
            {
                int.TryParse(selectedItem.Content?.ToString(), out cutleryCount);
            }

            // Получаем номер комнаты пользователя
            int roomNumber = GetRoomNumberForUser(_userId);
            if (roomNumber == 0)
            {
                MessageBox.Show("У вас нет назначенной комнаты. Обратитесь к администратору.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Открываем окно оплаты
            var paymentWindow = new FoodPaymentWindow(_userId, roomNumber, cutleryCount, orderedDishes);
            paymentWindow.Owner = this;
            if (paymentWindow.ShowDialog() == true)
            {
                // Оплата прошла успешно - закрываем окно заказа
                Close();
            }
        }

        private ComboBox FindComboBoxForDish(int dish_id)
        {
            foreach (var item in DishesItemsControl.Items)
            {
                var container = (System.Windows.Controls.ContentPresenter)DishesItemsControl.ItemContainerGenerator.ContainerFromItem(item);
                if (container != null)
                {
                    var comboBox = FindChild<ComboBox>(container, "QuantityComboBox");
                    if (comboBox != null)
                    {
                        var tag = comboBox.Tag?.ToString();
                        if (int.TryParse(tag, out int id) && id == dish_id)
                        {
                            return comboBox;
                        }
                    }
                }
            }
            return null;
        }

        private T FindChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.Name == name && child is T t)
                    return t;

                var childOfChild = FindChild<T>(child, name);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private int GetRoomNumberForUser(int userId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT r.number
                    FROM Guests g
                    LEFT JOIN Rooms r ON g.room_id = r.room_id
                    WHERE g.user_id = @userId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (int)result;
                    }
                }
            }
            return 0;
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
