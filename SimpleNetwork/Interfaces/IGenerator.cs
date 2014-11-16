namespace BusinessLogic.Interfaces
{
    /// <summary>
    /// Power generation abstraction.
    /// </summary>
    public interface IGenerator : IMeasureable, ITickListener
    {
        
        string Name { get; }
        double Production { get; }

    }

}
