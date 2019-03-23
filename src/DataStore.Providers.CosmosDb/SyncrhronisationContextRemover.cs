namespace DataStore.Providers.CosmosDb
{
    namespace ExtremeConfigAwait
    {
        using System;
        using System.Runtime.CompilerServices;
        using System.Threading;

        public struct SynchronizationContextRemover : INotifyCompletion
        {
            public bool IsCompleted => SynchronizationContext.Current == null;

            public void OnCompleted(Action continuation)
            {
                var prevContext = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                    continuation();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prevContext);
                }
            }

            public SynchronizationContextRemover GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
            }
        }
    }
}