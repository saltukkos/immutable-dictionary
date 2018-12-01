using System;
using System.Linq;
using NUnit.Framework;

namespace ImmutableDictionary.Tests
{
    [TestFixture]
    public class AwlNodeTests
    {
        private const int Seed = 100; //this produces 132K of unique numbers

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void InsertNodes_CountElements_LogCountHeight(int count)
        {
            var random = new Random(Seed);
            var node = new AwlNode<int>(random.Next());
            for (var i = 0; i < count; ++i)
            {
                node = node.Insert(random.Next());
            }

            //As written in Knuth, Donald E. (2000). p. 460. ISBN 0-201-89685-0
            Assert.True(node.Height < 1.45 * Math.Log(count + 2, 2));
        }

        [Test]
        public void InsertValue_HasSame_ThrowsException()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var unused = new AwlNode<int>(10).Insert(10);
            });
        }

        [Test]
        public void ToString_HasValueRepresentation()
        {
            Assert.True(new AwlNode<int>(1).ToString().Contains("1"));
        }

        [Test]
        public void InsertValues_Enumerate_ReturnedInSortedOrder()
        {
            var random = new Random(Seed);
            var node = new AwlNode<int>(random.Next());
            for (var i = 0; i < 100000; ++i)
            {
                node = node.Insert(random.Next());
            }

            var prevValue = int.MinValue;
            foreach (var value in node)
            {
                Assert.True(value > prevValue);
                prevValue = value;
            }
        }

        [Test]
        public void RemoveMin_HeightOne_ReturnNull()
        {
            var node = new AwlNode<int>(100);
            Assert.Null(node.RemoveMin(out var min));
            Assert.AreEqual(100, min);
        }

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void RemoveMin_HeightMoreThatOne_ReturnNodeWithRemovedMin(int count)
        {
            var random = new Random(Seed);
            var node = new AwlNode<int>(random.Next());
            for (var i = 0; i < count; ++i)
            {
                node = node.Insert(random.Next());
            }

            var values = node.ToList();
            var valuesAfterRemove = node.RemoveMin(out var min)?.ToList();

            Assert.NotNull(valuesAfterRemove);
            Assert.AreEqual(values[0], min);
            CollectionAssert.AreEqual(values.Skip(1), valuesAfterRemove);
            CollectionAssert.AreEqual(node.Skip(1), node.RemoveMin(out _));
        }

        [Test]
        public void Remove_HeightOne_ReturnNull()
        {
            var node = new AwlNode<int>(100);
            Assert.Null(node.Remove(100, out var success));
            Assert.True(success);
        }

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Remove_ValuesExist_ReturnNodeWithRemoved(int count)
        {
            var random = new Random(Seed);
            var node = new AwlNode<int>(random.Next());
            for (var i = 0; i < count; ++i)
            {
                node = node.Insert(random.Next());
            }

            var values = node.ToList();
            var withoutFirst = node.Remove(values[0], out var success1);
            var withoutMiddle = node.Remove(values[count / 2], out var success2);
            var withoutLast = node.Remove(values[count], out var success3);

            Assert.True(success1);
            Assert.True(success2);
            Assert.True(success3);
            CollectionAssert.AreEqual(node.Skip(1), withoutFirst);
            CollectionAssert.AreEqual(node.Take(count / 2).Union(node.Skip(count / 2 + 1)), withoutMiddle);
            CollectionAssert.AreEqual(node.Take(count), withoutLast);
        }

        [Test]
        public void Remove_ValueAbsent_ReturnSameWithFalse()
        {
            var node = new AwlNode<int>(0);
            for (var i = 1; i < 100; ++i)
            {
                node = node.Insert(i * 2);
            }

            for (var i = 0; i < 101; ++i)
            {
                var newNode = node.Remove(i * 2 - 1, out var success);
                Assert.AreSame(node, newNode);
                Assert.False(success);
            }
        }
    }
}