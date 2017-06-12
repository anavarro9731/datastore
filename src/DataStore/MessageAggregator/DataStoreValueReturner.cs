using System.Collections.Generic;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

namespace DataStore.MessageAggregator
{
    public class DataStoreValueReturner : IValueReturner
    {
        private readonly string eventType;

        private readonly Dictionary<string, object> returnValues;

        public DataStoreValueReturner(Dictionary<string, object> returnValues, string eventType)
        {
            this.returnValues = returnValues;
            this.eventType = eventType;
        }

        #region

        public IValueReturner Return<TReturnValue>(TReturnValue returnValue)
        {
            returnValues.Add(eventType, returnValue);
            return this;
        }

        #endregion
    }
}