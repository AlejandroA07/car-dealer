
using System.Collections.Generic;
using System.Threading.Tasks;

namespace westcoast_cars.web.Services
{
    public interface IGenericDataService<TListViewModel, TPostViewModel>
    {
        Task<IList<TListViewModel>> ListAllAsync();
        Task<bool> CreateAsync(TPostViewModel model);
        Task<bool> DeleteAsync(int id);
    }
}
