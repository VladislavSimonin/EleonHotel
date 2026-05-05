using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dadata;

namespace EleonHotel.Data.Classes
{
    /// <summary>
    /// Сервис для работы с API DaData
    /// </summary>
    public class DadataService
    {
        private readonly SuggestClientAsync _suggestClient;
        private readonly CleanClientAsync _cleanClient;

        // Замените на ваш API-ключ от DaData
        // Получите ключ на https://dadata.ru/api/key/
        private const string ApiKey = "ВАШ_API_КЛЮЧ";
        private const string SecretKey = "ВАШ_SECRET_КЛЮЧ"; // опционально для очистки

        public DadataService()
        {
            _suggestClient = new SuggestClientAsync(ApiKey);
            _cleanClient = new CleanClientAsync(ApiKey, SecretKey);
        }

        /// <summary>
        /// Подсказки по адресу
        /// </summary>
        public async Task<List<string>> GetAddressSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            try
            {
                var suggestions = await _suggestClient.SuggestAddressAsync(query);
                var result = new List<string>();
                
                foreach (var suggestion in suggestions)
                {
                    result.Add(suggestion.Value);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (адрес): {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Подсказки по ФИО
        /// </summary>
        public async Task<List<string>> GetFioSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            try
            {
                var suggestions = await _suggestClient.SuggestFioAsync(query);
                var result = new List<string>();
                
                foreach (var suggestion in suggestions)
                {
                    result.Add(suggestion.Value);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (ФИО): {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Подсказки по телефону
        /// </summary>
        public async Task<List<string>> GetPhoneSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            try
            {
                var suggestions = await _suggestClient.SuggestPhoneAsync(query);
                var result = new List<string>();
                
                foreach (var suggestion in suggestions)
                {
                    result.Add(suggestion.Value);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (телефон): {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Подсказки по email
        /// </summary>
        public async Task<List<string>> GetEmailSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            try
            {
                var suggestions = await _suggestClient.SuggestEmailAsync(query);
                var result = new List<string>();
                
                foreach (var suggestion in suggestions)
                {
                    result.Add(suggestion.Value);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (email): {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Очистка и валидация адреса (возвращает структурированные данные)
        /// </summary>
        public async Task<AddressInfo> CleanAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            try
            {
                var result = await _cleanClient.CleanAddressAsync(address);
                if (result != null)
                {
                    return new AddressInfo
                    {
                        FullAddress = result.Value,
                        Region = result.Region,
                        City = result.City,
                        Street = result.Street,
                        House = result.House,
                        PostalCode = result.PostalCode,
                        IsValid = result.Ok
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (очистка адреса): {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Очистка и валидация ФИО
        /// </summary>
        public async Task<FioInfo> CleanFioAsync(string fio)
        {
            if (string.IsNullOrWhiteSpace(fio))
                return null;

            try
            {
                var result = await _cleanClient.CleanFioAsync(fio);
                if (result != null)
                {
                    return new FioInfo
                    {
                        FullName = result.Value,
                        Surname = result.Surname,
                        Name = result.Name,
                        Patronymic = result.Patronymic,
                        IsValid = result.Ok
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (очистка ФИО): {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Очистка и валидация телефона
        /// </summary>
        public async Task<PhoneInfo> CleanPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            try
            {
                var result = await _cleanClient.CleanPhoneAsync(phone);
                if (result != null)
                {
                    return new PhoneInfo
                    {
                        PhoneNumber = result.Value,
                        Country = result.Country,
                        Region = result.Region,
                        City = result.City,
                        IsValid = result.Ok,
                        Formatted = result.International
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка DaData (очистка телефона): {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Структура для хранения информации об адресе
    /// </summary>
    public class AddressInfo
    {
        public string FullAddress { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string PostalCode { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// Структура для хранения информации о ФИО
    /// </summary>
    public class FioInfo
    {
        public string FullName { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// Структура для хранения информации о телефоне
    /// </summary>
    public class PhoneInfo
    {
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Formatted { get; set; }
        public bool IsValid { get; set; }
    }
}
