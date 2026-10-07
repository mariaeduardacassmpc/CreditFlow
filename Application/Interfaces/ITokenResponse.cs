using Domain.Entities;

namespace Application.Interfaces;
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
