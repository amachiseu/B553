using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkovChain.InferringStrategies 
{
    /// <summary>
    /// Provides Naive inferring.
    /// </summary>
    public class NaiveInferring<T> : IHmmInferringStrategy<T> 
    {
        #region IHmmInferringStrategy<T> Members

        /// <summary>
        /// Gets inference.
        /// </summary>
        /// <param name="makov">The hidden markov model on which to get inference.</param>
        /// <param name="observation">An observation of Wi.</param>
        /// <returns>An inference of Si.</returns>
        public int[] Infer(HiddenMarkovModel<T> markov, HmmObservation<T> observation) 
        {
            // A tiny value that indicates zero.
            double zero = Math.Pow(10.0, -Math.Sqrt(-(Math.Log10(double.Epsilon))));

            double maxSi;
            int inference;
            double[] pEmissions;
            double pEmissionPosteriors;
            T wi;

            observation.Si = new int[observation.Wi.Length];

            for (int i = 0; i < observation.Wi.Length; i++) 
            {
                wi = observation.Wi[i];

                // Gets the probability distribution of Wi given Si.
                // If Wi does not exist in the training set, sets a very small value to its probability.
                pEmissions = markov.PEmissions.Select(x => x.ContainsKey(wi) ? x[wi] : zero).ToArray();
                maxSi = -1.0;

                inference = -1;

                for (int j = 0; j < pEmissions.Length; j++) 
                {
                    // P(Si|Wi) = P(Wi|Si) * P(Si) / P(Wi). Here P(Wi) is 1.0.
                    pEmissionPosteriors = pEmissions[j] * markov.PSi[j];

                    if (maxSi < pEmissionPosteriors) 
                    {
                        maxSi = pEmissionPosteriors;
                        inference = j;
                    }
                }

                // Returns the inference of Si with max P(Si|Wi).
                observation.Si[i] = inference;
            }

            return observation.Si;
        }

        #endregion
    }
}
