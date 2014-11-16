namespace BusinessLogic.Interfaces
{
    public enum Response
    {
        Charge,
        Discharge
    }

    /// <summary>
    /// Storage abstraction.
    /// </summary>
    public interface IStorage : IMeasureable
    {
        /// <summary>
        /// Name/description of the storage.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Efficiency of the storage; a value between 1 (optimal) and zero.
        /// </summary>
        double Efficiency { get; }

        /// <summary>
        /// Initial capacity of the storage.
        /// </summary>
        double InitialEnergy { get; }

        /// <summary>
        /// Nominal energy capacity of the storage.
        /// </summary>
        double NominalEnergy { get; }

        /// <summary>
        /// Injects an amount of energy (positive or negative) into the storage.
        /// </summary>
        /// <param name="amount"> amount of energy </param>
        /// <returns> remaining energy </returns>
        double Inject(double amount);

        /// <summary>
        /// Inject maximum into the storage.
        /// </summary>
        /// <param name="response"> should we charge or discharge? </param>
        /// <returns> cost of charge/discharge </returns>
        double InjectMax(Response response);

        /// <summary>
        /// Remaining energy for a specific response; charge or dicharge.
        /// </summary>
        /// <param name="response"> needed backup response </param>
        /// <returns> remaining energy </returns>
        double RemainingEnergy(Response response);

        /// <summary>
        /// Available capacity for a specific response; charge or dicharge.
        /// </summary>
        /// <param name="response"> needed backup response </param>
        /// <returns> remaining capacity </returns>
        double AvailableEnergy(Response response);
        
        /// <summary>
        /// Reset capacity (to be used when a new simulation is started).
        /// </summary>
        void ResetEnergy();

        /// <summary>
        /// The capacity; how much can be charged/discharged in one time step NOT taking efficiency into account.
        /// </summary>
        double Capacity { get; }

    }

}
