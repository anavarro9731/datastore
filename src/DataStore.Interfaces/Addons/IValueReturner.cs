namespace DataStore.DataAccess.Interfaces.Addons
{
    public interface IValueReturner
    {
        void Return<TReturnValue>(TReturnValue returnValue);
    }
}