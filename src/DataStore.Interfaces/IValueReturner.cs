namespace DataStore.Interfaces
{
    public interface IValueReturner
    {
        void Return<TReturnValue>(TReturnValue returnValue);
    }
}