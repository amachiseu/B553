using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MarkovChain.InferringStrategies;

namespace MarkovChain 
{
    /// <summary>
    /// Provides training and inferring methods with Markov Chain.
    /// </summary>
    public abstract class MarkovChain 
    {
        /// <summary>
        /// Gets a value indicates the count of states.
        /// </summary>
        public int StateCount 
        {
            get;
            protected set;
        }

        /// <summary>
        /// The occurance of the first variable S1.
        /// </summary>
        public int[] CS1;

        /// <summary>
        /// The occurance of Si.
        /// </summary>
        public int[] CSi;

        /// <summary>
        /// The occurance of the last variable Sn.
        /// </summary>
        public int[] CSn;

        /// <summary>
        /// The occurance of transition from Si to S(i+1).
        /// </summary>
        public int[,] CTransitions;

        /// <summary>
        /// The probability distribution of the first variable S1.
        /// </summary>
        public double[] PS1;

        /// <summary>
        /// The probability distribution of Si.
        /// </summary>
        public double[] PSi;

        /// <summary>
        /// The probability distribution of the last variable Sn.
        /// </summary>
        public double[] PSn;

        /// <summary>
        /// The transition probability distribution of S(i+1) given Si.
        /// </summary>
        public double[,] PTransitions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stateCount">A value indicates the count of states.</param>
        protected MarkovChain(int stateCount) 
        {
            StateCount = stateCount;
            CS1 = new int[StateCount];
            CSi = new int[StateCount];
            CSn = new int[StateCount];
            CTransitions = new int[StateCount, StateCount];

            PS1 = new double[StateCount];
            PSi = new double[StateCount];
            PSn = new double[StateCount];
            PTransitions = new double[StateCount, StateCount];
        }

        /// <summary>
        /// Learns the given observation.
        /// </summary>
        /// <param name="observation">An object indicates an observation.</param>
        public virtual void LearnData(Observation observation) 
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

            int si = observation.Si[0];

            // Counts the occurance of S1.
            CS1[si]++;

            // Counts the occurance of Si.
            CSi[si]++;

            // S(i+1) starts from the second value.
            foreach (var si1 in observation.Si.Skip(1)) 
            {
                // Counts the occurance of S(i+1) given Si.
                CTransitions[si, si1]++;
                si = si1;

                // Counts the occurance of Si;
                CSi[si]++;
            }

            CSn[observation.Si.Last()]++;

            observation.HasLearned = true;
        }

        /// <summary>
        /// Calculates the probability distributions.
        /// </summary>
        public virtual void CalculateProbabilities() 
        {
            int sumOfCS1 = CS1.Sum();

            for (int i = 0; i < StateCount; i++) 
            {
                PS1[i] = 1.0 * CS1[i] / sumOfCS1;
            }

            int sumOfCSi = CSi.Sum();

            for (int i = 0; i < StateCount; i++) 
            {
                PSi[i] = 1.0 * CSi[i] / sumOfCSi;
            }

            int sumOfCSn = CSn.Sum();

            for (int i = 0; i < StateCount; i++) 
            {
                PSn[i] = 1.0 * CSn[i] / sumOfCSn;
            }

            int sumOfCTransitions;

            for (int i = 0; i < StateCount; i++) 
            {
                sumOfCTransitions = 0;

                for (int j = 0; j < StateCount; j++) 
                {
                    sumOfCTransitions += CTransitions[i, j];
                }

                for (int j = 0; j < StateCount; j++) 
                {
                    PTransitions[i, j] = 1.0 * CTransitions[i, j] / sumOfCTransitions;
                }
            }
        }
    }
}
