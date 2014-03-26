using MarkovChain;
using MarkovChain.InferringStrategies;

namespace PartOfSpeechTagging.Tagger 
{
    internal class ViterbiTagger : Tagger 
    {
        public ViterbiTagger() 
        {
            Name = Messages.ViterbiInference;
            FullName = TaggerNames.Viterbi;
            Strategy = new Viterbi<string>();
        }
    }
}
