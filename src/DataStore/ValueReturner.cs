namespace DataStore
{
    using System.Collections.Generic;
    using DataAccess.Interfaces.Addons;

    public class ValueReturner : IValueReturner
    {
        private readonly Dictionary<string, object> _returnValues;

        private readonly string _eventType;

        public ValueReturner(Dictionary<string, object> returnValues, string eventType)
        {
            this._returnValues = returnValues;
            this._eventType = eventType;
        }

        public void Return<TReturnValue>(TReturnValue returnValue)
        {
            this._returnValues.Add(_eventType, returnValue);
        }
    }
}