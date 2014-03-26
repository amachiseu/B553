using MarkovChain;
using MarkovChain.InferringStrategies;

namespace PartOfSpeechTagging.Tagger 
{
    internal class NaiveInferringTagger : Tagger 
    {
        public NaiveInferringTagger() 
        {
            Name = Messages.NaiveInference;
            FullName = TaggerNames.Naive;
            Strategy = new NaiveInferring<string>();
        }
    }
}
