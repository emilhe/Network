using Utils;

namespace BusinessLogic.Cost.Transmission
{
    public interface IWindCostModel
    {

        double OnshoreWindCost(double capacity);
        double OffshoreWindCost(double capacity);

    }

    public class WindCostModelImpl : IWindCostModel
    {

        // Lifetime in years
        private const double LifeTime = 30;

        public double OffshoreWindCost(double capacity)
        {
            return capacity * (Costs.OffshoreWind.CapExFixed * 1e6 + Costs.OffshoreWind.OpExFixed * 1e3 * Costs.AnnualizationFactor(LifeTime));
        }

        public double OnshoreWindCost(double capacity)
        {
            return capacity * (Costs.OnshoreWind.CapExFixed * 1e6 + Costs.OnshoreWind.OpExFixed * 1e3 * Costs.AnnualizationFactor(LifeTime));
        }

    }
}
