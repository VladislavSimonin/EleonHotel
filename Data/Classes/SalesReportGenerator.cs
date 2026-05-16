using System;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace EleonHotel.Services
{
    public class SalesReportGenerator
    {
        private readonly string _connectionString;

        public SalesReportGenerator(string connectionString)
        {
            _connectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        }

        public void GenerateReport(string filePath)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Вычисляем финансовые показатели
                var totalRevenue = GetTotalRevenue(connection);
                var roomRevenue = GetRoomRevenue(connection);
                var foodBeverageRevenue = GetFoodBeverageRevenue(connection);
                var additionalServicesRevenue = GetAdditionalServicesRevenue(connection);

                var operationalExpenses = GetOperationalExpenses(connection);
                var administrativeExpenses = GetAdministrativeExpenses(connection);
                var totalExpenses = operationalExpenses + administrativeExpenses;

                var gop = totalRevenue - totalExpenses;
                var ebitda = gop; // Для простоты EBITDA = GOP (без учета амортизации)
                var netProfit = gop; // Чистая прибыль (без налогов и процентов)

                // Операционные показатели (KPI)
                var kpiData = GetKPIData(connection);
                var occupancyRate = kpiData.OccupancyRate;
                var adr = kpiData.ADR;
                var revPAR = kpiData.RevPAR;
                var tRevPAR = kpiData.TRevPAR;
                var gopPAR = kpiData.GOPPAR;
                var cpor = kpiData.CPOR;

                // Создаем Excel файл
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sales Report");

                    // Заголовок
                    worksheet.Cell(1, 1).Value = "ОТЧЕТ ПО ПРОДАЖАМ";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Row(1).Height = 30;

                    // Дата генерации
                    worksheet.Cell(2, 1).Value = $"Создан: {DateTime.Now:dd.MM.yyyy HH:mm}";
                    worksheet.Row(2).Height = 20;

                    // Финансовые показатели (P&L)
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "ФИНАНСОВЫЕ ПОКАЗАТЕЛИ (P&L)";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Общий доход:";
                    worksheet.Cell(currentRow, 2).Value = totalRevenue;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "  - Доход от бронирований комнат:";
                    worksheet.Cell(currentRow, 2).Value = roomRevenue;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "  - Доход от заказанных блюд (F&B):";
                    worksheet.Cell(currentRow, 2).Value = foodBeverageRevenue;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "  - Дополнительные услуги:";
                    worksheet.Cell(currentRow, 2).Value = additionalServicesRevenue;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Операционные расходы:";
                    worksheet.Cell(currentRow, 2).Value = operationalExpenses;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Административные расходы:";
                    worksheet.Cell(currentRow, 2).Value = administrativeExpenses;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Общие расходы:";
                    worksheet.Cell(currentRow, 2).Value = totalExpenses;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                    currentRow++;

                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Валовая операционная прибыль (GOP):";
                    worksheet.Cell(currentRow, 2).Value = gop;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Прибыль до вычета процентов, налогов, износа и амортизации (EBITDA):";
                    worksheet.Cell(currentRow, 2).Value = ebitda;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Чистая прибыль:";
                    worksheet.Cell(currentRow, 2).Value = netProfit;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    currentRow++;

                    // Операционные показатели (KPI)
                    currentRow += 2;
                    worksheet.Cell(currentRow, 1).Value = "ОПЕРАЦИОННЫЕ ПОКАЗАТЕЛИ (KPI)";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Загрузка номеров:";
                    worksheet.Cell(currentRow, 2).Value = $"{occupancyRate:F2}%";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Средняя цена номера (ADR):";
                    worksheet.Cell(currentRow, 2).Value = adr;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Доход на доступный номер (RevPAR):";
                    worksheet.Cell(currentRow, 2).Value = revPAR;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Общий доход на доступный номер (TRevPAR):";
                    worksheet.Cell(currentRow, 2).Value = tRevPAR;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Валовая операционная прибыль на доступный номер (GOPPAR):";
                    worksheet.Cell(currentRow, 2).Value = gopPAR;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Затраты на проданный номер (CPOR):";
                    worksheet.Cell(currentRow, 2).Value = cpor;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                    currentRow++;

                    // Автоподбор ширины колонок
                    worksheet.Columns().AdjustToContents();

                    // Сохраняем файл
                    workbook.SaveAs(filePath);
                }
            }
        }

        private decimal GetTotalRevenue(SqlConnection connection)
        {
            return GetRoomRevenue(connection) + GetFoodBeverageRevenue(connection) + GetAdditionalServicesRevenue(connection);
        }

        private decimal GetRoomRevenue(SqlConnection connection)
        {
            const string query = @"
                SELECT ISNULL(SUM(pi.total), 0)
                FROM Payment_invoices pi
                WHERE pi.is_paid = 1";

            using (var command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
        }

        private decimal GetFoodBeverageRevenue(SqlConnection connection)
        {
            const string query = @"
                SELECT ISNULL(SUM(rm.cost * od.ordered_dishes_count), 0)
                FROM Ordered_dishes od
                JOIN Restaurant_menu rm ON od.dish_id = rm.dish_id
                JOIN Guests g ON od.guest_id = g.guest_id
                JOIN Payment_invoices pi ON g.guest_id = pi.guest_id
                WHERE pi.is_paid = 1";

            using (var command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
        }

        private decimal GetAdditionalServicesRevenue(SqlConnection connection)
        {
            const string query = @"
                SELECT ISNULL(SUM(asvc.cost), 0)
                FROM Ordered_services os
                JOIN Additional_services asvc ON os.service_id = asvc.service_id
                JOIN Guests g ON os.guest_id = g.guest_id
                JOIN Payment_invoices pi ON g.guest_id = pi.guest_id
                WHERE pi.is_paid = 1";

            using (var command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
        }

        private decimal GetOperationalExpenses(SqlConnection connection)
        {
            // Зарплаты сотрудников за год
            const string query = @"
                SELECT ISNULL(SUM(e.salary), 0)
                FROM Employees e";

            using (var command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
        }

        private decimal GetAdministrativeExpenses(SqlConnection connection)
        {
            // Коммунальные услуги и расходные материалы (можно добавить отдельные таблицы)
            // Пока вернем 0 или можно добавить фиксированную сумму
            return 0m;
        }

        private KPIData GetKPIData(SqlConnection connection)
        {
            var kpiData = new KPIData();

            // Получаем общее количество номеров
            const string totalRoomsQuery = "SELECT COUNT(*) FROM Rooms";
            int totalRooms;
            using (var command = new SqlCommand(totalRoomsQuery, connection))
            {
                totalRooms = Convert.ToInt32(command.ExecuteScalar());
            }

            // Получаем количество проданных ночей за год
            const string occupiedNightsQuery = @"
                SELECT COUNT(DISTINCT g.guest_id)
                FROM Guests g
                WHERE g.room_id IS NOT NULL";

            int occupiedRooms;
            using (var command = new SqlCommand(occupiedNightsQuery, connection))
            {
                var result = command.ExecuteScalar();
                occupiedRooms = result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }

            // Occupancy Rate
            if (totalRooms > 0)
            {
                kpiData.OccupancyRate = (decimal)occupiedRooms / totalRooms * 100;
            }

            // ADR (Average Daily Rate)
            const string adrQuery = @"
                SELECT
                    CASE
                        WHEN COUNT(DISTINCT g.guest_id) > 0
                        THEN ISNULL(SUM(pi.total), 0) / COUNT(DISTINCT g.guest_id)
                        ELSE 0
                    END
                FROM Guests g
                LEFT JOIN Payment_invoices pi ON g.guest_id = pi.guest_id AND pi.is_paid = 1
                WHERE g.room_id IS NOT NULL";

            using (var command = new SqlCommand(adrQuery, connection))
            {
                var result = command.ExecuteScalar();
                kpiData.ADR = result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }

            // RevPAR
            const string revPARQuery = @"
                SELECT ISNULL(SUM(pi.total), 0)
                FROM Payment_invoices pi
                WHERE pi.is_paid = 1";

            decimal totalRoomRevenue;
            using (var command = new SqlCommand(revPARQuery, connection))
            {
                var result = command.ExecuteScalar();
                totalRoomRevenue = result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }

            kpiData.RevPAR = totalRooms > 0 ? totalRoomRevenue / totalRooms : 0m;

            // TRevPAR (Total Revenue per Available Room)
            decimal totalRevenue = GetTotalRevenue(connection);
            kpiData.TRevPAR = totalRooms > 0 ? totalRevenue / totalRooms : 0m;

            // GOPPAR
            var operationalExpenses = GetOperationalExpenses(connection);
            var gop = totalRevenue - operationalExpenses;
            kpiData.GOPPAR = totalRooms > 0 ? gop / totalRooms : 0m;

            // CPOR (Cost per Occupied Room)
            kpiData.CPOR = occupiedRooms > 0 ? operationalExpenses / occupiedRooms : 0m;

            return kpiData;
        }

        private class KPIData
        {
            public decimal OccupancyRate { get; set; }
            public decimal ADR { get; set; }
            public decimal RevPAR { get; set; }
            public decimal TRevPAR { get; set; }
            public decimal GOPPAR { get; set; }
            public decimal CPOR { get; set; }
        }
    }
}