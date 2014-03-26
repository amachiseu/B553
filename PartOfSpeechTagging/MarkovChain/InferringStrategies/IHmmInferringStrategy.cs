namespace MarkovChain.InferringStrategies 
{
    /// <summary>
    /// Provides algorithm for inferring with Hidden Markov Model.
    /// </summary>
    public interface IHmmInferringStrategy<T> 
    {
        /// <summary>
        /// Gets inference.
        /// </summary>
        /// <param name="makov">The hidden markov model on which to get inference.</param>
        /// <param name="observation">An observation of Wi.</param>
        /// <returns>An inference of Si.</returns>
        int[] Infer(HiddenMarkovModel<T> markov, HmmObservation<T> observation);
    }
}
