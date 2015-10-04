using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameDataEditor
{
    public abstract partial class IGDEData
    {
		protected string _key;
		public string Key
		{
			get { return _key; }
			private set { _key = value; }
		}

        public abstract void LoadFromDict(string key, Dictionary<string, object> dict);
		public abstract void LoadFromSavedData(string key);
    }
}

