namespace Library.Services;

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel);
    Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);
    Task<string?> ShowPromptAsync(string title, string message, string accept, string cancel);
}
