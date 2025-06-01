using System.Collections.Generic;

namespace Lucky.Kits.Collections
{
    public class Trie<T>
    {
        private DefaultDict<char, Trie<T>> children;
        private List<T> values = new();
        private bool _ignoreCase;

        public Trie(bool ignoreCase)
        {
            _ignoreCase = ignoreCase;
            children = new DefaultDict<char, Trie<T>>(() => new Trie<T>(ignoreCase));
        }

        public void Add(string pattern, T value, char delimiter = '\0')
        {
            Trie<T> cur = this;
            foreach (char ch in pattern)
            {
                char c = ParseChar(ch);
                if (c == delimiter)
                    break;
                cur = children[c];
            }

            cur.values.Add(value);
        }

        public List<T> Get(string pattern)
        {
            Trie<T> cur = this;
            foreach (char ch in pattern)
            {
                char c = ParseChar(ch);
                cur = children[c];
            }

            return cur.values;
        }

        private char ParseChar(char ch)
        {
            if (_ignoreCase)
                return char.ToLower(ch);
            return ch;
        }
    }
}