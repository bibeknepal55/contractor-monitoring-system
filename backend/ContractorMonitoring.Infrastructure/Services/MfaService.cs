using System.Security.Cryptography;
using System.Text;
using OtpNet;
using QRCoder;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Infrastructure.Services;

public class MfaService : IMfaService
{
    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "ContractorMonitoring")
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public string GenerateQrCodeBase64(string email, string secret)
    {
        var uri = GenerateQrCodeUri(email, secret);
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var bytes = qrCode.GetGraphic(5);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }

    public bool ValidateTotp(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6) return false;
        try
        {
            var key = Base32Encoding.ToBytes(secret);
            var totp = new Totp(key);
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch { return false; }
    }

    public string[] GenerateBackupCodes(int count = 8)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(5);
            codes[i] = Convert.ToHexString(bytes).ToLower();
        }
        return codes;
    }
}
