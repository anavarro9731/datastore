namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DataAccess.Interfaces;

    using Finygo.DocumentDb;

    using Infrastructure.HandlerServiceInterfaces;
    using Infrastructure.PureFunctions;

    using Microsoft.Azure.Documents;

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection)
        {
            this.DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;
            return await this.DbConnection.Exists(id);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : IAggregate
        {
            var queryable = this.DbConnection.CreateDocumentQuery<T>();
            if (queryableExtension != null)
            {
                queryable = queryableExtension(queryable);
            }

            var results = await this.DbConnection.ExecuteQuery(queryable);
            return results;
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null, 
            bool includeHidden = false) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> queryableExtension2 = (q) =>
                {
                    if (queryableExtension != null)
                    {
                        q = queryableExtension(q);
                    }

                    q = q.Where(a => a.Active);
                    if (!includeHidden)
                    {
                        q = q.Where(a => !a.Hidden);
                    }

                    return q;
                };
            return await this.Read<T>(queryableExtension2);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = (q) => q.Where(a => a.id == modelId && a.Active);
            var results = await this.Read<T>(queryableExtension);
            return results.Single();
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<Document> ReadById(Guid modelId)
        {
            var result = await this.DbConnection.GetItemAsync(modelId);
            return result;
        }
    }
}