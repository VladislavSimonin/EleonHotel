using EleonHotel.Services;
using EleonHotel.Windows.AdminWindows;
using EleonHotel.Windows.MainWindows.Admin;
using EleonHotel.Windows.MainWindows.Staff;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using EleonHotel.Properties;
using System.Data.SqlClient;

namespace EleonHotel.Windows.MainWindows
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public int UserId { get; private set; }

        public AdminWindow(int userId)
        {
            InitializeComponent();
            UserId = userId;
        }

        private void fullUsersInfo_Click(object sender, RoutedEventArgs e)
        {
            new fullUsersInfo().ShowDialog();
        }

        private void roomsCondition_Click(object sender, RoutedEventArgs e)
        {
            new roomsCondition().ShowDialog();
        }

        private void deleteUser_Click(object sender, RoutedEventArgs e)
        {
            new deleteUser().ShowDialog();
        }

        private void staffInfo_Click(object sender, RoutedEventArgs e)
        {
            new staffInfo().ShowDialog();
        }

        private void Conflicts_Click(object sender, RoutedEventArgs e)
        {
            new Conflicts().ShowDialog();
        }

        private void guestsInfo_Click(object sender, RoutedEventArgs e)
        {
            new guestsInfo().ShowDialog();
        }

        private void SalesReport_Click(object sender, RoutedEventArgs e)
        {
            try насрать
            {
                // Открываем диалог сохранения файла
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Отчет_По_Продажам{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Получаем строку подключения из настроек
                    string connectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

                    // Создаем генератор отчета и генерируем файл
                    var reportGenerator = new SalesReportGenerator(connectionString);
                    reportGenerator.GenerateReport(saveFileDialog.FileName);

                    MessageBox.Show($"Отчет успешно создан:\n{saveFileDialog.FileName}",
                        "Sales Report", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
