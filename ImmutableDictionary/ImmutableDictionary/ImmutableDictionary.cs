using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ImmutableDictionary
{
    public class ImmutableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private struct ComparableKeyValuePair : IComparable<ComparableKeyValuePair>
        {
            public ComparableKeyValuePair([NotNull] TKey key, [CanBeNull] TValue value)
            {
                Key = key;
                Value = value;
            }

            [NotNull]
            public TKey Key { get; }

            [CanBeNull]
            public TValue Value { get; }

            public int CompareTo(ComparableKeyValuePair other)
            {
                return Key.CompareTo(other.Key);
            }
        }

        [CanBeNull]
        private readonly AwlNode<ComparableKeyValuePair> _rootNode;

        public ImmutableDictionary()
        {
        }

        private ImmutableDictionary([CanBeNull] AwlNode<ComparableKeyValuePair> rootNode, int count)
        {
            _rootNode = rootNode;
            Count = count;
        }

        [Pure]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_rootNode == null)
            {
                yield break;
            }

            foreach (var pair in _rootNode)
            {
                yield return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
            }
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }

        [Pure]
        public bool ContainsKey(TKey key)
        {
            if (_rootNode == null)
            {
                return false;
            }

            foreach (var actualKey in _rootNode.Select(n => n.Key))
            {
                var compareResult = actualKey.CompareTo(key);
                if (compareResult == 0)
                {
                    return true;
                }

                if (compareResult > 0)
                {
                    return false;
                }
            }

            return false;
        }

        [Pure]
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            if (_rootNode == null)
            {
                return false;
            }

            foreach (var pair in _rootNode)
            {
                var compareResult = pair.Key.CompareTo(key);
                if (compareResult == 0)
                {
                    value = pair.Value;
                    return true;
                }

                if (compareResult > 0)
                {
                    return false;
                }
            }

            return false;
        }

        public TValue this[TKey key]
        {
            [Pure]
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (!TryGetValue(key, out var value))
                {
                    throw new KeyNotFoundException(key.ToString());
                }

                return value;
            }
        }

        [Pure]
        [NotNull]
        public ImmutableDictionary<TKey, TValue> Insert([NotNull] TKey key, [CanBeNull] TValue value)
        {
            var pair = new ComparableKeyValuePair(key, value);
            var nodeWithRemovedOldValue = _rootNode?.Remove(pair, out _);
            if (nodeWithRemovedOldValue == null)
            {
                return new ImmutableDictionary<TKey, TValue>(
                    new AwlNode<ComparableKeyValuePair>(pair), 1);
            }

            return new ImmutableDictionary<TKey, TValue>(_rootNode.Insert(pair), Count + 1);
        }

        [Pure]
        [NotNull]
        public ImmutableDictionary<TKey, TValue> Remove([NotNull] TKey key, out bool success)
        {
            if (_rootNode == null)
            {
                success = false;
                return this;
            }

            var pair = new ComparableKeyValuePair(key, default(TValue));
            var nodeWithoutKey = _rootNode.Remove(pair, out success);
            if (!success)
            {
                return this;
            }
            return new ImmutableDictionary<TKey, TValue>(nodeWithoutKey, Count - 1);
        }

        public IEnumerable<TKey> Keys
        {
            [Pure]
            get
            {
                if (_rootNode == null)
                {
                    return new TKey[0];
                }

                return _rootNode.Select(pair => pair.Key);
            }
        }

        public IEnumerable<TValue> Values
        {
            [Pure]
            get
            {
                if (_rootNode == null)
                {
                    return new TValue[0];
                }

                return _rootNode.Select(pair => pair.Value);
            }
        }
    }
}