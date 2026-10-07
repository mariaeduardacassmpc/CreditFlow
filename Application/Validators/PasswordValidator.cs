namespace Application.Services;

public static class PasswordValidator
{
    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("A senha é obrigatória.");

        if (password.Length < 8)
            throw new ArgumentException(
                "A senha deve ter no mínimo 8 caracteres.");

        if (!password.Any(char.IsUpper))
            throw new ArgumentException(
                "A senha deve conter pelo menos uma letra maiúscula.");

        if (!password.Any(char.IsLower))
            throw new ArgumentException(
                "A senha deve conter pelo menos uma letra minúscula.");

        if (!password.Any(char.IsDigit))
            throw new ArgumentException(
                "A senha deve conter pelo menos um número.");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            throw new ArgumentException(
                "A senha deve conter pelo menos um caractere especial.");
    }
}