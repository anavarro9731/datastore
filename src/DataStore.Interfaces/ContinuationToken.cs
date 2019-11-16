namespace DataStore.Interfaces
{
    using System;

    public class ContinuationToken
    {
        public ContinuationToken(object value)
        {
            Value = value;
        }

        public ContinuationToken()
        {
        }

        public object Value { set; get; }

        public int ToInt()
        {
            try
            {
                NullCheck();
                return Convert.ToInt32(Value);
            }
            catch (FormatException)
            {
                throw new FormatException("Could not convert continuation token to number, please check inputs");
            }
        }

        public override string ToString()
        {
            NullCheck();
            return Value.ToString();
        }

        private void NullCheck()
        {
            if (Value == null) throw new Exception("Trying to use continuation token but it's value is null");
        }
    }
}