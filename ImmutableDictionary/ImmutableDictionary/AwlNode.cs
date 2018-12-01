using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace ImmutableDictionary
{
    internal class AwlNode<T> : IEnumerable<T>
        where T : IComparable<T>
    {
        [CanBeNull]
        public readonly AwlNode<T> Left;

        [CanBeNull]
        public readonly AwlNode<T> Right;

        [NotNull]
        public readonly T Value;

        public readonly int Height;

        public AwlNode([NotNull] T value)
        {
            Value = value;
            Height = 1;
        }

        private AwlNode([NotNull] AwlNode<T> original, [NotNull] T value)
        {
            Left = original.Left;
            Right = original.Right;
            Height = original.Height;
            Value = value;
        }

        private AwlNode([CanBeNull] AwlNode<T> left, [CanBeNull] AwlNode<T> right, [NotNull] T value)
        {
            Left = left;
            Right = right;
            Height = Math.Max(left.SafeHeight(), right.SafeHeight()) + 1;
            Value = value;
        }

        private AwlNode(
            [CanBeNull] AwlNode<T> leftLeft,
            [CanBeNull] AwlNode<T> leftRight,
            [CanBeNull] AwlNode<T> rightLeft,
            [CanBeNull] AwlNode<T> rightRight,
            [NotNull] T leftValue,
            [NotNull] T rightValue,
            [NotNull] T myValue)
        {
            Left = new AwlNode<T>(leftLeft, leftRight, leftValue);
            Right = new AwlNode<T>(rightLeft, rightRight, rightValue);
            Height = Math.Max(Left.Height, Right.Height) + 1;
            Value = myValue;
        }

        [Pure]
        [NotNull]
        private AwlNode<T> RotateRight()
        {
            Debug.Assert(Left != null);

            var a = Left.Left;
            var b = Left.Right;
            var c = Right;
            var rotatedThis = new AwlNode<T>(b, c, Value);
            var rotatedLeft = new AwlNode<T>(a, rotatedThis, Left.Value);

            return rotatedLeft;
        }

        [Pure]
        [NotNull]
        private AwlNode<T> RotateLeft()
        {
            Debug.Assert(Right != null);

            var a = Left;
            var b = Right.Left;
            var c = Right.Right;

            var rotatedThis = new AwlNode<T>(a, b, Value);
            var rotatedRight = new AwlNode<T>(rotatedThis, c, Right.Value);

            return rotatedRight;
        }

        [Pure]
        [NotNull]
        private AwlNode<T> Balance()
        {
            if (this.BalanceFactor() == 2)
            {
                Debug.Assert(Right != null);

                if (Right.BalanceFactor() >= 0)
                {
                    return RotateLeft();
                }

                Debug.Assert(Right.Left != null);

                var a = Left;
                var b = Right.Left.Left;
                var c = Right.Left.Right;
                var d = Right.Right;

                return new AwlNode<T>(a, b, c, d, Value, Right.Value, Right.Left.Value);
            }

            if (this.BalanceFactor() == -2)
            {
                Debug.Assert(Left != null);

                if (Left.BalanceFactor() <= 0)
                {
                    return RotateRight();
                }

                Debug.Assert(Left.Right != null);

                var a = Left.Left;
                var b = Left.Right.Left;
                var c = Left.Right.Right;
                var d = Right;

                return new AwlNode<T>(a, b, c, d, Left.Value, Value, Left.Right.Value);
            }

            return new AwlNode<T>(this, Value);
        }

        [Pure]
        [NotNull]
        public AwlNode<T> Insert([NotNull] T newValue)
        {
            var compareResult = newValue.CompareTo(Value);
            if (compareResult == 0)
            {
                throw new NotSupportedException(
                    $"value {newValue} can't be inserted as it's already present");
            }

            var newLeft = Left;
            var newRight = Right;
            if (compareResult < 0)
            {
                newLeft = Left?.Insert(newValue) ?? new AwlNode<T>(newValue);
            }

            if (compareResult > 0)
            {
                newRight = Right?.Insert(newValue) ?? new AwlNode<T>(newValue);
            }

            return new AwlNode<T>(newLeft, newRight, Value).Balance();
        }

        [Pure]
        [CanBeNull]
        public AwlNode<T> RemoveMin([NotNull] out T minValue)
        {
            if (Left == null)
            {
                Debug.Assert(Right?.Left == null);
                Debug.Assert(Right?.Right == null);
                minValue = Value;
                return Right;
            }

            var newLeft = Left.RemoveMin(out minValue);
            return new AwlNode<T>(newLeft, Right, Value).Balance();
        }

        [Pure]
        [CanBeNull]
        public AwlNode<T> Remove([NotNull] T valueToRemove, out bool success)
        {
            var compareResult = valueToRemove.CompareTo(Value);
            var value = Value;
            var newLeft = Left;
            var newRight = Right;

            if (compareResult < 0)
            {
                if (Left == null)
                {
                    success = false;
                    return this;
                }

                newLeft = Left.Remove(valueToRemove, out success);
            }
            else if (compareResult > 0)
            {
                if (Right == null)
                {
                    success = false;
                    return this;
                }

                newRight = Right.Remove(valueToRemove, out success);
            }
            else
            {
                success = true;
                if (newRight == null)
                {
                    return newLeft;
                }

                newRight = newRight.RemoveMin(out value);
            }

            return success ? new AwlNode<T>(newLeft, newRight, value).Balance() : this;
        }

        [CanBeNull]
        public AwlNode<T> FindNode([NotNull] T valueToFind)
        {
            var compareResult = valueToFind.CompareTo(Value);
            if (compareResult > 0)
            {
                return Right?.FindNode(valueToFind);
            }

            if (compareResult < 0)
            {
                return Left?.FindNode(valueToFind);
            }

            return this;
        }

        [Pure]
        public IEnumerator<T> GetEnumerator()
        {
            var stack = new Stack<AwlNode<T>>((int) Math.Pow(2, Height));
            var visited = new HashSet<AwlNode<T>>();
            stack.Push(this);
            while (stack.Any())
            {
                var node = stack.Peek();
                Debug.Assert(node != null, nameof(node) + " != null");

                if (visited.Contains(node))
                {
                    yield return node.Value;
                    stack.Pop();
                    if (node.Right != null && !visited.Contains(node.Right))
                    {
                        stack.Push(node.Right);
                    }

                    continue;
                }

                visited.Add(node);

                if (node.Left != null && !visited.Contains(node.Left))
                {
                    stack.Push(node.Left);
                }
            }
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [Pure]
        public override string ToString()
        {
            return $"Node{{{Value}}}";
        }
    }

    internal static class AwlNodeExtensions
    {
        public static int SafeHeight<T>([CanBeNull] this AwlNode<T> node) where T : IComparable<T>
        {
            return node?.Height ?? 0;
        }

        public static int BalanceFactor<T>([NotNull] this AwlNode<T> node) where T : IComparable<T>
        {
            return node.Right.SafeHeight() - node.Left.SafeHeight();
        }
    }
}