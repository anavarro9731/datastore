namespace Infrastructure.Messages.Events
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