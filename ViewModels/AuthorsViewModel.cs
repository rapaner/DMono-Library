using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class AuthorsViewModel : ObservableObject
{
    private readonly IAuthorService _authorService;
    private readonly INavigationService _navigation;

    private List<AuthorWithBookCount> _allAuthors = new();

    [ObservableProperty]
    private ObservableCollection<AuthorWithBookCount> _authors = new();

    [ObservableProperty]
    private int _selectedSortIndex;

    public AuthorsViewModel(IAuthorService authorService, INavigationService navigation)
    {
        _authorService = authorService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        _allAuthors = await _authorService.GetAuthorsWithBookCountAsync();
        ApplySort();
    }

    partial void OnSelectedSortIndexChanged(int value)
    {
        ApplySort();
    }

    private void ApplySort()
    {
        var sorted = SelectedSortIndex switch
        {
            1 => _allAuthors.OrderByDescending(a => a.BookCount).ThenBy(a => a.Name),
            _ => _allAuthors.OrderBy(a => a.Name)
        };

        Authors.Clear();
        foreach (var author in sorted)
            Authors.Add(author);
    }

    [RelayCommand]
    private async Task GoToEditAuthorAsync(AuthorWithBookCount? author)
    {
        if (author == null) return;
        await _navigation.GoToAsync($"{nameof(EditAuthorPage)}?authorId={author.Id}");
    }
}
