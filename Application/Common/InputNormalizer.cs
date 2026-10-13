namespace Application.Common;

public static class InputNormalizer
{
    public static string Text(string value) => value.Trim();
    public static string Email(string value) => value.Trim().ToLowerInvariant();
    public static string Cpf(string value) => new(value.Where(char.IsDigit).ToArray());
}