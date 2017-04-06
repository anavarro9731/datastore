//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
//using DataStore.Interfaces;
//using DataStore.Interfaces.Events;
//using DataStore.Interfaces.LowLevel;
//using DataStore.Models;
//using DataStore.Models.Messages.Events;
//using DataStore.Models.PureFunctions.Extensions;
//using Newtonsoft.Json;

//namespace DataStore.Impl.SqlServer
//{
//    public class DocumentRepository : IDocumentRepository
//    {
//        private readonly SqlServerDbClientFactory clientFactory;

//        public DocumentRepository(SqlServerDbSettings settings)
//        {
//            clientFactory = new SqlServerDbClientFactory(settings);
//            SqlServerDbInitialiser.Initialise(clientFactory);
//        }

//        #region IDocumentRepository Members

//        public async Task AddAsync<T>(IDataStoreWriteEvent<T> aggregateAdded) where T : IAggregate
//        {
//            if (aggregateAdded == null || aggregateAdded.Model == null)
//                throw new ArgumentNullException(nameof(aggregateAdded));

//            var stopWatch = Stopwatch.StartNew();

//            using (var con = clientFactory.GetClient())
//            {
//                con.Open();

//                using (var command = new SqlCommand(
//                    $"INSERT INTO {SqlServerDbSettings.SqlServerAggregatesTableName} VALUES(@AggregatedId, @Schema, @Json)", con))
//                {
//                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateAdded.Model.id));

//                    command.Parameters.Add(new SqlParameter("@Schema", aggregateAdded.Model.schema));

//                    var json = JsonConvert.SerializeObject(aggregateAdded.Model);
//                    command.Parameters.Add(new SqlParameter("Json", json));
                    
//                    command.ExecuteNonQuery();
//                }
//            }
            
//            stopWatch.Stop();
//            aggregateAdded.StateOperationDuration = stopWatch.Elapsed;
//            await Task.Delay(0);
//        }

//        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
//        {
//            var schema = typeof(T).FullName;
            
//            var query = new List<T>();
//            using (SqlConnection connection = clientFactory.GetClient())
//            {
//                connection.Open();

//                using (SqlCommand command = new SqlCommand($"SELECT * FROM DataStoreAggregates WHERE Schema = {schema}", connection))
//                {
//                    SqlDataReader reader = command.ExecuteReader();
//                    while (reader.Read())
//                    {
//                        string json = reader.GetString(2); 

//                        query.Add(JsonConvert.DeserializeObject<T>(json));                        
//                    }
//                }
//            }
//            return query.AsQueryable();
//        }

//        public async Task DeleteHardAsync<T>(IDataStoreWriteEvent<T> aggregateHardDeleted) where T : IAggregate
//        {
//            var stopWatch = Stopwatch.StartNew();


//            using (var con = clientFactory.GetClient())
//            {
//                con.Open();

//                using (var command = new SqlCommand(
//                    $"DELETE FROM {SqlServerDbSettings.SqlServerAggregatesTableName} WHERE AggregatedId = @AggregatedId", con))
//                {
//                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateHardDeleted.Model.id));

//                    command.ExecuteNonQuery();
//                }
//            }

//            stopWatch.Stop();
//            aggregateHardDeleted.StateOperationDuration = stopWatch.Elapsed;
//            await Task.Delay(0);
//        }

//        public async Task DeleteSoftAsync<T>(IDataStoreWriteEvent<T> aggregateSoftDeleted) where T : IAggregate
//        {
//            var stopWatch = Stopwatch.StartNew();
//            using (var connection = clientFactory.GetClient())
//            {
//                connection.Open();

//                using (var command = new SqlCommand(
//                    $"UPDATE {SqlServerDbSettings.SqlServerAggregatesTableName} SET Json = @Json WHERE AggregatedId = @AggregateId", connection))
//                {
//                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateSoftDeleted.Model.id));

//                    var now = DateTime.UtcNow;
//                    aggregateSoftDeleted.Model.Modified = now;
//                    aggregateSoftDeleted.Model.ModifiedAsMillisecondsEpochTime = now.ConvertToMillisecondsEpochTime();
//                    aggregateSoftDeleted.Model.Active = false;
//                    var json = JsonConvert.SerializeObject(aggregateSoftDeleted.Model);
//                    command.Parameters.Add(new SqlParameter("Json", json));

//                    command.ExecuteNonQuery();
//                }
//            }
//            stopWatch.Stop();
//            aggregateSoftDeleted.StateOperationDuration = stopWatch.Elapsed;
//            await Task.Delay(0);
//        }

        
//        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
//        {
//            var stopWatch = Stopwatch.StartNew();

//            var results = aggregatesQueried.Query.ToList();
                
//            stopWatch.Stop();

//            aggregatesQueried.StateOperationDuration = stopWatch.Elapsed;

//            await Task.Delay(0);

//            return results;
//        }

//        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
//        {
//            var result = await GetItemAsync(aggregateQueriedById);
//            return (T) result;
//        }

//        public async Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
//        {
//            var stopWatch = Stopwatch.StartNew();

//            var id = aggregateQueriedById.Id;

//            using (SqlConnection connection = clientFactory.GetClient())
//            {
//                connection.Open();

//                using (SqlCommand command = new SqlCommand($"SELECT * FROM DataStoreAggregates WHERE Id = {id}", connection))
//                {

//                    SqlDataReader reader = command.ExecuteReader();
//                    if (!reader.HasRows)
//                    {
//                        throw new DatabaseRecordNotFoundException(id.ToString());
//                    }
//                    reader.Read();
                    
//                    string json = reader.GetString(2);
//                    query.Add(JsonConvert.DeserializeObject<T>(json));
                    
//                }
//            }

//            stopWatch.Stop();
//            aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;
            
//        }

//        public async Task UpdateAsync<T>(IDataStoreWriteEvent<T> aggregateUpdated) where T : IAggregate
//        {
//            var stopWatch = Stopwatch.StartNew();

//            using (var connection = clientFactory.GetClient())
//            {
//                connection.Open();

//                using (var command = new SqlCommand(
//                    $"UPDATE {SqlServerDbSettings.SqlServerAggregatesTableName} SET Json = @Json WHERE AggregatedId = @AggregateId", connection))
//                {
//                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateUpdated.Model.id));

//                    var json = JsonConvert.SerializeObject(aggregateUpdated.Model);
//                    command.Parameters.Add(new SqlParameter("Json", json));

//                    command.ExecuteNonQuery();
//                }
//            }

//            stopWatch.Stop();
//            aggregateUpdated.StateOperationDuration = stopWatch.Elapsed;
//        }

//        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
//        {
//            var stopWatch = Stopwatch.StartNew();
//            var query =
//                documentClient.CreateDocumentQuery(config.CollectionSelfLink())
//                    .Where(item => item.Id == aggregateQueriedById.Id.ToString())
//                    .AsDocumentQuery();

//            var results = await query.ExecuteNextAsync();

//            stopWatch.Stop();
//            aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;

//            return results.Count > 0;
//        }

//        public void Dispose()
//        {
//        }

//        #endregion
//    }
//}