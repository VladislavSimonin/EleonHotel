using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Media;

public class RegistrationViewModel : INotifyPropertyChanged
{
    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            if (_password == value) return;
            _password = value;
            OnPropertyChanged();
            EvaluateStrength();
        }
    }

    // Выводимые в UI свойства
    public string StrengthText { get; private set; } = "Введите пароль";
    public Brush StrengthBrush { get; private set; } = Brushes.Gray;
    public double StrengthPercent { get; private set; }

    // Флаги правил (для чек-листа)
    public bool IsLongEnough { get; private set; }
    public bool HasUpper { get; private set; }
    public bool HasLower { get; private set; }
    public bool HasDigit { get; private set; }
    public bool HasSpecial { get; private set; }

    private void EvaluateStrength()
    {
        int score = 0;
        string pwd = Password ?? string.Empty;

        IsLongEnough = pwd.Length >= 8; 
        HasUpper = Regex.IsMatch(pwd, @"[A-ZА-ЯЁ]");
        HasLower = Regex.IsMatch(pwd, @"[a-zа-яё]");
        HasDigit = Regex.IsMatch(pwd, @"\d");
        HasSpecial = Regex.IsMatch(pwd, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        if (IsLongEnough) score++;
        if (HasUpper) score++;
        if (HasLower) score++;
        if (HasDigit) score++;
        if (HasSpecial) score++;

        StrengthPercent = (score / 5.0) * 100;

        switch (score)
        {
            case 0:
            case 1:
                StrengthText = "Очень слабый"; StrengthBrush = Brushes.Red; break;
            case 2:
                StrengthText = "Слабый"; StrengthBrush = Brushes.Orange; break;
            case 3:
                StrengthText = "Средний"; StrengthBrush = Brushes.Goldenrod; break;
            case 4:
                StrengthText = "Хороший"; StrengthBrush = Brushes.LimeGreen; break;
            case 5:
                StrengthText = "Отличный"; StrengthBrush = Brushes.ForestGreen; break;
        }

        OnPropertyChanged(nameof(StrengthText));
        OnPropertyChanged(nameof(StrengthBrush));
        OnPropertyChanged(nameof(StrengthPercent));
        OnPropertyChanged(nameof(IsLongEnough));
        OnPropertyChanged(nameof(HasUpper));
        OnPropertyChanged(nameof(HasLower));
        OnPropertyChanged(nameof(HasDigit));
        OnPropertyChanged(nameof(HasSpecial));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
