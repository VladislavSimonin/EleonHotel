using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using EleonHotel.Data;
using EleonHotel.Models;
using System.Net;
using System.Net.Mail;

namespace EleonHotel.Windows.MainWindows
{
    public partial class PaymentWindow : Window
    {
        private readonly string _smtpServer = Properties.Settings.Default.smtpServer;
        private readonly int _smtpPort = Properties.Settings.Default.smtpPort;
        private readonly string _senderEmail = Properties.Settings.Default.senderEmail;
        private readonly string _senderPassword = Properties.Settings.Default.senderPassword;
        private readonly string _senderName = Properties.Settings.Default.senderName;

        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private readonly int _userId;
        private readonly int _categoryId;
        private readonly string _categoryName;
        private readonly DateTime _checkIn;
        private readonly DateTime _checkOut;
        private readonly decimal _pricePerNight;
        private readonly int _nights;
        private readonly decimal _totalAmount;

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

                        // Обновляем статус комнаты
                        string updateRoomStatusQuery = "UPDATE Rooms SET room_status_id = 2 WHERE room_id = @roomId";
                        using (var cmd = new SqlCommand(updateRoomStatusQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@roomId", roomId);
                            cmd.ExecuteNonQuery();
                        }

                        // Создаем запись в Payment_invoices

                        using (var cmdMaxPayment_invoices = new SqlCommand(
                            "select isnull(max(payment_id), 0) from Payment_invoices", conn))
                        {
                            int newPayment_invoices = (int)cmdMaxPayment_invoices.ExecuteScalar() + 1;

                            string createPaymentQuery = @"
                            INSERT INTO Payment_invoices (payment_id, guest_id, total, is_paid) 
                            VALUES (@paymentId, @guestId, @total, @isPaid)";
                            using (var cmd = new SqlCommand(createPaymentQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@paymentId", newPayment_invoices);
                                cmd.Parameters.AddWithValue("@guestId", guestId);
                                cmd.Parameters.AddWithValue("@total", _totalAmount);
                                cmd.Parameters.AddWithValue("@isPaid", true);
                                cmd.ExecuteNonQuery();
                            }

                            // Получаем email пользователя и номер комнаты для отправки письма
                            string userEmail = "";
                            int roomNumber = 0;

                            string getUserEmailQuery = "SELECT Email FROM Users WHERE user_id = @userId";
                            using (var cmd = new SqlCommand(getUserEmailQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@userId", _userId);
                                object result = cmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    userEmail = result.ToString();
                                }
                            }

                            string getRoomNumberQuery = "SELECT Number FROM Rooms WHERE room_id = @roomId";
                            using (var cmd = new SqlCommand(getRoomNumberQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@roomId", roomId);
                                object result = cmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    roomNumber = (int)result;
                                }
                            }

                            // Отправляем email с информацией о бронировании
                            if (!string.IsNullOrEmpty(userEmail))
                            {
                                SendBookingConfirmationEmail(userEmail, roomNumber);
                            }

                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        private void SendBookingConfirmationEmail(string userEmail, int roomNumber)
        {
            try
            {
                var fromAddress = new MailAddress(_senderEmail, _senderName);
                var toAddress = new MailAddress(userEmail);

                var message = new MailMessage
                {
                    From = fromAddress,
                    Subject = $"Подтверждение бронирования номера {roomNumber}",
                    IsBodyHtml = true
                };
                message.To.Add(toAddress);

                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #4CAF50;'>Бронирование подтверждено!</h2>
                        <p>Уважаемый гость,</p>
                        <p>Ваше бронирование успешно оформлено. Ниже представлена информация о вашем пребывании:</p>

                        <table style='border-collapse: collapse; width: 100%; max-width: 500px; margin: 20px 0;'>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Номер комнаты:</td>
                                <td style='padding: 12px; border: 1px solid #ddd;'>{roomNumber}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Категория номера:</td>
                                <td style='padding: 12px; border: 1px solid #ddd;'>{_categoryName}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Дата заезда:</td>
                                <td style='padding: 12px; border: 1px solid #ddd;'>{_checkIn:dd.MM.yyyy}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Дата выезда:</td>
                                <td style='padding: 12px; border: 1px solid #ddd;'>{_checkOut:dd.MM.yyyy}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Количество суток:</td>
                                <td style='padding: 12px; border: 1px solid #ddd;'>{_nights}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Стоимость за сутки:</td>
                                <td style='padding: 12px; border: 1px solid #ddd;'>{_pricePerNight:#,##0} ₽</td>
                            </tr>
                            <tr style='background-color: #4CAF50; color: white;'>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>Итоговая сумма:</td>
                                <td style='padding: 12px; border: 1px solid #ddd; font-weight: bold;'>{_totalAmount:#,##0} ₽</td>
                            </tr>
                        </table>

                        <p>Ждем вас в нашем отеле!</p>
                        <p style='color: #666; font-size: 12px;'>С уважением,<br>{_senderName}</p>
                    </body>
                    </html>
                ";

                message.Body = body;

                var smtp = new SmtpClient
                {
                    Host = _smtpServer,
                    Port = _smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_senderEmail, _senderPassword)
                };

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                // Логируем ошибку отправки email, но не прерываем процесс оплаты
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки email: {ex.Message}");
            }
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
