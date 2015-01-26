using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace BusinessLogic.Cost.CostModels
{
    public interface IBackupCostModel
    {

        double BackupEnergyCost(double energy);
        double BackupCapacityCost(double capacity);

    }

    public class BackupCostModelImpl : IBackupCostModel
    {

        // Lifetime in years
        private const double Lifetime = 30;

        public double BackupEnergyCost(double energy)
        {
            return energy * Costs.CCGT.OpExVariable * Costs.AnnualizationFactor(Lifetime);
        }

        public double BackupCapacityCost(double capacity)
        {
            return capacity * (Costs.CCGT.CapExFixed * 1e6 + Costs.CCGT.OpExFixed * 1e3 * Costs.AnnualizationFactor(Lifetime));
        }

    }
}
