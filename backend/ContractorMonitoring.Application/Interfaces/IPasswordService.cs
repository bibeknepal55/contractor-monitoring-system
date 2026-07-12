namespace ContractorMonitoring.Application.Interfaces;

// Password hashing service interface
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}