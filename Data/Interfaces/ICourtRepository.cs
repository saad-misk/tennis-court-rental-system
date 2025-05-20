using System.Collections;
using TennisCourtRentalSystem.Models;

namespace TennisCourtRentalSystem.Data.Interfaces
{
    public interface ICourtRepository
    {
        Task<IEnumerable> GetAllCourtsAsync();
        Task<Court> GetCourtByNumberAsync(int courtNumber);
        Task<IEnumerable> GetAvailableCourtsAsync();
        // Other methods...
    }
}
