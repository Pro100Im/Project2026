namespace Code.Infrastructure.Persistence
{
    public interface IStorageService
    {
        bool TryLoad<T>(string key, out T data) where T : class;
        void Save<T>(string key, T data) where T : class;
    }
}
