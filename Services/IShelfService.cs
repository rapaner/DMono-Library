using Library.Core.Models;
using Library.Models;

namespace Library.Services
{
    public interface IShelfService
    {
        Task<List<Shelf>> GetAllShelvesAsync();
        Task<Shelf?> GetShelfByIdAsync(int id);
        Task<List<ShelfWithBookCount>> GetShelvesWithBookCountAsync();
        Task<Shelf> AddShelfAsync(Shelf shelf);
        Task<Shelf> UpdateShelfAsync(Shelf shelf);
        Task<bool> DeleteShelfAsync(Shelf shelf);
        Task<bool> CanDeleteShelfAsync(int shelfId);
    }
}
