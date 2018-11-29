﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BencodeNET
{
    /// <summary>
    /// Represents a bencoded list of <see cref="IBObject"/>.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IList{IBObject}"/>.
    /// </remarks>
    public class BList : BObject<IList<IBObject>>, IList<IBObject>
    {
        private readonly IList<IBObject> fValue = new List<IBObject>();

        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<IBObject> Value
        {
            get {
                return fValue;
            }
        }
        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        { }

        /// <summary>
        /// Creates a list from strings using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <param name="strings"></param>
        public BList(IEnumerable<string> strings)
            : this(strings, Encoding.UTF8)
        { }

        /// <summary>
        /// Creates a list from strings using the specified encoding.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="encoding"></param>
        public BList(IEnumerable<string> strings, Encoding encoding)
        {
            foreach (var str in strings) {
                Add(str, encoding);
            }
        }

        /// <summary>
        /// Creates a list from en <see cref="IEnumerable{T}"/> of <see cref="IBObject"/>.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<IBObject> objects)
        {
            fValue = new List<IBObject>(objects);
        }

        /// <summary>
        /// Adds a string to the list using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <param name="value"></param>
        public void Add(string value)
        {
            Add(new BString(value));
        }

        /// <summary>
        /// Adds a string to the list using the specified encoding.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public void Add(string value, Encoding encoding)
        {
            Add(new BString(value, encoding));
        }

        /// <summary>
        /// Adds an integer to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            Add((IBObject)new BNumber(value));
        }

        /// <summary>
        /// Adds a long to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(long value)
        {
            Add((IBObject)new BNumber(value));
        }

        /// <summary>
        /// Appends a list to the end of this instance.
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(BList list)
        {
            foreach (var obj in list) {
                Add(obj);
            }
        }

        /// <summary>
        /// Gets the object at the specified index as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to cast the object to.</typeparam>
        /// <param name="index">The index in the list to get the object from.</param>
        /// <returns>The object at the specified index as the specified type or null if the object is not of that type.</returns>
        public T Get<T>(int index) where T : class, IBObject
        {
            return this[index] as T;
        }

        /// <summary>
        /// Assumes all elements are <see cref="BString"/>
        /// and returns an enumerable of their string representation.
        /// </summary>
        public IEnumerable<string> AsStrings()
        {
            return AsStrings(Encoding.UTF8);
        }

        /// <summary>
        /// Assumes all elements are <see cref="BString"/> and returns
        /// an enumerable of their string representation using the specified encoding.
        /// </summary>
        public IEnumerable<string> AsStrings(Encoding encoding)
        {
            IList<BString> bstrings = AsType<BString>();
            return bstrings.Select(x => x.ToString(encoding));
        }

        /// <summary>
        /// Assumes all elements are <see cref="BNumber"/>
        /// and returns an enumerable of their <c>long</c> value.
        /// </summary>
        public IEnumerable<long> AsNumbers()
        {
            IList<BNumber> bnumbers = AsType<BNumber>();
            return bnumbers.Select(x => x.Value);
        }

        /// <summary>
        /// Attempts to cast all elements to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">
        /// An element is not of type <typeparamref name="T"/>.
        /// </exception>
        public BList<T> AsType<T>() where T : class, IBObject
        {
            try {
                return new BList<T>(this.Cast<T>());
            } catch (InvalidCastException ex) {
                throw new InvalidCastException(string.Format("Not all elements are of type '{0}'.", typeof(T).FullName), ex);
            }
        }

        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('l');
            foreach (var item in this) {
                item.EncodeTo(stream);
            }
            stream.Write('e');
        }

        #region IList<IBObject> Members

        public int Count
        {
            get {
                return fValue.Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return fValue.IsReadOnly;
            }
        }

        public IBObject this[int index]
        {
            get { return fValue[index]; }
            set {
                if (value == null) throw new ArgumentNullException("value");
                fValue[index] = value;
            }
        }

        public void Add(IBObject item)
        {
            if (item == null) throw new ArgumentNullException("item");
            fValue.Add(item);
        }

        public void Clear()
        {
            fValue.Clear();
        }

        public bool Contains(IBObject item)
        {
            return fValue.Contains(item);
        }

        public void CopyTo(IBObject[] array, int arrayIndex)
        {
            fValue.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IBObject> GetEnumerator()
        {
            return fValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IBObject item)
        {
            return fValue.IndexOf(item);
        }

        public void Insert(int index, IBObject item)
        {
            fValue.Insert(index, item);
        }

        public bool Remove(IBObject item)
        {
            return fValue.Remove(item);
        }

        public void RemoveAt(int index)
        {
            fValue.RemoveAt(index);
        }

        #endregion
    }

    /// <summary>
    /// Represents a bencoded list of type <typeparamref name="T"/> which implements <see cref="IBObject"/> .
    /// </summary>
    public sealed class BList<T> : BList, IList<T> where T : class, IBObject
    {
        private readonly IList<IBObject> fValue = new List<IBObject>();

        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<IBObject> Value
        {
            get {
                return fValue;
            }
        }
        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        { }

        /// <summary>
        /// Creates a list from the specified objects.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<T> objects)
        {
            fValue = objects.Cast<IBObject>().ToList();
        }

        #region IList<T> Members

        public new T this[int index]
        {
            get {
                var obj = fValue[index] as T;
                if (obj == null) throw new InvalidCastException(string.Format("The object at index {0} is not of type {1}", index, typeof(T).FullName));
                return obj;
            }
            set {
                if (value == null) throw new ArgumentNullException("value");
                fValue[index] = value;
            }
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            fValue.Add(item);
        }

        public bool Contains(T item)
        {
            return fValue.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            fValue.CopyTo(array.Cast<IBObject>().ToArray(), arrayIndex);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            var i = 0;
            var enumerator = fValue.GetEnumerator();
            while (enumerator.MoveNext()) {
                var obj = enumerator.Current as T;
                if (obj == null) throw new InvalidCastException(string.Format("The object at index {0} is not of type {1}", i, typeof(T).FullName));
                yield return (T)enumerator.Current;
                i++;
            }
        }

        public int IndexOf(T item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            fValue.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return fValue.Remove(item);
        }

        #endregion
    }
}
