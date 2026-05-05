# Инструкция по подключению API DaData к CreateAccount.xaml.cs

## Что было сделано

1. **Создан сервис DadataService** (`/workspace/Data/Classes/DadataService.cs`)
   - Обеспечивает работу с API DaData для подсказок и очистки данных
   - Поддерживает адреса, ФИО, телефоны и email

2. **Создан помощник DadataTextBoxHelper** (`/workspace/Helpers/DadataTextBoxHelper.cs`)
   - Автоматически подключает автодополнение к любому TextBox
   - Отображает подсказки в выпадающем списке (Popup)
   - Поддерживает навигацию стрелками и выбор Enter

3. **Обновлен CreateAccount.xaml.cs**
   - Добавлена инициализация DaData для 6 TextBox
   - Добавлена очистка ресурсов при закрытии окна

4. **Добавлен NuGet пакет Dadata** в EleonHotel.csproj

---

## Настройка API-ключей

### Шаг 1: Получите API-ключи
1. Зарегистрируйтесь на https://dadata.ru/
2. Перейдите в личный кабинет: https://dadata.ru/api/key/
3. Скопируйте API-ключ и Secret-ключ

### Шаг 2: Вставьте ключи в код
Откройте файл `/workspace/Data/Classes/DadataService.cs` и замените:

```csharp
private const string ApiKey = "ВАШ_API_КЛЮЧ";
private const string SecretKey = "ВАШ_SECRET_КЛЮЧ";
```

На ваши реальные ключи:

```csharp
private const string ApiKey = "ваш_api_ключ_из_личного_кабинета";
private const string SecretKey = "ваш_secret_ключ_из_личного_кабинета";
```

---

## Какие TextBox подключены к DaData

В файле `CreateAccount.xaml.cs` автоматически подключены следующие поля:

| TextBox | Тип подсказок | Описание |
|---------|--------------|----------|
| `TbRegistrationAddress` | Address | Адрес регистрации |
| `TbSurname` | Fio | Фамилия |
| `TbName` | Fio | Имя |
| `TbPatronymic` | Fio | Отчество |
| `TbPhone` | Phone | Номер телефона |
| `TbEmail` | Email | Электронная почта |

---

## Как это работает

1. Пользователь начинает вводить текст в поле (минимум 2 символа)
2. Через 200 мс отправляется запрос к API DaData
3. Полученные подсказки отображаются в выпадающем списке под полем
4. Пользователь может:
   - Кликнуть мышкой по нужному варианту
   - Использовать стрелки ↑↓ для навигации
   - Нажать Enter для выбора
   - Нажать Escape для закрытия списка

---

## Как добавить автодополнение к другому TextBox

Если вы хотите подключить DaData к другому полю в другом окне:

```csharp
// В конструкторе окна или в методе инициализации
private DadataTextBoxHelper _myHelper;

public MyWindow()
{
    InitializeComponent();
    
    // Подключаем автодополнение для адреса
    _myHelper = new DadataTextBoxHelper(MyTextBox, DadataTextBoxHelper.PopupType.Address);
}

// При закрытии окна освобождаем ресурсы
protected override void OnClosed(EventArgs e)
{
    _myHelper?.Cleanup();
    base.OnClosed(e);
}
```

Доступные типы подсказок:
- `PopupType.Address` - адреса
- `PopupType.Fio` - ФИО
- `PopupType.Phone` - телефоны
- `PopupType.Email` - email

---

## Доступные методы DadataService

Если вам нужно программно получить подсказки или проверить данные:

