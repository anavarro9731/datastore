using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.LowLevel;
using DataStore.Models.PureFunctions.Extensions;
using Newtonsoft.Json;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerRepository : IDocumentRepository
    {
        private readonly SqlServerDbClientFactory clientFactory;
        private readonly SqlServerDbSettings settings;

        public SqlServerRepository(SqlServerDbSettings settings)
        {
            this.settings = settings;
            clientFactory = new SqlServerDbClientFactory(settings);
            SqlServerDbInitialiser.Initialise(clientFactory, settings);
        }

        #region

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            using (var con = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"INSERT INTO {settings.TableName} ([AggregateId], [Schema], [Json]) VALUES(Convert(uniqueidentifier, @AggregateId), @Schema, @Json)",
                    con))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateAdded.Model.id));

                    command.Parameters.Add(new SqlParameter("Schema", aggregateAdded.Model.schema));

                    var json = JsonConvert.SerializeObject(aggregateAdded.Model);
                    command.Parameters.Add(new SqlParameter("Json", json));

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : class, IAggregate, new()
        {
            var schema = typeof(T).FullName;

            var query = new List<T>();
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand($"SELECT Json FROM {settings.TableName} WHERE [Schema] = '{schema}'",
                    connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var json = reader.GetString(0);

                            query.Add(JsonConvert.DeserializeObject<T>(json));
                        }
                    }
                }
            }
            return query.AsQueryable();
        }

        public async Task DeleteHardAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            using (var con = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"DELETE FROM {settings.TableName} WHERE AggregateId = CONVERT(uniqueidentifier, @AggregateId)", con))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateHardDeleted.Model.id));

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

        }

        public async Task DeleteSoftAsync<T>(IDataStoreWriteOperation<T> aggregateSoftDeleted) where T : class, IAggregate, new()
        {
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"UPDATE {settings.TableName} SET Json = @Json WHERE AggregateId = CONVERT(uniqueidentifier, @AggregateId)",
                    connection))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateSoftDeleted.Model.id));

                    var now = DateTime.UtcNow;
                    aggregateSoftDeleted.Model.Modified = now;
                    aggregateSoftDeleted.Model.ModifiedAsMillisecondsEpochTime = now.ConvertToMillisecondsEpochTime();
                    aggregateSoftDeleted.Model.Active = false;
                    var json = JsonConvert.SerializeObject(aggregateSoftDeleted.Model);
                    command.Parameters.Add(new SqlParameter("Json", json));

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var results = aggregatesQueried.Query.ToList();

            return Task.FromResult(results.AsEnumerable());
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            // NOTE: SqlCommand.ExecuteScalarAsync() has severe performance issues when 
            //       retrieving large recordsets, therefore we use the sync implementation.

            var result = GetItem<T>(aggregateQueriedById);
            return Task.FromResult(result);
        }

        private T GetItem<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var id = aggregateQueriedById.Id;

            T result;
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"SELECT Json FROM {settings.TableName} WHERE AggregateId = CONVERT(uniqueidentifier, '{id}')", connection))
                {
                    var response = command.ExecuteScalar() as string;

                    result = response == null ? null : JsonConvert.DeserializeObject<T>(response);
                }
            }
            return result;
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"UPDATE {settings.TableName} SET Json = @Json WHERE AggregateId = CONVERT(uniqueidentifier, @AggregateId)",
                    connection))
                {
                    command.Parameters.Add(new SqlParameter("AggregateId", aggregateUpdated.Model.id));

                    var json = JsonConvert.SerializeObject(aggregateUpdated.Model);
                    command.Parameters.Add(new SqlParameter("Json", json));

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            var id = aggregateQueriedById.Id;

            string result;
            using (var connection = clientFactory.OpenClient())
            {
                using (var command = new SqlCommand(
                    $"SELECT AggregateId FROM {settings.TableName} WHERE AggregateId = CONVERT(uniqueidentifier, '{id}')",
                    connection))
                {
                    result = (await command.ExecuteScalarAsync().ConfigureAwait(false))?.ToString();
                }
            }

            return result != null;
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        #endregion
    }
}