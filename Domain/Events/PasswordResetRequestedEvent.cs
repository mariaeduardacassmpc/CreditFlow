namespace Domain.Events;

public class PasswordResetRequestedEvent
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}