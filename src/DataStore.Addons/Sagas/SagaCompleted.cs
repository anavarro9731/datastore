using DataStore.DataAccess.Models.Messages;

namespace DataStore.Infrastructure.Impl.Sagas
{
    public class SagaCompleted : Event
    {
    }

    public class SagaCompleted<T> : Event<T>
    {
        public SagaCompleted(T model)
            : base(model)
        {
        }
    }
}