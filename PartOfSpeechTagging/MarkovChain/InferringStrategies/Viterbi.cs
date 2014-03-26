using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkovChain.InferringStrategies 
{
    /// <summary>
    /// Provides Viterbi algorithm for inferring.
    /// </summary>
    public class Viterbi<T> : IHmmInferringStrategy<T> 
    {
        #region IHmmInferringStrategy<T> Members

        public int[] Infer(HiddenMarkovModel<T> markov, HmmObservation<T> observation) 
        {
            // A tiny value that indicates zero.
            double zero = Math.Pow(10.0, -Math.Sqrt(-(Math.Log10(double.Epsilon))));

            // V.
            double[,] v = new double[observation.Wi.Length, markov.StateCount];

            // Path and temp path.
            IDictionary<int, int[]> path = new Dictionary<int, int[]>();
            IDictionary<int, int[]> tempPath;

            // Emission probability distribution of Si.
            IDictionary<T, double> pEmissioni;
            T wi;
            
            // Initializes the path.
            for (int i = 0; i < markov.StateCount; i++) 
            {
                wi = observation.Wi[0];
                pEmissioni = markov.PEmissions[i];

                if (pEmissioni.ContainsKey(wi)) 
                {
                    // M1(S1) = P(S1) * P(W1 | S1).
                    v[0, i] = markov.PS1[i] * markov.PEmissions[i][wi];
                }
                else 
                {
                    // If the Wi does not exist given Si, uses a small probability as P(Wi|Si).
                    v[0, i] = markov.PS1[i] * zero;
                }

                path[i] = new int[] { i };
            }

            double max, p;
            int iWithMaxP;
            double e;

            // Mk(Sk) = Max(P(Wk|Sk) * P(Sk|S(k-1)) * M(k-1)(S(k-1))).
            for (int i = 1; i < observation.Wi.Length; i++) 
            {
                tempPath = new Dictionary<int, int[]>();
                wi = observation.Wi[i];
                
                for (int j = 0; j < markov.StateCount; j++) 
                {
                    max = -1.0;
                    pEmissioni = markov.PEmissions[j];

                    if (pEmissioni.ContainsKey(wi)) 
                    {
                        e = pEmissioni[wi];
                    }
                    else 
                    {
                        e = zero;
                    }

                    iWithMaxP = -1;

                    for (int k = 0; k < markov.StateCount; k++) 
                    {
                        p = v[i - 1, k] * markov.PTransitions[k, j] * e;

                        if (p > max) 
                        {
                            max = p;
                            v[i, j] = p;
                            iWithMaxP = k;
                        }
                    }

                    tempPath[j] = path[iWithMaxP].Concat(new int[] { j }).ToArray();
                }

                // Replaces the paths.
                path = tempPath;
            }

            max = -1.0;
            iWithMaxP = -1;

            int last = observation.Wi.Length - 1;

            for (int i = 0; i < markov.StateCount; i++) 
            {
                p = v[last, i];

                if (p > max) 
                {
                    p = max;
                    iWithMaxP = i;
                }
            }

            observation.Si = path[iWithMaxP];

            return observation.Si;
        }

        #endregion
    }
}
