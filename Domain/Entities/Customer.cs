namespace Domain.Entities;

public class Customer
{
    public int CustomerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public int CreditScore { get; private set; }
    public bool Active { get; private set; }

    private Customer() { }

    public static Customer Create(
        string name, string phone, string email, string cpf, DateOnly birthDate, int creditScore)
    {
        var customer = new Customer { Active = true };
        customer.SetData(name, phone, email, cpf, birthDate);
        customer.UpdateCreditScore(creditScore);
        return customer;
    }

    public void Update(string name, string phone, string email, string cpf, DateOnly birthDate)
        => SetData(name, phone, email, cpf, birthDate);

    public void UpdateCreditScore(int score)
    {
        if (score < 0)
            throw new ArgumentOutOfRangeException(nameof(score), "O score não pode ser negativo.");

        CreditScore = score;
    }

    public void SetActive(bool active) => Active = active;

    private void SetData(string name, string phone, string email, string cpf, DateOnly birthDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(cpf);

        Name = name;
        Phone = phone;
        Email = email;
        Cpf = cpf;
        BirthDate = birthDate;
    }
}