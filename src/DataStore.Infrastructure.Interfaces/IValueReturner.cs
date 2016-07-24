namespace Infrastructure.HandlerServiceInterfaces
{
    public interface IValueReturner
    {
        void Return<TReturnValue>(TReturnValue returnValue);
    }
}