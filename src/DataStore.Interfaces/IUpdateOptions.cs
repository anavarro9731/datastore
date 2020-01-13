namespace DataStore.Interfaces
{
    public interface IUpdateOptions : IUpdateOptionsRepoSide, IUpdateOptionsClientSide
    {
        //- here to allow referencing in IUpdateCapabilities
    }

    //- here to clarify the two sides, implement explicitly to hide from client side
    public interface IUpdateOptionsRepoSide 
    {
        bool OptimisticConcurrency { get; }
    }

    public interface IUpdateOptionsClientSide 
    {
        void DisableOptimisticConcurrecy();
    }
}