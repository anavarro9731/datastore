namespace DataStore.Infrastructure.Impl
{
    using System.Collections.Generic;
    using DataAccess.Interfaces.Addons;

    public class ValueReturner : IValueReturner
    {
        private readonly Dictionary<string, object> returnValues;

        private readonly string eventType;

        public ValueReturner(Dictionary<string, object> returnValues, string eventType)
        {
            this.returnValues = returnValues;
            this.eventType = eventType;
        }

        public void Return<TReturnValue>(TReturnValue returnValue)
        {
            this.returnValues.Add(eventType, returnValue);
        }
    }
}