using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MarkovChain.InferringStrategies;

namespace MarkovChain 
{
    /// <summary>
    /// Provides training and inferring methods with Hidden Markov Model.
    /// </summary>
    /// <typeparam name="T">Observation data type.</typeparam>
    public class HiddenMarkovModel<T> : MarkovChain 
    {
        /// <summary>
        /// The probability distribution of Si with distinct Wi.
        /// </summary>
        public double[] PSiWithDistinctWi;

        /// <summary>
        /// The occurance of emission from Si to Wi.
        /// </summary>
        public IDictionary<T, int>[] CEmissions;

        /// <summary>
        /// The emission probability distribution of Wi given Si.
        /// </summary>
        public IDictionary<T, double>[] PEmissions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stateCount">A value indicates the count of states.</param>
        public HiddenMarkovModel(int stateCount) : base(stateCount) 
        {
            PSiWithDistinctWi = new double[StateCount];
            CEmissions = new Dictionary<T, int>[] 
            {
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>(), 
                new Dictionary<T, int>()
            };
            PEmissions = new Dictionary<T, double>[] 
            {
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>(), 
                new Dictionary<T, double>()
            };
        }

        /// <summary>
        /// Learns the given observation.
        /// </summary>
        /// <param name="observation">An object indicates an observation.</param>
        public void LearnData(HmmObservation<T> observation) 
        {
            // Avoids duplicately learning.
            if (null == observation || observation.HasLearned) 
            {
                return;
            }

            if (null == observation.Si || observation.Si.Length < 1) 
            {
                return;
            }

            // Checks the length of Si array equals to that of Wi array.
            if (null == observation.Wi || observation.Wi.Length != observation.Si.Length) 
            {
                return;
            }

            int si;
            T wi;

            for (int i = 0; i < observation.Si.Length; i++) 
            {
                si = observation.Si[i];
                wi = observation.Wi[i];

                // Counts the occurance of wi given si.
                if (CEmissions[si].ContainsKey(wi)) 
                {
                    CEmissions[si][wi]++;
                }
                else 
                {
                    CEmissions[si][wi] = 1;
                }
            }

            // HasLearned is set to true by this function.
            base.LearnData(observation);
        }

        /// <summary>
        /// Learns the given observation.
        /// </summary>
        /// <param name="observation">An object indicates an observation.</param>
        public override void LearnData(Observation observation) 
        {
            LearnData(observation as HmmObservation<T>);
        }

        /// <summary>
        /// Calculates the probability distributions.
        /// </summary>
        public override void CalculateProbabilities() 
        {
            int sumOfCEmissions;
            IDictionary<T, int> wSi;

            for (int i = 0; i < StateCount; i++) 
            {
                wSi = CEmissions[i];
                sumOfCEmissions = wSi.Values.Sum();

                foreach (var wiSi in wSi)
                {
                    PEmissions[i][wiSi.Key] = 1.0 * wiSi.Value / sumOfCEmissions;
                }
            }

            int sumOfWord = CEmissions.Select(x => x.Count).Sum();

            for (int i = 0; i < StateCount; i++) 
            {
                PSiWithDistinctWi[i] = 1.0 * CEmissions[i].Count / sumOfWord;
            }

            base.CalculateProbabilities();
        }

        /// <summary>
        /// Gets inference with a given strategy.
        /// </summary>
        /// <param name="strategy">An object provides inferring strategy.</param>
        /// <returns>The probability distribution of Si given W.</returns>
        public virtual int[] GetInference(IHmmInferringStrategy<T> strategy, HmmObservation<T> observation) 
        {
            return strategy.Infer(this, observation);
        }
    }
}
