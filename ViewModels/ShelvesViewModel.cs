using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class ShelvesViewModel : ObservableObject
{
    private readonly IShelfService _shelfService;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private ObservableCollection<ShelfWithBookCount> _shelves = new();

    public ShelvesViewModel(IShelfService shelfService, INavigationService navigation)
    {
        _shelfService = shelfService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var shelvesWithCount = await _shelfService.GetShelvesWithBookCountAsync();
        Shelves.Clear();
        foreach (var shelf in shelvesWithCount)
            Shelves.Add(shelf);
    }

    [RelayCommand]
    private async Task GoToAddShelfAsync()
    {
        await _navigation.GoToAsync(nameof(AddEditShelfPage));
    }

    [RelayCommand]
    private async Task GoToEditShelfAsync(ShelfWithBookCount? shelf)
    {
        if (shelf == null) return;
        await _navigation.GoToAsync($"{nameof(AddEditShelfPage)}?shelfId={shelf.Id}");
    }
}
