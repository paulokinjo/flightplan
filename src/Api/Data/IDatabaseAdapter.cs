namespace Api.Data
{
    public interface IDatabaseAdapter<T>
    {
        Task<List<T>> GetAll();
        Task<T> GetById(string id);
        Task<TransactionResult> UpdateById(string id, T data);
        Task<bool> DeleteById(string id);
    }
}
