namespace Library.Services;

public class MauiDialogService : IDialogService
{
    private static Page CurrentPage =>
        Shell.Current as Page
        ?? Application.Current?.Windows[0].Page
        ?? throw new InvalidOperationException("No page available for displaying dialogs");

    public Task ShowAlertAsync(string title, string message, string cancel) =>
        CurrentPage.DisplayAlert(title, message, cancel);

    public Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel) =>
        CurrentPage.DisplayAlert(title, message, accept, cancel);

    public Task<string?> ShowPromptAsync(string title, string message, string accept, string cancel) =>
        CurrentPage.DisplayPromptAsync(title, message, accept, cancel);
}
