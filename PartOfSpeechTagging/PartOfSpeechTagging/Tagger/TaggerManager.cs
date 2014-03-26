using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using MarkovChain;
using MarkovChain.InferringStrategies;

namespace PartOfSpeechTagging.Tagger 
{
    public class TaggerManager 
    {
        internal struct Breaks 
        {
            public static readonly string SentenceBreak = string.Empty;

            public static readonly string WordBreak = " ";
        }

        internal static IDictionary<string, int> Tags = new Dictionary<string, int>() 
        {
            {"ADJ", 0}, 
            {"ADV", 1}, 
            {"ADP", 2}, 
            {"CONJ", 3}, 
            {"DET", 4}, 
            {"NOUN", 5}, 
            {"NUM", 6}, 
            {"PRON", 7}, 
            {"PRT", 8}, 
            {"VERB", 9}, 
            {"X", 10}, 
            {".", 11}
        };

        private HiddenMarkovModel<string> markov;

        public TaggerManager() 
        {
            markov = new HiddenMarkovModel<string>(Tags.Count);
        }

        public void TrainHiddenMarkovModel(string trainingSetFileName) 
        {
            if (null == trainingSetFileName) 
            {
                Console.WriteLine(Messages.DefaultTrainingFile);
                trainingSetFileName = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, FileNames.DataPath, FileNames.TrainingSet);
            }

            if (!File.Exists(trainingSetFileName)) 
            {
                Console.WriteLine(ErrorMessages.NoTrainingFile);
                return;
            }

            Debug.WriteLine(string.Format(Messages.BeginLearning, trainingSetFileName));
            DateTime startTime = DateTime.Now;

            HmmObservation<string> observation = new HmmObservation<string>();
            string line;
            string[] wordTag;
            IList<string> inputWords = new List<string>();
            IList<int> inputTags = new List<int>();

            using (StreamReader reader = new StreamReader(
                new FileStream(trainingSetFileName, FileMode.Open), Encoding.UTF8)) 
            {
                while (!reader.EndOfStream) 
                {
                    line = reader.ReadLine();

                    if (line.Equals(Breaks.SentenceBreak, StringComparison.InvariantCultureIgnoreCase)) 
                    {
                        // Avoids duplicately learning caused by reading consequent blank lines from training set.
                        if (!observation.HasLearned) 
                        {
                            observation.Si = inputTags.ToArray();
                            inputTags.Clear();
                            observation.Wi = inputWords.ToArray();
                            inputWords.Clear();

                            // observation.HasLearned is set to true by this function.
                            markov.LearnData(observation);
                        }

                        continue;
                    }

                    wordTag = line.Split(new string[] { Breaks.WordBreak }, StringSplitOptions.RemoveEmptyEntries);

                    // Dirty data.
                    if (wordTag.Length < 2) 
                    {
                        continue;
                    }

                    // New observation.
                    if (observation.HasLearned) 
                    {
                        observation.HasLearned = false;
                    }

                    inputWords.Add(wordTag[0].ToLowerInvariant());
                    inputTags.Add(Tags[wordTag[1]]);
                }

            }

            markov.CalculateProbabilities();

            Debug.WriteLine(string.Format(Messages.FinishLearning, (DateTime.Now - startTime).TotalSeconds));
        }

        public void Tag(string testingSetFileName, Tagger[] taggers) 
        {
            if (null == taggers || taggers.Length < 1) 
            {
                return;
            }

            if (null == testingSetFileName) 
            {
                Console.WriteLine(Messages.DefaultTestingFile);
                testingSetFileName = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, FileNames.DataPath, FileNames.TestingSet);
            }

            if (!File.Exists(testingSetFileName)) 
            {
                Console.WriteLine(ErrorMessages.NoTestingFile);
                return;
            }

            int totalWord = 0;
            int totalSentence = 0;

            HmmObservation<string> observation = new HmmObservation<string>();

            Debug.WriteLine(string.Format(Messages.BeginTesting, testingSetFileName));
            DateTime startTime = DateTime.Now;
            Console.WriteLine(Messages.Separator);

