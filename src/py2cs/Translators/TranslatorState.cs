using System.Collections.Immutable;

namespace Py2Cs.Translators
{
    public class TranslatorState
    {
        public TranslatorState(ImmutableDictionary<string, string> variables)
        {
            this.Variables = variables;
        }

        public ImmutableDictionary<string, string> Variables { get; }

        public TranslatorState WithVariable(string name, string value)
        {
            return new TranslatorState(Variables.Add(name, value));
        }
    }

}