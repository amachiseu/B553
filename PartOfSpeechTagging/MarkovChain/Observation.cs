namespace MarkovChain 
{
    /// <summary>
    /// Provides an observation of Markov Chain.
    /// </summary>
    public abstract class Observation 
    {
        /// <summary>
        /// A value indicates whether the observation has been learned.
        /// </summary>
        public bool HasLearned;

        /// <summary>
        /// The occurance of Si.
        /// </summary>
        public int[] Si;
    }
}
