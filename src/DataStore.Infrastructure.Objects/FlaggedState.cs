namespace Infrastructure.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    public class FlaggedState
    {
        public FlaggedState(Enum initialState)
        {
            this.ReplaceState(initialState);
        }

        [JsonConstructor]
        private FlaggedState()
        {
        }

        public int AsInteger { get; set; }

        public List<int> AsIntegers { get; set; } = new List<int>();

        public void AddState(Enum additionalState)
        {
            var value = Convert.ToInt32(additionalState);
            if (!this.AsIntegers.Contains(value))
            {
                this.AsInteger += value;
                this.AsIntegers.Add(value);
            }
        }

        public T AsEnum<T>()
        {
            return (T)Enum.Parse(typeof(T), this.AsInteger.ToString());
        }

        public void RemoveState(Enum stateToRemove)
        {
            var value = Convert.ToInt32(stateToRemove);
            if (this.AsIntegers.Count == 1) throw new Exception("Cannot have an empty state. Set FlaggedState variable to null instead.");
            if (this.AsIntegers.Contains(value))
            {
                this.AsInteger -= value;
                this.AsIntegers.Remove(value);
            }
        }

        public void ReplaceState(Enum newState)
        {
            this.AsInteger = Convert.ToInt32(newState);
            this.AsIntegers.Clear();
            this.AsIntegers.AddRange(Enum.GetValues(newState.GetType()).Cast<int>().Where(v => (v & Convert.ToInt32(newState)) == v));
        }
    }
}