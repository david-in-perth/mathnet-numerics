﻿// <copyright file="Vector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the generic class for <c>Vector</c> classes.
    /// </summary>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    [Serializable]
    public abstract partial class Vector<T> :
        IFormattable, IEquatable<Vector<T>>, IList, IList<T>
#if !PORTABLE
        , ICloneable
#endif
        where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the Vector class.
        /// </summary>
        protected Vector(VectorStorage<T> storage)
        {
            Storage = storage;
            Count = storage.Length;
        }

        public static readonly VectorBuilder<T> Build = BuilderInstance<T>.Vector;

        /// <summary>
        /// Gets the raw vector data storage.
        /// </summary>
        public VectorStorage<T> Storage { get; private set; }

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative or
        /// greater than the size of the vector.</exception>
        public T this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
            get { return Storage[index]; }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
            set { Storage[index] = value; }
        }

        /// <summary>Gets the value at the given <paramref name="index"/> without range checking..</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
        public T At(int index)
        {
            return Storage.At(index);
        }

        /// <summary>Sets the <paramref name="value"/> at the given <paramref name="index"/> without range checking..</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <param name="value">The value to set.</param>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
        public void At(int index, T value)
        {
            Storage.At(index, value);
        }

        /// <summary>
        /// Resets all values to zero.
        /// </summary>
        public void Clear()
        {
            Storage.Clear();
        }

        /// <summary>
        /// Sets all values of a subvector to zero.
        /// </summary>
        public void ClearSubVector(int index, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count", Resources.ArgumentMustBePositive);
            }

            if (index + count > Count || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Storage.Clear(index, count);
        }

        /// <summary>
        /// Set all values whose absolute value is smaller than the threshold to zero, in-place.
        /// </summary>
        public abstract void CoerceZero(double threshold);

        /// <summary>
        /// Set all values that meet the predicate to zero, in-place.
        /// </summary>
        public void CoerceZero(Func<T, bool> zeroPredicate)
        {
            MapInplace(x => zeroPredicate(x) ? Zero : x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Returns a deep-copy clone of the vector.
        /// </summary>
        /// <returns>A deep-copy clone of the vector.</returns>
        public Vector<T> Clone()
        {
            var result = Build.SameAs(this);
            Storage.CopyToUnchecked(result.Storage, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Set the values of this vector to the given values.
        /// </summary>
        /// <param name="values">The array containing the values to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="values"/> is not the same size as this vector.</exception>
        public void SetValues(T[] values)
        {
            var source = new DenseVectorStorage<T>(Count, values);
            source.CopyTo(Storage);
        }

        /// <summary>
        /// Copies the values of this vector into the target vector.
        /// </summary>
        /// <param name="target">The vector to copy elements into.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="target"/> is not the same size as this vector.</exception>
        public void CopyTo(Vector<T> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Storage.CopyTo(target.Storage);
        }

        /// <summary>
        /// Creates a vector containing specified elements.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="count">The number of elements to copy.</param>
        /// <returns>A vector containing a copy of the specified elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><list><item>If <paramref name="index"/> is not positive or
        /// greater than or equal to the size of the vector.</item>
        /// <item>If <paramref name="index"/> + <paramref name="count"/> is greater than or equal to the size of the vector.</item>
        /// </list></exception>
        /// <exception cref="ArgumentException">If <paramref name="count"/> is not positive.</exception>
        public Vector<T> SubVector(int index, int count)
        {
            var target = Build.SameAs(this, count);
            Storage.CopySubVectorTo(target.Storage, index, 0, count, ExistingData.AssumeZeros);
            return target;
        }

        /// <summary>
        /// Copies the values of a given vector into a region in this vector.
        /// </summary>
        /// <param name="index">The field to start copying to</param>
        /// <param name="count">The number of fields to cpy. Must be positive.</param>
        /// <param name="subVector">The sub-vector to copy from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="subVector"/> is <see langword="null" /></exception>
        public void SetSubVector(int index, int count, Vector<T> subVector)
        {
            if (subVector == null)
            {
                throw new ArgumentNullException("subVector");
            }

            subVector.Storage.CopySubVectorTo(Storage, 0, index, count);
        }

        /// <summary>
        /// Copies the requested elements from this vector to another.
        /// </summary>
        /// <param name="destination">The vector to copy the elements to.</param>
        /// <param name="sourceIndex">The element to start copying from.</param>
        /// <param name="targetIndex">The element to start copying to.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopySubVectorTo(Vector<T> destination, int sourceIndex, int targetIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            // TODO: refactor range checks
            Storage.CopySubVectorTo(destination.Storage, sourceIndex, targetIndex, count);
        }

        /// <summary>
        /// Returns the data contained in the vector as an array.
        /// </summary>
        /// <returns>
        /// The vector's data as an array.
        /// </returns>
        public T[] ToArray()
        {
            var result = new DenseVectorStorage<T>(Count);
            Storage.CopyToUnchecked(result, ExistingData.AssumeZeros);
            return result.Data;
        }

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>
        /// This vector as a column matrix.
        /// </returns>
        public Matrix<T> ToColumnMatrix()
        {
            var result = Matrix<T>.Build.SameAs(this, Count, 1);
            Storage.CopyToColumnUnchecked(result.Storage, 0, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>
        /// This vector as a row matrix.
        /// </returns>
        public Matrix<T> ToRowMatrix()
        {
            var result = Matrix<T>.Build.SameAs(this, 1, Count);
            Storage.CopyToRowUnchecked(result.Storage, 0, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector.
        /// </summary>
        /// <remarks>
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<T> Enumerate()
        {
            return Storage.Enumerate();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector.
        /// </summary>
        /// <remarks>
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<T> Enumerate(Zeros zeros = Zeros.Include)
        {
            switch (zeros)
            {
                case Zeros.AllowSkip:
                    return Storage.EnumerateNonZero();
                default:
                    return Storage.Enumerate();
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the element index
        /// and the second value being the value of the element at that index.
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<Tuple<int, T>> EnumerateIndexed()
        {
            return Storage.EnumerateIndexed();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the element index
        /// and the second value being the value of the element at that index.
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<Tuple<int, T>> EnumerateIndexed(Zeros zeros = Zeros.Include)
        {
            switch (zeros)
            {
                case Zeros.AllowSkip:
                    return Storage.EnumerateNonZeroIndexed();
                default:
                    return Storage.EnumerateIndexed();
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all non-zero values of the vector.
        /// </summary>
        /// <remarks>
        /// The enumerator will skip all elements with a zero value.
        /// </remarks>
        [Obsolete("Use Enumerate(Zeros.AllowSkip) instead. Will be removed in v4.")]
        public IEnumerable<T> EnumerateNonZero()
        {
            return Storage.EnumerateNonZero();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all non-zero values of the vector and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the element index
        /// and the second value being the value of the element at that index.
        /// The enumerator will skip all elements with a zero value.
        /// </remarks>
        [Obsolete("Use EnumerateIndexed(Zeros.AllowSkip) instead. Will be removed in v4.")]
        public IEnumerable<Tuple<int, T>> EnumerateNonZeroIndexed()
        {
            return Storage.EnumerateNonZeroIndexed();
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value with its result.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapInplace(Func<T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.MapToUnchecked(Storage, f, zeros, ExistingData.AssumeZeros);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value with its result.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapIndexedInplace(Func<int, T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.MapIndexedToUnchecked(Storage, f, zeros, ExistingData.AssumeZeros);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void Map<TU>(Func<T, TU> f, Vector<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            // TODO: in v4 update this method to replace TU with T (consistent with Matrix, see MapConvert)
            Storage.MapTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapIndexed<TU>(Func<int, T, TU> f, Vector<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            // TODO: in v4 update this method to replace TU with T (consistent with Matrix, see MapIndexedConvert)
            Storage.MapIndexedTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapConvert<TU>(Func<T, TU> f, Vector<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            Storage.MapTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapIndexedConvert<TU>(Func<int, T, TU> f, Vector<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            Storage.MapIndexedTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this vector and returns the results as a new vector.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public Vector<TU> Map<TU>(Func<T, TU> f, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var result = Vector<TU>.Build.SameAs(this);
            Storage.MapToUnchecked(result.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Applies a function to each value of this vector and returns the results as a new vector.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public Vector<TU> MapIndexed<TU>(Func<int, T, TU> f, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var result = Vector<TU>.Build.SameAs(this);
            Storage.MapIndexedToUnchecked(result.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }
    }
}