            string line;
            string[] wordTag;
            IList<string> input, ground, output;
            int[] groundValue;
            int[][] inferences;
            int sampleNo;

            input = new List<string>();
            ground = new List<string>();
            output = new List<string>();

            using (StreamReader reader = new StreamReader(
                new FileStream(testingSetFileName, FileMode.Open), Encoding.UTF8)) 
            {
                while (!reader.EndOfStream) 
                {
                    line = reader.ReadLine();

                    if (line.Equals(Breaks.SentenceBreak, StringComparison.InvariantCultureIgnoreCase)) 
                    {
                        #region Test and Output

                        // Avoids duplicately testing caused by reading consequent blank lines from testing set.
                        if (input.Count > 0) 
                        {
                            totalWord += input.Count;
                            totalSentence++;
                            Console.WriteLine(
                                string.Format(Messages.ConsiderSentence, string.Join(Breaks.WordBreak, input)));
                            groundValue = ground.Select(x => Tags[x]).ToArray();
                            observation.Wi = input.Select(x => x.ToLowerInvariant()).ToArray();
                            input.Clear();
                            Console.WriteLine(
                                string.Format(Messages.GroundTruth, string.Join(Breaks.WordBreak, ground)));
                            ground.Clear();

                            foreach (var tagger in taggers) 
                            {
                                observation.Si = groundValue.ToArray();
                                inferences = tagger.Tag(markov, observation);

                                // Output inferences.
                                if (null == inferences || 
                                    tagger.InferenceCount < 1 || inferences.GetLength(0) != tagger.InferenceCount) 
                                {
                                    continue;
                                }
                                else if (tagger.InferenceCount == 1) 
                                {
                                    // If runs for only one time, no sample # will be output in the message.

                                    foreach (var si in inferences[0]) 
                                    {
                                        output.Add(Tags.Keys.ElementAt(si));
                                    }

                                    Console.WriteLine(
                                        string.Format(tagger.Name, string.Empty, string.Join(Breaks.WordBreak, output)));
                                    output.Clear();
                                }
                                else 
                                {
                                    sampleNo = 0;

                                    while (sampleNo < tagger.InferenceCount) 
                                    {
                                        foreach (var si in inferences[sampleNo]) 
                                        {
                                            output.Add(Tags.Keys.ElementAt(si));
                                        }

                                        sampleNo++;
                                        Console.WriteLine(string.Format(tagger.Name, 
                                            Breaks.WordBreak + sampleNo, string.Join(Breaks.WordBreak, output)));
                                        output.Clear();
                                    }
                                }
                            }

                            Console.WriteLine(Messages.Separator);
                        }

                        #endregion Test and Output

                        continue;
                    }

                    wordTag = line.Split(new string[] { Breaks.WordBreak }, StringSplitOptions.RemoveEmptyEntries);

                    if (wordTag.Length < 2) 
                    {
                        continue;
                    }

                    input.Add(wordTag[0]);
                    ground.Add(wordTag[1]);
                }
            }

            Debug.WriteLine(string.Format(Messages.FinishTesting, (DateTime.Now - startTime).TotalSeconds));

            #region Report

            Console.WriteLine(ReportMessages.PerformanceSummary);
            Console.WriteLine(ReportMessages.ParameterSummary, totalWord, totalSentence);

            foreach (var tagger in taggers) 
            {
                Console.WriteLine(string.Format(ReportMessages.FullName, tagger.FullName));

                if (!string.IsNullOrEmpty(tagger.Comments)) 
                {
                    Console.WriteLine(tagger.Comments);
                }

                Console.WriteLine(string.Format(ReportMessages.Percentage, ReportMessages.WordsAccuracy, 
                    100.0 * tagger.Stats.WordCorrectCount / tagger.Stats.WordCount));
                Console.WriteLine(string.Format(ReportMessages.Percentage, ReportMessages.SentencesAccuracy, 
                    100.0 * tagger.Stats.SentenceCorrectCount / tagger.Stats.SentenceCount));
            }

            #endregion Report
        }
    }
}
