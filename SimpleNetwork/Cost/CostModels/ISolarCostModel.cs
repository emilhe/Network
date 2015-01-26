using Utils;

namespace BusinessLogic.Cost.Transmission
{
    public interface ISolarCostModel
    {

        double SolarCost(double capacity);

    }

    public class SolarCostModelImpl : ISolarCostModel
    {

        private readonly ISolarCostModel _mCore;

        public SolarCostModelImpl()
        {
            _mCore = new ScaledSolarCostModel(1.0);
        }

        public double SolarCost(double capacity)
        {
            return _mCore.SolarCost(capacity);
        }

    }

    public class ScaledSolarCostModel : ISolarCostModel
    {

        // Lifetime in years
        private const double Lifetime = 30;
        private readonly double _mScale;

        public ScaledSolarCostModel(double scale)
        {
            _mScale = scale;
        }

        public double SolarCost(double capacity)
        {
            return _mScale*capacity*
                   (Costs.Solar.CapExFixed*1e6 + Costs.Solar.OpExFixed*1e3*Costs.AnnualizationFactor(Lifetime));
        }

    }

}
