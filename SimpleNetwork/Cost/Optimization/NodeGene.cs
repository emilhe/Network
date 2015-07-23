namespace BusinessLogic.Cost
{
    public class NodeGene
    {

        public double Alpha { get; set; }
        public double Gamma { get; set; }
        public double OffshoreFraction { get; set; }
        // New hydro/bio stuff.
        public double HydroFraction { get; set; }
        public double BiomassFraction { get; set; }

        // Update gamma value, but KEEP original hydro/bio absolute production.
        public void UpdateGamma(double newGamma)
        {
            HydroFraction *= Gamma/newGamma;
            BiomassFraction *= Gamma/newGamma;
            // The fractions cannot exceed one; reverse the scale partly.
            if (HydroFraction + BiomassFraction > 1)
            {
                var correction = 1/(HydroFraction + BiomassFraction);
                newGamma /= correction;
                HydroFraction *= correction;
                BiomassFraction *= correction;
            }
            Gamma = newGamma;
        }

    }
}
