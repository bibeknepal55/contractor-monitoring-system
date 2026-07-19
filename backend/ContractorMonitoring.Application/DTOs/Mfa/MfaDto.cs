namespace ContractorMonitoring.Application.DTOs.Mfa;

public class MfaSetupDto
{
    public string Password { get; set; } = string.Empty;
}

public class MfaSetupResponseDto
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public List<string> BackupCodes { get; set; } = new();
}

public class MfaVerifyDto
{
    public string Code { get; set; } = string.Empty;
}

public class MfaDisableDto
{
    public string Password { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}