using System;

using PartOfSpeechTagging.Tagger;

namespace PartOfSpeechTagging 
{
	internal class MainClass 
	{
		public static void Main (string[] args) 
        {
            if (null == args) 
            {
                // Never gets here.
                // The args can be empty but never null.
            }

            TaggerManager manager = new TaggerManager();

            manager.TrainHiddenMarkovModel(args.Length > 0 ? args[0] : null);
            manager.Tag(args.Length > 1 ? args[1] : null, 
                new Tagger.Tagger[] 
                {
                    new NaiveInferringTagger(), 
                    new ForwardBackwardTagger(), 
                    new ViterbiTagger(), 
                    new SamplingTagger { InferenceCount = 5 }
                });

            Console.ReadLine();
        }
	}
}
