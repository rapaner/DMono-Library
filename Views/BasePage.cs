namespace Library.Views;

public class BasePage : ContentPage
{
    protected async void SafeExecute(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
    }
}
