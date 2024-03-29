﻿namespace DataStore.MessageAggregator
{
    #region

    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;

    #endregion

    public class DataStoreValueReturner : IValueReturner
    {
        private readonly string eventType;

        private readonly Dictionary<string, object> returnValues;

        public DataStoreValueReturner(Dictionary<string, object> returnValues, string eventType)
        {
            this.returnValues = returnValues;
            this.eventType = eventType;
        }

        public IValueReturner Return<TReturnValue>(TReturnValue returnValue)
        {
            this.returnValues.Add(this.eventType, returnValue);
            return this;
        }
    }
}