namespace CircuitBoard.MessageAggregator
{
    /// <summary>
    ///     required because of reference by IMockGatedFunctions
    /// </summary>
    public interface IValueReturner
    {
        IValueReturner Return<TReturnValue>(TReturnValue returnValue);
    }
}