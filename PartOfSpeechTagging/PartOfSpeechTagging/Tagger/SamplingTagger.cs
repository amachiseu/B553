using MarkovChain;
using MarkovChain.InferringStrategies;

namespace PartOfSpeechTagging.Tagger 
{
    internal class SamplingTagger : Tagger 
    {
        public SamplingTagger() 
        {
            Name = Messages.SampleInference;
            FullName = TaggerNames.Sampling;
            Comments = ReportMessages.SampleComments;
            Strategy = new Sampling<string>();
        }
    }
}
