namespace DataStore.Interfaces
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options.ClientSide;

    #endregion

    public interface IDataStoreWriteOnly<T> where T : class, IAggregate, new()
    {
        Task<T> Create<O>(T model, Action<O> setOptions = null, string methodName = null) where O : CreateClientSideOptions, new();

        Task<T> Create(T model, Action<CreateClientSideOptions> setOptions = null, string methodName = null);

        Task<T> Delete<O>(T instance, Action<O> setOptions = null, string methodName = null) where O : DeleteClientSideOptions, new();

        Task<T> Delete(T instance, Action<DeleteClientSideOptions> setOptions = null, string methodName = null);

        Task<T> DeleteById<O>(Guid id, Action<O> setOptions = null, string methodName = null) where O : DeleteClientSideOptions, new();

        Task<T> DeleteById(Guid id, Action<DeleteClientSideOptions> setOptions = null, string methodName = null);

        Task<T> DeleteById(string longId, Action<DeleteClientSideOptions> setOptions = null, string methodName = null);

        Task<IEnumerable<T>> DeleteWhere<O>(Expression<Func<T, bool>> predicate, Action<O> setOptions = null, string methodName = null)
            where O : DeleteClientSideOptions, new();

        Task<IEnumerable<T>> DeleteWhere(Expression<Func<T, bool>> predicate, Action<DeleteClientSideOptions> setOptions = null, string methodName = null);

        Task<T> Update<O>(T src, Action<O> setOptions = null, string methodName = null) where O : UpdateClientSideOptions, new();

        Task<T> Update(T src, Action<UpdateClientSideOptions> setOptions = null, string methodName = null);

        Task<T> UpdateById<O>(Guid id, Action<T> action, Action<O> setOptions = null, string methodName = null) where O : UpdateClientSideOptions, new();

        Task<T> UpdateById(Guid id, Action<T> action, Action<UpdateClientSideOptions> setOptions = null, string methodName = null);

        Task<T> UpdateById(string longId, Action<T> action, Action<UpdateClientSideOptions> setOptions = null, string methodName = null);

        Task<IEnumerable<T>> UpdateWhere<O>(Expression<Func<T, bool>> predicate, Action<T> action, Action<O> setOptions = null, string methodName = null)
            where O : UpdateClientSideOptions, new();

        Task<IEnumerable<T>> UpdateWhere(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<UpdateClientSideOptions> setOptions = null,
            string methodName = null);
    }
    
    
    
   public interface IDataStoreWriteOnly
    {
         Task<T1> Create<T1, O>(T1 model, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : CreateClientSideOptions, new();

         Task<T1> Create<T1>(T1 model, Action<CreateClientSideOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<T1> Delete<T1, O>(T1 instance, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : DeleteClientSideOptions, new();

         Task<T1> Delete<T1>(T1 instance, Action<DeleteClientSideOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<T1> DeleteById<T1, O>(Guid id, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : DeleteClientSideOptions, new();

         Task<T1> DeleteById<T1>(Guid id, Action<DeleteClientSideOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();
         
         Task<T1> DeleteById<T1>(string longId, Action<DeleteClientSideOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<IEnumerable<T1>> DeleteWhere<T1, O>(
            Expression<Func<T1, bool>> predicate,
            Action<O> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() where O : DeleteClientSideOptions, new();

         Task<IEnumerable<T1>> DeleteWhere<T1>(
            Expression<Func<T1, bool>> predicate,
            Action<DeleteClientSideOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();

         Task<T1> Update<T1, O>(T1 src, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : UpdateClientSideOptions, new();

         Task<T1> Update<T1>(T1 src, Action<UpdateClientSideOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<T1> UpdateById<T1, O>(Guid id, Action<T1> action, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : UpdateClientSideOptions, new();

         Task<T1> UpdateById<T1>(
            Guid id,
            Action<T1> action,
            Action<UpdateClientSideOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();
         
         Task<T1> UpdateById<T1>(
            string longId,
            Action<T1> action,
            Action<UpdateClientSideOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();

         Task<IEnumerable<T1>> UpdateWhere<T1, O>(
            Expression<Func<T1, bool>> predicate,
            Action<T1> action,
            Action<O> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() where O : UpdateClientSideOptions, new();

         Task<IEnumerable<T1>> UpdateWhere<T1>(
            Expression<Func<T1, bool>> predicate,
            Action<T1> action,
            Action<UpdateClientSideOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();
    }
}