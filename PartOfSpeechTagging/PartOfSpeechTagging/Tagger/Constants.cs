namespace PartOfSpeechTagging.Tagger 
{
    /// <summary>
    /// Defines the file names of default data sets.
    /// </summary>
    internal struct FileNames 
    {
        public static readonly string DataPath = "Data";

        public static readonly string TrainingSet = "bc.train";

        public static readonly string TestingSet = "bc.test";

        public static readonly string SmallTestingSet = "bc.small.test";

        public static readonly string TinyTestingSet = "bc.tiny.test";
    };

    /// <summary>
    /// Defines the output messages.
    /// </summary>
    internal struct Messages 
    {
        public const string BeginLearning = "Learning from {0} ...";

        public const string FinishLearning = "Finished learning in {0} second(s).";

        public const string BeginTesting = "Testing data {0} ...";

        public const string FinishTesting = "Finished testing in {0} second(s).";

        public const string ConsiderSentence = "Considering sentence: \t{0}";

        public const string GroundTruth = "Ground truth: \t\t{0}";

        public const string NaiveInference = "Naive{0}: \t\t\t{1}";

        public const string BayesInference = "Bayes{0}: \t\t\t{1}";

        public const string ViterbiInference = "Viterbi{0}: \t\t{1}";

        public const string SampleInference = "Sample{0}: \t\t{1}";

        public const string Separator = "--------------";

        public const string DefaultTrainingFile = "Using default training set...";

        public const string DefaultTestingFile = "Using default testing set...";
    }

    internal struct ReportMessages 
    {
        public const string PerformanceSummary = "PERFORMANCE SUMMARY";

        public const string ParameterSummary = "Total words: {0} \tsentences: {1}";

        public const string FullName = "{0}:";

        public const string Percentage = "{0, -24}: {1}%";

        public const string WordsAccuracy = "Words correct";

        public const string SentencesAccuracy = "Sentences correct";

        public const string SampleComments = "(Here, sentences correct is fraction for which at least one sample is completely correct.)";
    }

    /// <summary>
    /// Defines the error messages.
    /// </summary>
    internal struct ErrorMessages 
    {
        public const string NoTrainingFile = "Cannot find default training set!";

        public const string NoTestingFile = "Cannot find default testing set!";
    }

    /// <summary>
    /// Defines the Tagger names.
    /// </summary>
    internal struct TaggerNames 
    {
        public const string Naive = "Naive graphical model";

        public const string ForwardBackward = "Bayes net";

        public const string Viterbi = "Viterbi";

        public const string Sampling = "Sampling";
    }
}
