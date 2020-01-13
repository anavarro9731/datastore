namespace DataStore.Options
{
    using global::DataStore.Interfaces;

    public class UpdateOptions : IUpdateOptions
    {
        private bool optimisticConcurrency = true;

        bool IUpdateOptionsRepoSide.OptimisticConcurrency => this.optimisticConcurrency;

        public void DisableOptimisticConcurrecy()
        {
            this.optimisticConcurrency = false;
        }
    }
}