namespace PocketStorage.Domain.Application.DataTransferObjects;

public class ResetPasswordInput
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Code { get; set; }
}
