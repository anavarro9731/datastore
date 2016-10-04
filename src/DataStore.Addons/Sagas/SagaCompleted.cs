﻿namespace DataStore.Infrastructure.Impl.Sagas
{
    using global::DataStore.Messages;

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