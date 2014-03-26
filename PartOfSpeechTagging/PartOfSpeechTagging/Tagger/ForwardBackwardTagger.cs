using MarkovChain;
using MarkovChain.InferringStrategies;

namespace PartOfSpeechTagging.Tagger 
{
    internal class ForwardBackwardTagger : Tagger 
    {
        public ForwardBackwardTagger() 
        {
            Name = Messages.BayesInference;
            FullName = TaggerNames.ForwardBackward;
            Strategy = new ForwardBackward<string>();
        }
    }
}
