namespace PocketStorage.Domain.Application.DataTransferObjects;

public class LoginWithTwoFactorAuthInput
{
    public string? TwoFactorCode { get; set; }
    public bool RememberMachine { get; set; }
}
