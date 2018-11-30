using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ImmutableDictionary.Tests
{
    [TestFixture]
    public class ImmutableDictionaryTests
    {
        [Test]
        public void GetValue_WasAdded_Returns()
        {
            var dictionary = new ImmutableDictionary<int, int>().Insert(1, 2);
            Assert.AreEqual(2, dictionary[1]);
        }

        [Test]
        public void GetValue_Absent_Throws()
        {
            var dictionary = new ImmutableDictionary<int, int>().Insert(1, 2);
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var _ = dictionary[100];
            });

            dictionary = new ImmutableDictionary<int, int>();
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var _ = dictionary[100];
            });
        }

        [Test]
        public void InsertValue_ValueExist_Overwritten()
        {
            var dictionary = new ImmutableDictionary<int, int>().Insert(1, 2).Insert(1, 3);
            Assert.AreEqual(3, dictionary[1]);
        }

        [Test]
        public void RemoveKey_KeyExist_Removed()
        {
            var dictionary = new ImmutableDictionary<int, int>()
                .Insert(1, 2)
                .Insert(2, 3)
                .Remove(1, out var successRemove);

            Assert.True(successRemove);
            Assert.False(dictionary.TryGetValue(1, out _));

            dictionary = dictionary.Remove(2, out successRemove);
            Assert.True(successRemove);
            Assert.False(dictionary.TryGetValue(2, out _));
        }

        [Test]
        public void RemoveKey_KeyNotExist_NotRemoved()
        {
            var dictionary = new ImmutableDictionary<int, int>()
                .Insert(1, 2)
                .Remove(1, out _)
                .Remove(1, out var successRemove);

            Assert.False(successRemove);
            Assert.False(dictionary.TryGetValue(1, out _));

            dictionary.Insert(2, 0).Remove(1, out successRemove);
            Assert.False(successRemove);
        }

        [Test]
        public void ModifyDictionary_OriginalNotChanged_Test()
        {
            var empty = new ImmutableDictionary<int, int>();
            // ReSharper disable InconsistentNaming
            var map_1_10 = empty.Insert(1, 10);
            var map_1_10_2_20 = map_1_10.Insert(2, 20);
            var map_1_10_2_30 = map_1_10.Insert(2, 30);
            var map_2_20 = map_1_10_2_20.Remove(1, out _);
            // ReSharper restore InconsistentNaming
            Assert.False(empty.Any());
            Assert.False(empty.ContainsKey(1));
            Assert.False(empty.ContainsKey(2));
            Assert.True(map_1_10.Any());
            Assert.True(map_1_10.ContainsKey(1));
            Assert.False(map_1_10.ContainsKey(2));
            Assert.True(map_1_10_2_30.ContainsKey(1));
            Assert.True(map_1_10_2_30.ContainsKey(2));
            Assert.False(map_2_20.ContainsKey(1));
            Assert.True(map_1_10_2_20.Count == 2);
            Assert.True(map_1_10_2_30.Count == 2);
            Assert.True(map_1_10_2_20[2] == 20);
            Assert.True(map_1_10_2_30[2] == 30);
        }

        [Test]
        public void EnumerateDictionary_ContainsKeysAndValues_Test()
        {
            var dictionary = new ImmutableDictionary<int, int>();
            for (var i = 0; i < 10; ++i)
            {
                dictionary = dictionary.Insert(i, i);
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, 10), dictionary.Values);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), dictionary.Keys);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), dictionary.Select(pair => pair.Key));
        }
    }
}