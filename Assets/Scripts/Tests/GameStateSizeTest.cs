using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    /// <summary>
    /// GC.GetAllocatedBytesForCurrentThread (used in MemoryTest.cs) always reports 0 under
    /// Unity's Mono runtime, so it can't be used to measure GameState's footprint.
    /// This test instead walks the object graph with reflection and sums up an estimate
    /// of the actual data size, which works reliably and is what we actually care about.
    /// </summary>
    public class GameStateSizeTest
    {
        [Test]
        public void TestGameStateByteSize()
        {
            GameState gameState = GameState.CreateInitial();

            long totalBytes = ObjectSizeEstimator.EstimateSize(gameState);

            Debug.Log($"Estimated GameState size: {totalBytes} bytes");
            Assert.That(totalBytes, Is.GreaterThan(0));
        }

        [Test]
        public void TestGameStateByteSizeAfter20Moves()
        {
            GameState gameState = GameState.CreateInitial();
            Move[] moves = new Move[20];

            long totalBytes = ObjectSizeEstimator.EstimateSize(gameState) 
                              + ObjectSizeEstimator.EstimateSize(moves);

            Debug.Log($"Estimated GameState size after 20 moves: {totalBytes} bytes");
            Assert.That(totalBytes, Is.GreaterThan(0));
        }

        [Test]
        public void TestClientGameStateByteSize()
        {
            GameState gameState = GameState.CreateInitial();
            
            ClientGameState clientGameState = gameState.ToClientGameState(gameState.ToPlay);

            long totalBytes = ObjectSizeEstimator.EstimateSize(clientGameState);
            
            Debug.Log($"Estimated ClientGameState size: {totalBytes} bytes");
            Assert.That(totalBytes, Is.GreaterThan(0));
        }
        
        [Test]
        public void TestClientGameStateByteSizeAfter20Moves()
        {
            GameState gameState = GameState.CreateInitial();
            Move[] moves = new Move[20];
            
            ClientGameState clientGameState = gameState.ToClientGameState(gameState.ToPlay);
            
            long totalBytes = ObjectSizeEstimator.EstimateSize(clientGameState)
                              + ObjectSizeEstimator.EstimateSize(moves);

            Debug.Log($"Estimated ClientGameState size after 20 moves: {totalBytes} bytes");
            Assert.That(totalBytes, Is.GreaterThan(0));
        }
    }

    /// <summary>
    /// Rough reflection-based estimate of the number of bytes of data held by an object
    /// graph. Not a precise measure of CLR memory layout (that includes object headers,
    /// padding, etc.), but a stable, non-zero estimate of how much actual data is stored.
    /// </summary>
    internal static class ObjectSizeEstimator
    {
        private const int ObjectOverheadBytes = 16;
        private const int ArrayOverheadBytes = 24;
        private const int StringOverheadBytes = 26;

        public static long EstimateSize(object obj)
        {
            return EstimateSize(obj, new HashSet<object>(ReferenceComparer.Instance));
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) =>
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        private static long EstimateSize(object obj, HashSet<object> visited)
        {
            if (obj is null)
            {
                return 0;
            }

            Type type = obj.GetType();

            if (type.IsPrimitive)
            {
                return GetPrimitiveSize(type);
            }

            if (obj is string str)
            {
                return StringOverheadBytes + str.Length * sizeof(char);
            }

            if (type.IsEnum)
            {
                return GetPrimitiveSize(Enum.GetUnderlyingType(type));
            }

            // Only reference types can create cycles / be shared, so only track those.
            if (!type.IsValueType && !visited.Add(obj))
            {
                return 0;
            }

            if (type.IsArray)
            {
                Array array = (Array)obj;
                long size = ArrayOverheadBytes;
                foreach (object element in array)
                {
                    size += EstimateSize(element, visited);
                }

                return size;
            }

            long total = type.IsValueType ? 0 : ObjectOverheadBytes;

            foreach (FieldInfo field in GetAllInstanceFields(type))
            {
                total += EstimateSize(field.GetValue(obj), visited);
            }

            return total;
        }

        private static IEnumerable<FieldInfo> GetAllInstanceFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (Type current = type; current is not null && current != typeof(object); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(flags))
                {
                    yield return field;
                }
            }
        }

        private static long GetPrimitiveSize(Type type)
        {
            if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
            {
                return 1;
            }

            if (type == typeof(short) || type == typeof(ushort) || type == typeof(char))
            {
                return 2;
            }

            if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
            {
                return 4;
            }

            if (type == typeof(long) || type == typeof(ulong) || type == typeof(double))
            {
                return 8;
            }

            return System.Runtime.InteropServices.Marshal.SizeOf(type);
        }
    }
}
