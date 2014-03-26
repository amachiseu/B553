using System.Collections.Generic;
using System.Linq;

using MarkovChain;
using MarkovChain.InferringStrategies;

namespace PartOfSpeechTagging.Tagger 
{
    public abstract class Tagger 
    {
        /// <summary>
        /// Tagger name.
        /// </summary>
        public string FullName;

        /// <summary>
        /// Output message template.
        /// </summary>
        public string Name;

        /// <summary>
        /// Report comments.
        /// </summary>
        public string Comments;

        /// <summary>
        /// The count of inferences will be returned by Infer.
        /// </summary>
        public int InferenceCount;

        /// <summary>
        /// Strategy to be used for tagging.
        /// </summary>
        protected IHmmInferringStrategy<string> Strategy;

        /// <summary>
        /// Stats for reporting.
        /// </summary>
        public TaggerStats Stats;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected Tagger() 
        {
            // Runs Tag once by default.
            InferenceCount = 1;
            Stats = new TaggerStats();
            RefreshStats();
        }

        public void RefreshStats() 
        {
            Stats.WordCount = 0;
            Stats.WordCorrectCount = 0;
            Stats.SentenceCount = 0;
            Stats.SentenceCorrectCount = 0;
        }

        /// <summary>
        /// Tags the makov chain based on an obersavation.
        /// </summary>
        /// <param name="makov">The hidden markov model on which to get inference.</param>
        /// <param name="observation">An observation of Wi.</param>
        /// <param name="inferenceCount">The count of inferences will be returned.</param>
        /// <returns>An inference of Si.</returns>
        public virtual int[][] Tag(HiddenMarkovModel<string> markov, HmmObservation<string> observation) 
        {
            if (null == markov || null == observation || null == observation.Wi || observation.Wi.Length < 1) 
            {
                return null;
            }

            int[] ground = null == observation.Si ? null : observation.Si.ToArray();

            bool sentenceCorrect = false;
            bool sampleCorrect;
            List<int[]> inferences = new List<int[]>();

            for (int i = 0; i < InferenceCount; i++) 
            {
                inferences.Add(markov.GetInference(Strategy, observation));

                // Stats.
                Stats.WordCount += observation.Wi.Length;
                sampleCorrect = true;

                if (null != ground) 
                {
                    for (int j = 0; j < observation.Wi.Length; j++) 
                    {
                        if (ground[j] == observation.Si[j]) 
                        {
                            Stats.WordCorrectCount++;
                        }
                        else 
                        {
                            sampleCorrect = false;
                        }
                    }
                }

                // Sentence is correct if any of the samples is correct.
                sentenceCorrect |= sampleCorrect;
            }

            if (sentenceCorrect) 
            {
                Stats.SentenceCorrectCount++;
            }

            Stats.SentenceCount++;

            return inferences.ToArray();
        }
    }
}
