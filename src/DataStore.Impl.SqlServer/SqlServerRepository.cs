using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.Events;
using DataStore.Interfaces.LowLevel;
using DataStore.Models;
using DataStore.Models.PureFunctions.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerRepository : IDocumentRepository
    {
        private readonly SqlServerDbClientFactory clientFactory;

        public SqlServerRepository(SqlServerDbSettings settings)
        {
            clientFactory = new SqlServerDbClientFactory(settings);
            SqlServerDbInitialiser.Initialise(clientFactory);
        }

        #region IDocumentRepository Members

        public async Task AddAsync<T>(IDataStoreWriteEvent<T> aggregateAdded) where T : IAggregate
        {
            var stopWatch = Stopwatch.StartNew();

            using (var con = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"INSERT INTO {SqlServerDbSettings.SqlServerAggregatesTableName} VALUES(Convert(uniqueidentifier, @AggregateId), @Schema, @Json)", con))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateAdded.Model.id));

                    command.Parameters.Add(new SqlParameter("Schema", aggregateAdded.Model.schema));

                    var json = JsonConvert.SerializeObject(aggregateAdded.Model);
                    command.Parameters.Add(new SqlParameter("Json", json));

                    await command.ExecuteNonQueryAsync();
                }
            }

            stopWatch.Stop();
            aggregateAdded.StateOperationDuration = stopWatch.Elapsed;
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            var schema = typeof(T).FullName;

            var query = new List<T>();
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand($"SELECT * FROM DataStoreAggregates WHERE [Schema] = '{schema}'", connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var json = reader.GetString(2);

                        query.Add(JsonConvert.DeserializeObject<T>(json));
                    }
                }
            }
            return query.AsQueryable();
        }

        public async Task DeleteHardAsync<T>(IDataStoreWriteEvent<T> aggregateHardDeleted) where T : IAggregate
        {
            var stopWatch = Stopwatch.StartNew();


            using (var con = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"DELETE FROM {SqlServerDbSettings.SqlServerAggregatesTableName} WHERE AggregateId = CONVERT(uniqueidentifier, @AggregateId)", con))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateHardDeleted.Model.id));

                    await command.ExecuteNonQueryAsync();
                }
            }

            stopWatch.Stop();
            aggregateHardDeleted.StateOperationDuration = stopWatch.Elapsed;
        }

        public async Task DeleteSoftAsync<T>(IDataStoreWriteEvent<T> aggregateSoftDeleted) where T : IAggregate
        {
            var stopWatch = Stopwatch.StartNew();
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"UPDATE {SqlServerDbSettings.SqlServerAggregatesTableName} SET Json = @Json WHERE AggregateId = CONVERT(uniqueidentifier, @AggregateId)",
                    connection))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateSoftDeleted.Model.id));

                    var now = DateTime.UtcNow;
                    aggregateSoftDeleted.Model.Modified = now;
                    aggregateSoftDeleted.Model.ModifiedAsMillisecondsEpochTime = now.ConvertToMillisecondsEpochTime();
                    aggregateSoftDeleted.Model.Active = false;
                    var json = JsonConvert.SerializeObject(aggregateSoftDeleted.Model);
                    command.Parameters.Add(new SqlParameter("Json", json));

                    await command.ExecuteNonQueryAsync();
                }
            }
            stopWatch.Stop();
            aggregateSoftDeleted.StateOperationDuration = stopWatch.Elapsed;
        }


        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var stopWatch = Stopwatch.StartNew();

            var results = aggregatesQueried.Query.ToList();

            stopWatch.Stop();

            aggregatesQueried.StateOperationDuration = stopWatch.Elapsed;

            await Task.Delay(0);

            return results;
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
        {
            var stopWatch = Stopwatch.StartNew();

            var id = aggregateQueriedById.Id;

            T result;
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand($"SELECT Json FROM DataStoreAggregates WHERE AggregateId = CONVERT(uniqueidentifier, '{id}')", connection))
                {
                    var response = await command.ExecuteScalarAsync() as string;

                    if (response == null) throw new DatabaseRecordNotFoundException(id.ToString());

                    result = JsonConvert.DeserializeObject<T>(response);
                }
            }

            stopWatch.Stop();
            aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;
            return result;
        }

        public async Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            var stopWatch = Stopwatch.StartNew();

            var id = aggregateQueriedById.Id;

            dynamic result;
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand($"SELECT Json FROM DataStoreAggregates WHERE AggregateId = CONVERT(uniqueidentifier, '{id}')", connection))
                {
                    var response = await command.ExecuteScalarAsync() as string;

                    if (response == null) throw new DatabaseRecordNotFoundException(id.ToString());

                    result = JsonConvert.DeserializeObject<dynamic>(response);
                }
            }

            stopWatch.Stop();
            aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;
            return result;
        }

        public async Task UpdateAsync<T>(IDataStoreWriteEvent<T> aggregateUpdated) where T : IAggregate
        {
            var stopWatch = Stopwatch.StartNew();

            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"UPDATE {SqlServerDbSettings.SqlServerAggregatesTableName} SET Json = @Json WHERE AggregateId = CONVERT(uniqueidentifier, @AggregateId)",
                    connection))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateUpdated.Model.id));

                    var json = JsonConvert.SerializeObject(aggregateUpdated.Model);
                    command.Parameters.Add(new SqlParameter("Json", json));

                    await command.ExecuteNonQueryAsync();
                }
            }

            stopWatch.Stop();
            aggregateUpdated.StateOperationDuration = stopWatch.Elapsed;
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            var stopWatch = Stopwatch.StartNew();

            var id = aggregateQueriedById.Id;

            string result;
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand($"SELECT AggregateId FROM DataStoreAggregates WHERE AggregateId = CONVERT(uniqueidentifier, '{id}')", connection))
                {
                    result = (await command.ExecuteScalarAsync())?.ToString();
                }
            }

            stopWatch.Stop();
            aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;
            return result != null;
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        #endregion
    }
}