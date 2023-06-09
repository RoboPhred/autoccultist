namespace AutoccultistNS.Yaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A configuration class that can load either one item, or an array of items.
    /// </summary>
    /// <typeparam name="T">The type of item to load.</typeparam>
    public class OneOrMany<T> : IList<T>, System.IConvertible
    {
        private readonly List<T> contents;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOrMany{T}"/> class.
        /// </summary>
        public OneOrMany()
        {
            this.contents = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOrMany{T}"/> class.
        /// </summary>
        /// <param name="source">The enumerable to copy items from.</param>
        public OneOrMany(IEnumerable<T> source)
        {
            this.contents = source.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOrMany{T}"/> class.
        /// </summary>
        /// <param name="source">The enumerable to copy items from.</param>
        public OneOrMany(IEnumerable source)
        {
            this.contents = source.Cast<T>().ToList();
        }

        /// <inheritdoc/>
        public int Count => this.contents.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public T this[int index] { get => this.contents[index]; set => this.contents[index] = value; }

        /// <inheritdoc/>
        public void Add(T item)
        {
            this.contents.Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.contents.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return this.contents.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.contents.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return this.contents.GetEnumerator();
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return this.contents.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            this.contents.Insert(index, item);
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            return this.contents.Remove(item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            this.contents.RemoveAt(index);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.contents.GetEnumerator();
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            // Here for the yaml parser.
            // We don't really need this, as we are careful not to cache these when the rest of the parser might want singles.
            // Still, it is nice to have.
            if (conversionType.IsAssignableFrom(typeof(T)))
            {
                if (this.Count > 1)
                {
                    throw new InvalidCastException("Cannot convert OneOrMany of multiple items to a single item.");
                }

                return this.contents.Single();
            }

            throw new InvalidCastException($"Cannot convert OneOrMany of {typeof(T)} to {conversionType}.");
        }
    }
}
