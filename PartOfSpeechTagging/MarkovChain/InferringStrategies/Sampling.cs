using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkovChain.InferringStrategies 
{
    /// <summary>
    /// Provides Sampling algorithm for inferring.
    /// </summary>
    public class Sampling<T> : IHmmInferringStrategy<T> 
    {
        /// <summary>
        /// Probability distribution of S1 given W.
        /// </summary>
        protected double[] PS1W;

        /// <summary>
        /// The observation for inferring in last time.
        /// </summary>
        protected T[] LastObservation;

        /// <summary>
        /// Random value generator.
        /// </summary>
        private static Random random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Gets a sample based on given probability distribution.
        /// </summary>
        /// <param name="p">Given probability distribution.</param>
        /// <returns>Sample.</returns>
        protected int Sample(double[] p) 
        {
            if (null == p || p.Length < 1) 
            {
                return -1;
            }

            double[] scopes = new double[p.Length];
            double randomValue = random.NextDouble() % p.Sum();
            double temp = 0.0;

            for (int i = 0; i < p.Length; i++) 
            {
                temp += p[i];

                if (temp > randomValue) 
                {
                    return i;
                }
            }

            return p.Length - 1;
        }

        #region IHmmInferringStrategy<T> Members

        public int[] Infer(HiddenMarkovModel<T> markov, HmmObservation<T> observation) 
        {
            if (null == observation || null == observation.Wi || observation.Wi.Length < 1) 
            {
                return null;
            }

            // equal indicates whether the given observation is equal to the one in last time.
            bool equal = (null != LastObservation) && observation.Wi.Length == LastObservation.Length;

            if (equal) 
            {
                for (int i = 0; i < observation.Wi.Length; i++) 
                {
                    if (!LastObservation[i].Equals(observation.Wi[i])) 
                    {
                        equal = false;
                        break;
                    }
                }
            }

            // If new observation comes, recalculates the P(S1|W); otherwise uses the initial probability in last time.
            if (!equal) 
            {
                PS1W = new double[markov.StateCount];
                IList<double[]> forwardDistribution;
                IList<double[]> backwardDistribution;

                (new ForwardBackward<T>()).Infer(markov, observation, out forwardDistribution, out backwardDistribution);

                for (int i = 0; i < markov.StateCount; i++) 
                {
                    PS1W[i] = forwardDistribution[0][i] * backwardDistribution[0][i];
                }

                LastObservation = observation.Wi;
            }

            // A tiny value that indicates zero.
            double zero = Math.Pow(10.0, -Math.Sqrt(-(Math.Log10(double.Epsilon))));

            double[] pSi = PS1W;
            int sample = Sample(PS1W);
            T wi;
            IDictionary<T, double> pEmissions;

            observation.Si = new int[observation.Wi.Length];
            observation.Si[0] = sample;

            // P(Si|S(i-1)=s(i-1), Wi) = P(Si|S(i-1)=s(i-1)) * P(Wi|Si).
            for (int i = 1; i < observation.Wi.Length; i++) 
            {
                pSi = new double[markov.StateCount];
                wi = observation.Wi[i];

                for (int j = 0; j < markov.StateCount; j++) 
                {
                    pEmissions = markov.PEmissions[j];

                    if (pEmissions.ContainsKey(wi)) 
                    {
                        pSi[j] = markov.PTransitions[sample, j] * pEmissions[wi];
                    }
                    else 
                    {
                        // If the Wi does not exist given Si, uses a small probability as P(Wi|Si).
                        pSi[j] = markov.PTransitions[sample, j] * zero;
                    }
                }

                sample = Sample(pSi);
                observation.Si[i] = sample;
            }

            return observation.Si;
        }

        #endregion
    }
}
