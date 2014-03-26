namespace MarkovChain 
{
    /// <summary>
    /// Provides an observation of Hidden Markov Model.
    /// </summary>
    /// <typeparam name="T">Observation data type.</typeparam>
    public class HmmObservation<T> : Observation 
    {
        /// <summary>
        /// The occurance of Wi.
        /// </summary>
        public T[] Wi;
    }
}
