namespace BusinessLogic.Interfaces
{
    public interface IMeasureableLeaf : IMeasureable
    {

        /// <summary>
        /// Get the measurement; could be a TimeSeries.
        /// </summary>
        /// <returns></returns>
        ITimeSeries TimeSeries { get; }

    }
}
