using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Guest.AfterBooking
{
    public partial class FoodPaymentWindow : Window
    {
        private readonly int _userId;
        private readonly int _roomNumber;
        private readonly int _cutleryCount;
        private readonly List<OrderedDishInfo> _orderedDishes;
        private readonly decimal _deliverycost = 200; // Стоимость доставки из Additional_services
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public FoodPaymentWindow(int userId, int roomNumber, int cutleryCount, List<OrderedDishInfo> orderedDishes)
        {
            InitializeComponent();
            _userId = userId;
            _roomNumber = roomNumber;
            _cutleryCount = cutleryCount;
            _orderedDishes = orderedDishes;

            InitializePaymentInfo();
        }

        private async void InitializePaymentInfo()
        {
            // Заполнение информации о доставке
            TblRoomNumber.Text = _roomNumber.ToString();
            TblCutleryCount.Text = _cutleryCount.ToString();

            // Заполнение списка блюд
            DishesItemsControl.ItemsSource = _orderedDishes;

            // Получение стоимости доставки из базы данных
            decimal deliverycost = await GetDeliverycostAsync();

            // Расчет стоимости
            decimal dishesTotal = 0;
            foreach (var dish in _orderedDishes)
            {
                dishesTotal += dish.cost * dish.Quantity;
            }

            decimal totalAmount = dishesTotal + deliverycost;

            TblDishesTotal.Text = $"{dishesTotal:#,##0} ₽";
            TblDeliveryCost.Text = $"{deliverycost:#,##0} ₽";
            TblTotalAmount.Text = $"{totalAmount:#,##0} ₽";
        }

        private async Task<decimal> GetDeliverycostAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();
                        string query = "SELECT cost FROM Additional_services WHERE service_id = 4";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                return (decimal)result;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Игнорируем ошибку и возвращаем значение по умолчанию
                }
                return _deliverycost;
            });
        }

        private bool ValidateCardData()
        {
            // Проверка номера карты (простая проверка на 16 цифр)
            string cardNumber = TbCardNumber.Text.Replace(" ", "");
            if (cardNumber.Length != 16 || !long.TryParse(cardNumber, out _))
            {
                MessageBox.Show("Пожалуйста, введите корректный номер карты (16 цифр)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCardNumber.Focus();
                return false;
            }

            // Проверка срока действия
            string expiryDate = TbExpiryDate.Text;
            if (expiryDate.Length != 5 || !DateTime.TryParseExact(expiryDate, "MM/yy", null, System.Globalization.DateTimeStyles.None, out _))
            {
                MessageBox.Show("Пожалуйста, введите корректный срок действия карты (MM/YY)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbExpiryDate.Focus();
                return false;
            }

            // Проверка CVV
            string cvv = TbCvv.Text;
            if (cvv.Length != 3 || !int.TryParse(cvv, out _))
            {
                MessageBox.Show("Пожалуйста, введите корректный CVV-код (3 цифры)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCvv.Focus();
                return false;
            }

            // Проверка имени держателя
            if (string.IsNullOrWhiteSpace(TbCardHolder.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя держателя карты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCardHolder.Focus();
                return false;
            }

            return true;
        }

        private async Task<bool> ProcessPaymentAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        conn.Open();

                        // Получаем guest_id по user_id
                        int guestId = -1;
                        string getGuestQuery = "SELECT guest_id FROM Guests WHERE user_id = @userId";
                        using (var cmd = new SqlCommand(getGuestQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", _userId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                guestId = (int)result;
                            }
                        }

                        if (guestId == -1)
                        {
                            MessageBox.Show(
                                "Не удалось найти информацию о госте. Пожалуйста, обратитесь к администратору.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return false;
                        }

                        decimal deliverycost = 0;
                        string getDeliverycostQuery = "SELECT cost FROM Additional_services WHERE service_id = 4";
                        using (var cmd = new SqlCommand(getDeliverycostQuery, conn))
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                deliverycost = (decimal)result;
                            }
                            else
                            {
                                deliverycost = _deliverycost;
                            }
                        }

                        // Проверяем, существует ли уже запись о доставке (service_id = 4) для этого гостя
                        string checkDeliveryQuery = @"
                            SELECT COUNT(*) FROM Ordered_services
                            WHERE guest_id = @guestId AND service_id = 4";
                        bool deliveryExists = false;
                        using (var cmd = new SqlCommand(checkDeliveryQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@guestId", guestId);
                            int count = (int)cmd.ExecuteScalar();
                            deliveryExists = count > 0;
                        }

                        // Добавляем запись о доставке в Ordered_services (service_id = 4), только если её ещё нет
                        if (!deliveryExists)
                        {
                            string insertDeliveryQuery = @"
                                INSERT INTO Ordered_services (guest_id, service_id)
                                VALUES (@guestId, 4)";
                            using (var cmd = new SqlCommand(insertDeliveryQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@guestId", guestId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Добавляем заказанные блюда в Ordered_dishes
                        foreach (var dish in _orderedDishes)
                        {
                            // Проверяем, существует ли уже запись для этого гостя и блюда
                            string checkDishQuery = @"
                                SELECT ordered_dishes_count FROM Ordered_dishes
                                WHERE guest_id = @guestId AND dish_id = @dish_id";

                            object existingCountObj = null;
                            using (var cmd = new SqlCommand(checkDishQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@guestId", guestId);
                                cmd.Parameters.AddWithValue("@dish_id", dish.dish_id);
                                existingCountObj = cmd.ExecuteScalar();
                            }

                            if (existingCountObj != null && existingCountObj != DBNull.Value)
                            {
                                // Запись существует - обновляем ordered_dishes_count
                                int currentCount = (int)existingCountObj;
                                int newCount = currentCount + dish.Quantity;

                                string updateDishQuery = @"
                                    UPDATE Ordered_dishes
                                    SET ordered_dishes_count = @count
                                    WHERE guest_id = @guestId AND dish_id = @dish_id";
                                using (var cmd = new SqlCommand(updateDishQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@count", newCount);
                                    cmd.Parameters.AddWithValue("@guestId", guestId);
                                    cmd.Parameters.AddWithValue("@dish_id", dish.dish_id);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Запись не существует - создаем новую
                                string insertDishQuery = @"
                                    INSERT INTO Ordered_dishes (guest_id, dish_id, ordered_dishes_count)
                                    VALUES (@guestId, @dish_id, @count)";
                                using (var cmd = new SqlCommand(insertDishQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@guestId", guestId);
                                    cmd.Parameters.AddWithValue("@dish_id", dish.dish_id);
                                    cmd.Parameters.AddWithValue("@count", dish.Quantity);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Рассчитываем общую сумму заказа
                        decimal dishesTotal = 0;
                        foreach (var dish in _orderedDishes)
                        {
                            dishesTotal += dish.cost * dish.Quantity;
                        }
                        decimal totalAmount = dishesTotal + deliverycost;

                        // Обновляем Payment_invoices - добавляем сумму к total соответствующего гостя
                        string updatePaymentQuery = @"
                            UPDATE Payment_invoices
                            SET total = total + @amount
                            WHERE guest_id = @guestId";
                        using (var cmd = new SqlCommand(updatePaymentQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@amount", totalAmount);
                            cmd.Parameters.AddWithValue("@guestId", guestId);
                            cmd.ExecuteNonQuery();
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Показываем детальное сообщение об ошибке для отладки
                    MessageBox.Show(
                        $"Произошла ошибка при обработке платежа:\n{ex.Message}\n\nВнутренняя ошибка: {(ex.InnerException?.Message ?? "Нет данных")}",
                        "Ошибка оплаты",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            });
        }

        private async void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных карты
            if (!ValidateCardData())
                return;

            // Показываем индикатор загрузки
            LoadingOverlay.Visibility = Visibility.Visible;
            BtnPay.IsEnabled = false;

            try
            {
                // Имитация задержки обработки платежа (2 секунды)
                await Task.Delay(2000);

                // Выполняем оплату и обновляем базу данных
                bool success = await ProcessPaymentAsync();

                if (success)
                {
                    decimal totalAmount = 0;
                    foreach (var dish in _orderedDishes)
                    {
                        totalAmount += dish.cost * dish.Quantity;
                    }
                    totalAmount += _deliverycost;

                    MessageBox.Show(
                        $"Оплата прошла успешно!\n\nСумма оплаты: {totalAmount:#,##0} ₽\nЗаказ будет доставлен в комнату {_roomNumber}.",
                        "Успешная оплата",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Произошла ошибка при обработке платежа. Пожалуйста, попробуйте снова.",
                        "Ошибка оплаты",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Произошла ошибка: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                BtnPay.IsEnabled = true;
            }
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TbCardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Форматирование номера карты (разделение пробелами каждые 4 символа)
            string cardNumber = TbCardNumber.Text.Replace(" ", "");
            if (cardNumber.Length > 16)
                cardNumber = cardNumber.Substring(0, 16);

            string formatted = "";
            for (int i = 0; i < cardNumber.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formatted += " ";
                formatted += cardNumber[i];
            }

            TbCardNumber.Text = formatted;
            TbCardNumber.CaretIndex = TbCardNumber.Text.Length;
        }

        private void TbExpiryDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Форматирование срока действия (MM/YY)
            string expiry = TbExpiryDate.Text.Replace("/", "");
            if (expiry.Length > 4)
                expiry = expiry.Substring(0, 4);

            if (expiry.Length >= 2)
            {
                TbExpiryDate.Text = expiry.Substring(0, 2) + (expiry.Length > 2 ? "/" + expiry.Substring(2) : "");
            }
            else
            {
                TbExpiryDate.Text = expiry;
            }

            TbExpiryDate.CaretIndex = TbExpiryDate.Text.Length;
        }
    }

    public class OrderedDishInfo
    {
        public int dish_id { get; set; }
        public string dish_name { get; set; }
        public decimal cost { get; set; }
        public int Quantity { get; set; }
        public decimal Totalcost => cost * Quantity;
    }
}