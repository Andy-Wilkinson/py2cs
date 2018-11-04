using System.Collections.Immutable;

namespace Py2Cs.Translators
{
    public class TranslatorState
    {
        private static TranslatorState _empty = new TranslatorState(ImmutableDictionary<string, ExpressionResult>.Empty);

        public TranslatorState(ImmutableDictionary<string, ExpressionResult> variables)
        {
            this.Variables = variables;
        }

        public ImmutableDictionary<string, ExpressionResult> Variables { get; }

        public TranslatorState WithVariable(string name, ExpressionResult value)
        {
            return new TranslatorState(Variables.Add(name, value));
        }

        public static TranslatorState Empty => _empty;
    }

}