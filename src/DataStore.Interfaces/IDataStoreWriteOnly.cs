namespace DataStore.Interfaces
{
   #region

   using System;
   using System.Collections.Generic;
   using System.Linq.Expressions;
   using System.Threading.Tasks;
   using DataStore.Interfaces.LowLevel;
   using DataStore.Interfaces.Options;

   #endregion

   public interface IDataStoreWriteOnly
    {
         Task<T1> Create<T1, O>(T1 model, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : ClientSideCreateOptions, new();

         Task<T1> Create<T1>(T1 model, Action<ClientSideCreateOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<T1> Delete<T1, O>(T1 instance, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : ClientSideDeleteOptions, new();

         Task<T1> Delete<T1>(T1 instance, Action<ClientSideDeleteOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<T1> DeleteById<T1, O>(Guid id, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : ClientSideDeleteOptions, new();

         Task<T1> DeleteById<T1>(Guid id, Action<ClientSideDeleteOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<IEnumerable<T1>> DeleteWhere<T1, O>(
            Expression<Func<T1, bool>> predicate,
            Action<O> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() where O : ClientSideDeleteOptions, new();

         Task<IEnumerable<T1>> DeleteWhere<T1>(
            Expression<Func<T1, bool>> predicate,
            Action<ClientSideDeleteOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();

         Task<T1> Update<T1, O>(T1 src, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : ClientSideUpdateOptions, new();

         Task<T1> Update<T1>(T1 src, Action<ClientSideUpdateOptions> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new();

         Task<T1> UpdateById<T1, O>(Guid id, Action<T1> action, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : ClientSideUpdateOptions, new();

         Task<T1> UpdateById<T1>(
            Guid id,
            Action<T1> action,
            Action<ClientSideUpdateOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();

         Task<IEnumerable<T1>> UpdateWhere<T1, O>(
            Expression<Func<T1, bool>> predicate,
            Action<T1> action,
            Action<O> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() where O : ClientSideUpdateOptions, new();

         Task<IEnumerable<T1>> UpdateWhere<T1>(
            Expression<Func<T1, bool>> predicate,
            Action<T1> action,
            Action<ClientSideUpdateOptions> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new();
    }
}