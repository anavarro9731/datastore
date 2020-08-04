namespace DataStore.Models
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Models.Messages;
    using DataStore.Models.PureFunctions.Extensions;

    public static class DocumentRepositoryExtensions
    {
        
        public static async Task<bool> Exists<T>(this IDocumentRepository repo, T model, string methodCalled = null) where T: IAggregate 
        {
            var type = model.GetType();

            var aggregateQueried = new AggregateQueriedByIdOperation(methodCalled, model.id);

            var getItemAsync = typeof(IDocumentRepository).GetMethod(nameof(IDocumentRepository.GetItemAsync)).MakeGenericMethod(type);
            
            var result = await (Task<T>)getItemAsync.InvokeAsync(repo, aggregateQueried);

            return result != null;

        }
        
        public static Task CreateAsync(this IDocumentRepository repo, IAggregate model, string methodCalled = null)
        {
            var type = model.GetType();

            var createOperationType = typeof(CreateOperation<>).MakeGenericType(type);

            var createAsync = typeof(IDocumentRepository).GetMethod(nameof(IDocumentRepository.AddAsync)).MakeGenericMethod(type);

            var createOperation = Activator.CreateInstance(createOperationType).As<IDataStoreWriteOperation>();

            createOperation.TypeName = type.FullName;
            createOperation.MethodCalled = methodCalled;
            createOperation.Created = DateTime.UtcNow;
            createOperation.Model = model;

            return createAsync.InvokeAsync(repo, createOperation);
        }

        public static Task DeleteAsync(this IDocumentRepository repo, IAggregate model, string methodCalled = null)
        {
            var type = model.GetType();
            var deleteOperationType = typeof(HardDeleteOperation<>).MakeGenericType(type);

            var deleteAsync = typeof(IDocumentRepository).GetMethod(nameof(IDocumentRepository.DeleteAsync)).MakeGenericMethod(type);

            var deleteOperation = Activator.CreateInstance(deleteOperationType).As<IDataStoreWriteOperation>();

            deleteOperation.TypeName = type.FullName;
            deleteOperation.MethodCalled = methodCalled;
            deleteOperation.Created = DateTime.UtcNow;
            deleteOperation.Model = model;

            return deleteAsync.InvokeAsync(repo, deleteOperation);
        }

        public static Task UpdateAsync(this IDocumentRepository repo, IAggregate model, string methodCalled = null)
        {
            var type = model.GetType();
            var updateOperationType = typeof(UpdateOperation<>).MakeGenericType(type);

            var updateAsync = typeof(IDocumentRepository).GetMethod(nameof(IDocumentRepository.UpdateAsync)).MakeGenericMethod(type);

            var updateoperation = Activator.CreateInstance(updateOperationType).As<IDataStoreWriteOperation>();
            updateoperation.TypeName = type.FullName;
            updateoperation.MethodCalled = methodCalled;
            updateoperation.Created = DateTime.UtcNow;
            updateoperation.Model = model;

            return updateAsync.InvokeAsync(repo, updateoperation);
        }

        private static Task InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, parameters);
            return awaitable;
        }
    }
}