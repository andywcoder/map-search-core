using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Util;
using System.Collections.Generic;
using System.IO;

namespace Santolibre.RavenDB.Analyzers
{
    public class LowerCaseNonDiacriticEnglishStopWordsAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new StandardTokenizer(Version.LUCENE_30, @reader);
            result = new StandardFilter(result);
            result = new LowerCaseFilter(result);
            result = new StopFilter(false, result, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            result = new ASCIIFoldingFilter(result);
            return result;
        }

        public static List<string> Tokenize(string value)
        {
            using (TextReader sr = new StringReader(value))
            {
                var analyzer = new LowerCaseNonDiacriticEnglishStopWordsAnalyzer();
                var tokenStream = analyzer.TokenStream(null, sr);
                var termAttribute = tokenStream.GetAttribute<ITermAttribute>();
                var terms = new List<string>();
                while (tokenStream.IncrementToken())
                {
                    terms.Add(termAttribute.Term);
                }
                return terms;
            }
        }
    }
}
