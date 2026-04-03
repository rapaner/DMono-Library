using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;

namespace Library.ViewModels;

public partial class EditAuthorViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAuthorService _authorService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private Author? _author;

    [ObservableProperty]
    private string _name = string.Empty;

    public EditAuthorViewModel(IAuthorService authorService, INavigationService navigation, IDialogService dialog)
    {
        _authorService = authorService;
        _navigation = navigation;
        _dialog = dialog;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("authorId", out var id))
        {
            int authorId;
            if (id is int intId) authorId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId)) authorId = parsedId;
            else return;

            _ = LoadAuthorAsync(authorId);
        }
    }

    private async Task LoadAuthorAsync(int authorId)
    {
        _author = await _authorService.GetAuthorByIdAsync(authorId);
        if (_author != null)
        {
            Name = _author.Name;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await _dialog.ShowAlertAsync("Ошибка", "Пожалуйста, введите имя автора", "OK");
            return;
        }

        if (_author == null) return;

        try
        {
            _author.Name = Name.Trim();
            await _authorService.UpdateAuthorAsync(_author);
            await _dialog.ShowAlertAsync("Успех", "Автор обновлён!", "OK");
            await _navigation.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_author == null) return;

        var canDelete = await _authorService.CanDeleteAuthorAsync(_author.Id);
        if (!canDelete)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Невозможно удалить автора, у которого есть книги. Сначала удалите автора из всех книг.", "OK");
            return;
        }

        bool result = await _dialog.ShowConfirmAsync("Подтверждение",
            $"Вы уверены, что хотите удалить автора \"{_author.Name}\"?",
            "Да", "Нет");

        if (result)
        {
            await _authorService.DeleteAuthorAsync(_author);
            await _dialog.ShowAlertAsync("Успех", "Автор удалён!", "OK");
            await _navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.GoBackAsync();
    }
}