```csharp
var dadata = new DadataService();

// Получить подсказки по адресу
var addresses = await dadata.GetAddressSuggestionsAsync("москва тверская 1");

// Получить подсказки по ФИО
var fios = await dadata.GetFioSuggestionsAsync("иванов иван");

// Получить подсказки по телефону
var phones = await dadata.GetPhoneSuggestionsAsync("+7999");

// Получить подсказки по email
var emails = await dadata.GetEmailSuggestionsAsync("gmail");

// Очистить и проверить адрес (возвращает структурированные данные)
var addressInfo = await dadata.CleanAddressAsync("москва тверская 1");
if (addressInfo.IsValid)
{
    string region = addressInfo.Region;
    string city = addressInfo.City;
    string street = addressInfo.Street;
    string house = addressInfo.House;
    string postalCode = addressInfo.PostalCode;
}

// Очистить и проверить ФИО
var fioInfo = await dadata.CleanFioAsync("иванов иван иванович");
if (fioInfo.IsValid)
{
    string surname = fioInfo.Surname;
    string name = fioInfo.Name;
    string patronymic = fioInfo.Patronymic;
}

// Очистить и проверить телефон
var phoneInfo = await dadata.CleanPhoneAsync("+79991234567");
if (phoneInfo.IsValid)
{
    string formatted = phoneInfo.Formatted;
    string country = phoneInfo.Country;
    string region = phoneInfo.Region;
}
```

---

## Структуры данных

### AddressInfo
```csharp
public class AddressInfo
{
    public string FullAddress { get; set; }   // Полный адрес
    public string Region { get; set; }        // Регион
    public string City { get; set; }          // Город
    public string Street { get; set; }        // Улица
    public string House { get; set; }         // Дом
    public string PostalCode { get; set; }    // Индекс
    public bool IsValid { get; set; }         // Валиден ли адрес
}
```

### FioInfo
```csharp
public class FioInfo
{
    public string FullName { get; set; }      // Полное ФИО
    public string Surname { get; set; }       // Фамилия
    public string Name { get; set; }          // Имя
    public string Patronymic { get; set; }    // Отчество
    public bool IsValid { get; set; }         // Валидно ли ФИО
}
```

### PhoneInfo
```csharp
public class PhoneInfo
{
    public string PhoneNumber { get; set; }   // Исходный номер
    public string Country { get; set; }       // Страна
    public string Region { get; set; }        // Регион
    public string City { get; set; }          // Город
    public string Formatted { get; set; }     // Отформатированный номер
    public bool IsValid { get; set; }         // Валиден ли номер
}
```

---

## Тарифы DaData

Бесплатный тариф включает:
- 10 000 запросов в сутки
- Подсказки (Suggest API)
- Очистка данных (Clean API) - 100 запросов в сутки

Подробнее: https://dadata.ru/pricing/

---

## Возможные проблемы и решения

### 1. "Неверный API-ключ"
- Проверьте, что ключ скопирован без лишних пробелов
- Убедитесь, что аккаунт активирован

### 2. "Превышен лимит запросов"
- Бесплатный тариф имеет ограничение 10 000 запросов/сутки
- Рассмотрите платный тариф при необходимости

### 3. "Подсказки не появляются"
- Проверьте интернет-соединение
- Убедитесь, что введено минимум 2 символа
- Проверьте Debug Output на наличие ошибок

### 4. "Popup не отображается"
- Убедитесь, что TextBox имеет фокус
- Проверьте, что Popup не перекрывается другими элементами

---

## Дополнительные возможности

### Кастомизация внешнего вида Popup

В файле `DadataTextBoxHelper.cs` можно изменить стиль ListBox:

```csharp
_listBox = new ListBox
{
    Width = _textBox.ActualWidth > 0 ? _textBox.ActualWidth : 300,
    MaxHeight = 200,
    BorderThickness = new Thickness(1),
    BorderBrush = System.Windows.Media.Brushes.Gray,
    Background = System.Windows.Media.Brushes.White,
    FontSize = 14
};
```

### Изменение задержки перед запросом

По умолчанию запрос отправляется сразу при изменении текста. 
Для добавления задержки (debounce) можно использовать Timer.

---

## Ссылки

- Документация DaData: https://dadata.ru/api/
- GitHub библиотека: https://github.com/hflabs/dadata-csharp
- Личный кабинет: https://dadata.ru/api/key/
