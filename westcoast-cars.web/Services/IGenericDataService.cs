namespace westcoast_cars.web.Services;

public interface IGenericDataService<TList, TPost> where TList : class where TPost : class
{
    Task<IList<TList>> ListAllAsync();
    Task<bool> CreateAsync(TPost model);
    Task<bool> DeleteAsync(int id);
}