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
        double InitialCapacity { get; }

        /// <summary>
        /// Nominal capacity of the storage.
        /// </summary>
        double Capacity { get; }

        /// <summary>
        /// Injects an amount of energy (positive or negative) into the storage.
        /// </summary>
        /// <param name="amount"> amount of energy </param>
        /// <returns> remaining energy </returns>
        double Inject(double amount);

        /// <summary>
        /// Restore (= fully charge/discharge) the storage.
        /// </summary>
        /// <param name="response"> should we charge or discharge? </param>
        /// <returns> cost of discharge </returns>
        double Restore(Response response);

        /// <summary>
        /// Remaining capacity for a specific response; charge or dicharge.
        /// </summary>
        /// <param name="response"> needed backup response </param>
        /// <returns> remaining capacity </returns>
        double RemainingCapacity(Response response);
        
        /// <summary>
        /// Reset capacity (to be used when a new simulation is started).
        /// </summary>
        void ResetCapacity();

        /// <summary>
        /// The POWER capacity (how much can be discharged in one time step).
        /// </summary>
        double LimitIn { get; }

        /// <summary>
        /// The POWER capacity (how much can be discharged in one time step).
        /// </summary>
        double LimitOut { get; }

    }

}
