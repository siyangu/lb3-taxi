using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TaxiService.Auth.Services;
public record AuthResult(bool Success, string Message);
public interface IAuthService
{
    AuthResult RegisterDriver(string callsign, string password);
    AuthResult LoginDriver(string callsign, string password);
    AuthResult ChangeDriverPassword(string callsign, string oldPassword, string newPassword);
    IReadOnlyList<string> GetActiveDrivers();
    void ClearDriverDatabase();
}
public class AuthService : IAuthService
{
    // Хранилище: {позывной_водителя: хэш_пароля}
    private readonly ConcurrentDictionary<string, string> _drivers = new();

    private static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
    private static bool IsValidCallsign(string callsign) =>
        Regex.IsMatch(callsign, @"^[a-zA-Z0-9_]{3,20}$");

    private static bool IsValidPassword(string password) =>
        password.Length >= 6;
    public AuthResult RegisterDriver(string callsign, string password)
    {
        if (string.IsNullOrEmpty(callsign) || string.IsNullOrEmpty(password))
            return new AuthResult(false, "Позывной и пароль обязательны");

        if (!IsValidCallsign(callsign))
            return new AuthResult(false, "Позывной: 3–20 символов, только буквы и цифры");

        if (!IsValidPassword(password))
            return new AuthResult(false, "Пароль: минимум 6 символов");

        if (_drivers.ContainsKey(callsign))
            return new AuthResult(false, "Водитель с таким позывным уже в штате");

        _drivers[callsign] = Hash(password);
        return new AuthResult(true, "Водитель успешно зарегистрирован");
    }
    public AuthResult LoginDriver(string callsign, string password)
    {
        if (!_drivers.TryGetValue(callsign, out var stored) || stored != Hash(password))
            return new AuthResult(false, "Неверный позывной или пароль");

        return new AuthResult(true, $"Добро пожаловать в смену, {callsign}!");
    }
    public AuthResult ChangeDriverPassword(string callsign, string oldPassword, string newPassword)
    {
        if (!_drivers.TryGetValue(callsign, out var stored))
            return new AuthResult(false, "Водитель не найден");

        if (stored != Hash(oldPassword))
            return new AuthResult(false, "Текущий пароль неверен");

        if (!IsValidPassword(newPassword))
            return new AuthResult(false, "Новый пароль слишком короткий");

        _drivers[callsign] = Hash(newPassword);
        return new AuthResult(true, "Пароль водителя обновлен");
    }
    public IReadOnlyList<string> GetActiveDrivers() => _drivers.Keys.ToList();
    public void ClearDriverDatabase() => _drivers.Clear();
}