namespace Domain.Entities;

public class User
{
    public int UserId { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public bool Active { get; set; } = true;
}