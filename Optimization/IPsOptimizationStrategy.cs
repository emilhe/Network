namespace Optimization
{
    public interface IPsOptimizationStrategy<T> where T : IParticle
    {

        ISolution BestSolution { get; }

        bool TerminationCondition(T[] particles);

        void UpdateVelocities(T[] particles);
        void UpdatePositions(T[] particles);
        void UpdateBestPositions(T[] particles);

    }
}
