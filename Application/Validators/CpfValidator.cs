namespace Application.Validators;

public static class CpfValidator
{
	public static bool IsValid(string? cpf)
	{
		if (string.IsNullOrWhiteSpace(cpf))
			return false;

		var digits = new string(cpf.Where(char.IsDigit).ToArray());

		if (digits.Length != 11 || digits.Distinct().Count() == 1)
			return false;

		var numbers = digits.Select(c => c - '0').ToArray();

		var sum = 0;

		for (var i = 0; i < 9; i++)
			sum += numbers[i] * (10 - i);

		var firstDigit = sum % 11 < 2 ? 0 : 11 - sum % 11;

		if (numbers[9] != firstDigit)
			return false;

		sum = 0;

		for (var i = 0; i < 10; i++)
			sum += numbers[i] * (11 - i);

		var secondDigit = sum % 11 < 2 ? 0 : 11 - sum % 11;

		return numbers[10] == secondDigit;
	}
}