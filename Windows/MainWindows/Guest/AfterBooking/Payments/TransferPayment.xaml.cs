using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EleonHotel.Windows.MainWindows.Guest.AfterBooking.Payments
{
    public partial class TransferPayment : Window
    {
        private readonly int _userId;
        private readonly int _serviceId;
        private readonly string _serviceName;
        private readonly decimal _serviceCost;
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public TransferPayment(int userId, int serviceId, string serviceName, decimal serviceCost)
        {
            InitializeComponent();
            _userId = userId;
            _serviceId = serviceId;
            _serviceName = serviceName;
            _serviceCost = serviceCost;

            InitializePaymentInfo();
        }

        private void InitializePaymentInfo()
        {
            TblServiceName.Text = _serviceName;
            TblServiceCost.Text = $"{_serviceCost:#,##0} ₽";
            TblTotalAmount.Text = $"{_serviceCost:#,##0} ₽";
        }

        private bool ValidateCardData()
        {
            string cardNumber = TbCardNumber.Text.Replace(" ", "");
            if (cardNumber.Length != 16 || !long.TryParse(cardNumber, out _))
            {
                MessageBox.Show("Пожалуйста, введите корректный номер карты (16 цифр)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCardNumber.Focus();
                return false;
            }

            string expiryDate = TbExpiryDate.Text;
            if (expiryDate.Length != 5 || !DateTime.TryParseExact(expiryDate, "MM/yy", null, System.Globalization.DateTimeStyles.None, out _))
            {
                MessageBox.Show("Пожалуйста, введите корректный срок действия карты (MM/YY)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbExpiryDate.Focus();
                return false;
            }

            string cvv = TbCvv.Text;
            if (cvv.Length != 3 || !int.TryParse(cvv, out _))
            {
                MessageBox.Show("Пожалуйста, введите корректный CVV-код (3 цифры)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCvv.Focus();
                return false;
            }

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

                        string checkServiceQuery = @"
                            SELECT ordered_services_count FROM Ordered_services
                            WHERE guest_id = @guestId AND service_id = @serviceId";

                        object existingCountObj = null;
                        using (var cmd = new SqlCommand(checkServiceQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@guestId", guestId);
                            cmd.Parameters.AddWithValue("@serviceId", _serviceId);
                            existingCountObj = cmd.ExecuteScalar();
                        }

                        if (existingCountObj != null && existingCountObj != DBNull.Value)
                        {
                            int currentCount = (int)existingCountObj;
                            int newCount = currentCount + 1;

                            string updateServiceQuery = @"
                                UPDATE Ordered_services
                                SET ordered_services_count = @count
                                WHERE guest_id = @guestId AND service_id = @serviceId";
                            using (var cmd = new SqlCommand(updateServiceQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@count", newCount);
                                cmd.Parameters.AddWithValue("@guestId", guestId);
                                cmd.Parameters.AddWithValue("@serviceId", _serviceId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insertServiceQuery = @"
                                INSERT INTO Ordered_services (guest_id, service_id, ordered_services_count)
                                VALUES (@guestId, @serviceId, 1)";
                            using (var cmd = new SqlCommand(insertServiceQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@guestId", guestId);
                                cmd.Parameters.AddWithValue("@serviceId", _serviceId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        string updatePaymentQuery = @"
                            UPDATE Payment_invoices
                            SET total = total + @amount
                            WHERE guest_id = @guestId";
                        using (var cmd = new SqlCommand(updatePaymentQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@amount", _serviceCost);
                            cmd.Parameters.AddWithValue("@guestId", guestId);
                            cmd.ExecuteNonQuery();
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
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
            if (!ValidateCardData())
                return;

            LoadingOverlay.Visibility = Visibility.Visible;
            BtnPay.IsEnabled = false;

            try
            {
                await Task.Delay(2000);

                bool success = await ProcessPaymentAsync();

                if (success)
                {
                    MessageBox.Show(
                        $"Оплата прошла успешно!\n\nСумма оплаты: {_serviceCost:#,##0} ₽\nУслуга \"{_serviceName}\" заказана.",
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
            string expiry = TbExpiryDate.Text.Replace("/", "");
            if (expiry.Length > 4)
                expiry = expiry.Substring(0, 4);

            string formatted = "";
            for (int i = 0; i < expiry.Length; i++)
            {
                if (i == 2)
                    formatted += "/";
                formatted += expiry[i];
            }

            TbExpiryDate.Text = formatted;
            TbExpiryDate.CaretIndex = TbExpiryDate.Text.Length;
        }
    }
}
