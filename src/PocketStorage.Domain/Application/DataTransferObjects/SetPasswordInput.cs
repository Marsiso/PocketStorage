namespace PocketStorage.Domain.Application.DataTransferObjects;

public class SetPasswordInput
{
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}
