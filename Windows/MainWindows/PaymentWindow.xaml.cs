using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace EleonHotel.Windows.MainWindows
{
    public partial class PaymentWindow : Window
    {
        private readonly int _userId;
        private readonly int _categoryId;
        private readonly string _categoryName;
        private readonly DateTime _checkIn;
        private readonly DateTime _checkOut;
        private readonly decimal _pricePerNight;
        private readonly int _nights;
        private readonly decimal _totalAmount;
        private readonly string ConnectionString = Properties.Settings.Default.ConnectionString;

        public PaymentWindow(int userId, int categoryId, string categoryName, DateTime checkIn, DateTime checkOut, decimal pricePerNight)
        {
            InitializeComponent();
            _userId = userId;
            _categoryId = categoryId;
            _categoryName = categoryName;
            _checkIn = checkIn;
            _checkOut = checkOut;
            _pricePerNight = pricePerNight;
            _nights = (checkOut - checkIn).Days;
            _totalAmount = _pricePerNight * _nights;

            InitializePaymentInfo();
        }

        private void InitializePaymentInfo()
        {
            // Заполнение информации о бронировании
            TblRoomCategory.Text = _categoryName;
            TblCheckIn.Text = _checkIn.ToString("dd.MM.yyyy");
            TblCheckOut.Text = _checkOut.ToString("dd.MM.yyyy");
            TblNights.Text = $"{_nights} сут.";
            TblPricePerNight.Text = $"{_pricePerNight:#,##0} ₽";
            TblTotalAmount.Text = $"{_totalAmount:#,##0} ₽";
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
                    MessageBox.Show(
                        $"Оплата прошла успешно!\n\nСумма оплаты: {_totalAmount:#,##0} ₽\nНомер категории \"{_categoryName}\" забронирован.",
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

                        // Проверяем, существует ли пользователь
                        string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE user_id = @userId";
                        using (var cmd = new SqlCommand(checkUserQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", _userId);
                            int userCount = (int)cmd.ExecuteScalar();
                            if (userCount == 0)
                                return false;
                        }

                        // Проверяем, есть ли уже запись Guest для этого пользователя
                        int guestId = -1;
                        string checkGuestQuery = "SELECT guest_id FROM Guests WHERE user_id = @userId";
                        using (var cmd = new SqlCommand(checkGuestQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", _userId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                guestId = (int)result;
                            }
                        }

                        // Если записи Guest нет, создаем новую
                        if (guestId == -1)
                        {
                            string createGuestQuery = "INSERT INTO Guests (user_id) VALUES (@userId); SELECT SCOPE_IDENTITY();";
                            using (var cmd = new SqlCommand(createGuestQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@userId", _userId);
                                guestId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        // Находим свободный номер выбранной категории
                        // RoomStatusId = 1 означает "свободен"
                        int roomId = -1;
                        string findRoomQuery = @"
                            SELECT TOP 1 room_id 
                            FROM Rooms 
                            WHERE room_category_id = @categoryId AND room_status_id = 1
                            ORDER BY room_id";
                        using (var cmd = new SqlCommand(findRoomQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@categoryId", _categoryId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                roomId = (int)result;
                            }
                        }

                        // Если нет свободных номеров с status_id = 1, пробуем найти любой номер этой категории
                        if (roomId == -1)
                        {
                            string findAnyRoomQuery = @"
                                SELECT TOP 1 room_id 
                                FROM Rooms 
                                WHERE room_category_id = @categoryId
                                ORDER BY room_id";
                            using (var cmd = new SqlCommand(findAnyRoomQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@categoryId", _categoryId);
                                object result = cmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    roomId = (int)result;
                                }
                            }
                        }

                        if (roomId == -1)
                            return false;

                        // Привязываем номер к гостю (обновляем таблицу Guests)
                        string updateGuestQuery = "UPDATE Guests SET room_id = @roomId WHERE guest_id = @guestId";
                        using (var cmd = new SqlCommand(updateGuestQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@roomId", roomId);
                            cmd.Parameters.AddWithValue("@guestId", guestId);
                            cmd.ExecuteNonQuery();
                        }

                        // Создаем запись в Payment_invoices
                        string createPaymentQuery = @"
                            INSERT INTO Payment_invoices (guest_id, total, is_paid) 
                            VALUES (@guestId, @total, @isPaid)";
                        using (var cmd = new SqlCommand(createPaymentQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@guestId", guestId);
                            cmd.Parameters.AddWithValue("@total", _totalAmount);
                            cmd.Parameters.AddWithValue("@isPaid", true);
                            cmd.ExecuteNonQuery();
                        }

                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TbCardNumber_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Форматирование номера карты (разделение на группы по 4 цифры)
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

            if (TbCardNumber.Text != formatted)
            {
                TbCardNumber.Text = formatted;
                TbCardNumber.CaretIndex = formatted.Length;
            }
        }

        private void TbExpiryDate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Форматирование срока действия (MM/YY)
            string input = TbExpiryDate.Text.Replace("/", "");
            if (input.Length > 4)
                input = input.Substring(0, 4);

            string formatted = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (i == 2 && input.Length > 2)
                    formatted += "/";
                formatted += input[i];
            }

            if (TbExpiryDate.Text != formatted)
            {
                TbExpiryDate.Text = formatted;
                TbExpiryDate.CaretIndex = formatted.Length;
            }
        }
    }
}
