using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
	[Serializable]
	public abstract class SerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, ISerializationCallbackReceiver
	{
		public SerializableDictionary()
		{

		}

		public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
		[SerializeField]
		private List<TKey> _Keys;
		[SerializeField]
		private List<TVal> _Values;

		protected abstract bool KeysAreEqual(TKey a, TKey b);
		protected abstract bool ValuesAreEqual(TVal a, TVal b);

		public void OnBeforeSerialize()
		{
			_Keys = new List<TKey>();
			_Values = new List<TVal>();
			foreach (KeyValuePair<TKey, TVal> pair in this)
			{
				try
				{
					_Keys.Add(pair.Key);
					_Values.Add(pair.Value);
				}
				catch { }
			}
		}

		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			// There are some situations where Unity will not get the serialized data correctly, returning null.
			// In this case we don't want to change anything, otherwise we'll lose the data entirely.
			if (_Keys == null || _Values == null)
				return;

			if (_Keys.Count != _Values.Count)
				throw new System.Exception(string.Format("Key count is different to value count after deserialising dictionary."));

			this.Clear();

			for (int i = 0; i < _Keys.Count; i++)
			{
				if (_Keys[i] != null)
				{
					try
					{
						this.Add(_Keys[i], _Values[i]);
					}
					catch { }
				}
			}

			_Keys = null;
			_Values = null;
		}
	}
}
