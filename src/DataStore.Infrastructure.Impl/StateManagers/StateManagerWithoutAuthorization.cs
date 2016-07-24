namespace Infrastructure.HandlerServices.StateManagers
{
    using System;

    using Apogee.Azure.Website;
    using Apogee.Storage;

    using DataAccess.Interfaces;

    using DataStore;

    using Infrastructure.Configuration;
    using Infrastructure.HandlerServiceInterfaces;

    public class StateManagerWithoutAuthorization : IStateManagerWithoutAuthorization
    {
        public StateManagerWithoutAuthorization(IDocumentRepository repository, IFileStorageProvider fileStorage, IEventAggregator eventAggregator)
        {
            DocumentDbPrimary = new DataStore(repository, eventAggregator);
            FileStoragePrimary = fileStorage;
            TransactionId = Guid.NewGuid();
        }

        public IDataStore DocumentDbPrimary { get; }

        public Guid TransactionId { get; set; }

        public IFileStorageProvider FileStoragePrimary { get; }

        public virtual void Dispose()
        {
            this.DocumentDbPrimary.Dispose();
        }

        public virtual void SubmitChanges()
        {
            //this.DocumentDbPrimary.CommitChanges();
        }
    }
}