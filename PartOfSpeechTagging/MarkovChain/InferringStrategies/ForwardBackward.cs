using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkovChain.InferringStrategies 
{
    /// <summary>
    /// Provides Forward-Backward algorithm for inferring.
    /// </summary>
    public class ForwardBackward<T> : IHmmInferringStrategy<T> 
    {
        /// <summary>
        /// Gets inference.
        /// </summary>
        /// <param name="makov">The hidden markov model on which to get inference.</param>
        /// <param name="observation">An observation of Wi.</param>
        /// <param name="forwardDistribution">Distribution of P(Si, W[1:i]) = P(Si, Wi), since W[1:i-1] are D-separated by Si with Wi.</param>
        /// <param name="backwardDistribution">Distribution of P(W(i+1:n)|Si).</param>
        public void Infer(HiddenMarkovModel<T> markov, HmmObservation<T> observation, 
            out IList<double[]> forwardDistribution, out IList<double[]> backwardDistribution) 
        {
            // A tiny value that indicates zero.
            double zero = Math.Pow(10.0, -Math.Sqrt(-(Math.Log10(double.Epsilon))));

            // Distribution of P(Si, W[1:i]) = P(Si, Wi), since W[1:i-1] are D-separated by Si with Wi.
            forwardDistribution = new List<double[]>();

            // Forward(i) = P(Si, W[1:i]).
            double[] fi = null;

            // Forward(i-1).
            double[] fiM1 = null;

            // Sum of (P(S(i-1)) * P(Si | S(i-1))).
            double sumOfSiM1 = 0.0;

            T wi;

            // Emission probability distribution of Si.
            IDictionary<T, double> pEmissioni;

            for (int i = 0; i < observation.Wi.Length; i++) 
            {
                fi = new double[markov.StateCount];
                wi = observation.Wi[i];

                for (int j = 0; j < markov.StateCount; j++) 
                {
                    if (i == 0) 
                    {
                        // P(S1)
                        sumOfSiM1 = markov.PS1[j];
                    }
                    else 
                    {
                        sumOfSiM1 = 0.0;

                        // To marginalize out S(i-1) and get P(Si), Sum(Forward(i-1) * P(Si|S(i-1))).
                        for (int k = 0; k < markov.StateCount; k++) 
                        {
                            sumOfSiM1 += fiM1[k] * markov.PTransitions[k, j];
                        }
                    }

                    pEmissioni = markov.PEmissions[j];

                    // P(Si, Wi) = P(Wi|Si) * Sum(P(S(i-1)) * P(Si | S(i-1))).
                    if (pEmissioni.ContainsKey(wi)) 
                    {
                        fi[j] = pEmissioni[wi] * sumOfSiM1;
                    }
                    else 
                    {
                        // If the Wi does not exist given Si, uses a small probability as P(Wi|Si).
                        fi[j] = zero * sumOfSiM1;
                    }
                }

                forwardDistribution.Add(fi);
                fiM1 = fi;
            }

            // Distribution of P(W(i+1:n)|Si).
            backwardDistribution = new List<double[]>();
            int last = observation.Wi.Length;
            double sum = 0.0;

            // Backward(i) = P(W[i+1:n]|Si).
            double[] bi = null;

            // Backward(i+1).
            double[] biP1 = null;

            // W(i+1).
            T wi1;
            IDictionary<T, double> pWi1Si1;

            // For W, "i" is i+1; for others, "i" is i. Here only W is bound to i.
            for (int i = last; i > 0; i--) 
            {
                bi = new double[markov.StateCount];

                if (i == last) 
                {
                    // Unnecessary step, just to initialize the W(i+1) for the compiler...
                    wi1 = observation.Wi[i - 1];
                }
                else 
                {
                    // W(i+1).
                    wi1 = observation.Wi[i];
                }

                for (int j = 0; j < markov.StateCount; j++) 
                {
                    if (i == last) 
                    {
                        // P(Sn). Backward(n+1) = 1, but it runs Backward from n.
                        bi[j] = markov.PSn[j];
                    }
                    else 
                    {
                        sum = 0.0;

                        // Marginalizes out S(i+1), and gets P(W[i+1:n]|Si).
                        // P(W[i+1:n]|Si) = Sum(P(S(i+1)|Si) * P(W(i+1)|S(i+1)) * Backward(i+1)).
                        for (int k = 0; k < markov.StateCount; k++) 
                        {
                            pWi1Si1 = markov.PEmissions[k];

                            if (pWi1Si1.ContainsKey(wi1)) 
                            {
                                sum += markov.PTransitions[j, k] * markov.PEmissions[k][wi1] * biP1[k];
                            }
                            else 
                            {
                                // If the W(i+1) does not exist given S(i+1), 
                                // uses a small probability as P(W(i+1)|S(i+1)).
                                sum += markov.PTransitions[j, k] * zero * biP1[k];
                            }
                        }

                        bi[j] = sum;
                    }
                }

                backwardDistribution.Insert(0, bi);
                biP1 = bi;
            }
        }

        #region IHmmInferringStrategy<T> Members

        public int[] Infer(HiddenMarkovModel<T> markov, HmmObservation<T> observation) 
        {
            IList<double[]> forwardDistribution;
            IList<double[]> backwardDistribution;

            Infer(markov, observation, out forwardDistribution, out backwardDistribution);

            double max, p;
            int inference = 0;

            observation.Si = new int[observation.Wi.Length];

            // Gets the max value of each P(Si|W) as inference.
            // Here, as what Forward-backward algorithm does, 
            // P(Si|W) is propotionally equal to Forward(i) * Backward(i) = P(Si, W(1:i)) * P(W(i+1:n)|Si).
            for (int i = 0; i < observation.Wi.Length; i++) 
            {
                max = -1.0;

                for (int j = 0; j < markov.StateCount; j++) 
                {
                    p = forwardDistribution[i][j] * backwardDistribution[i][j];

                    if (p > max) 
                    {
                        max = p;
                        inference = j;
                    }
                }

                observation.Si[i] = inference;
            }

            return observation.Si;
        }

        #endregion
    }
}
