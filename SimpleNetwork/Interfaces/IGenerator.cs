namespace BusinessLogic.Interfaces
{
    /// <summary>
    /// Power generation abstraction.
    /// </summary>
    public interface IGenerator : IMeasureableLeaf
    {
        string Name { get; }
        double GetProduction(int tick);
    }

}
