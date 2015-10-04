using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameDataEditor
{
    [Flags]
    public enum BasicFieldType
    {
        Undefined = 0,
        Bool = 1,
        Int = 2,
        Float = 4,
        String = 8,
        Vector2 = 16,
        Vector3 = 32,
        Vector4 = 64,
        Color = 128,
		GameObject = 256,
		Texture2D = 512,
		Material = 1024,
		AudioClip = 2048
    }

    public partial class GDEDataManager {
        private static bool isInitialized = false;

        #region Data Collections
        private static Dictionary<string, object> dataDictionary = null;
        private static Dictionary<string, List<string>> dataKeysBySchema = null;

        public static Dictionary<string, object> DataDictionary
        {
            get
            {
                return dataDictionary;
            }
        }
        #endregion

        #region Properties
        private static string _dataFilePath;
        public static string DataFilePath
        {
            private set { _dataFilePath = value; }
            get { return _dataFilePath; }
        }
        #endregion

        #region Init Methods
        /// <summary>
        /// Loads the specified data file
        /// </summary>
        /// <param name="filePath">Data file path.</param>
		/// <param name="encrypted">Indicates whether data file is encrypted</param>
		/// <returns>True if initialized, false otherwise</returns>
        public static bool Init(string filePath, bool encrypted = false)
        {
            bool result = true;

            if (isInitialized)
                return result;

            try
            {
                DataFilePath = filePath;
               
                TextAsset dataAsset = Resources.Load(DataFilePath) as TextAsset;
				Init(dataAsset, encrypted);

                isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                result = false;
            }
            return result;
        }

		/// <summary>
		/// Loads GDE data from the specified TextAsset
		/// </summary>
		/// <param name="dataAsset">TextAsset that contains GDE data</param>
		public static bool Init(TextAsset dataAsset, bool encrypted = false)
		{
			bool result = true;

			if (isInitialized)
				return result;
			else if (dataAsset == null) {
				Debug.LogError(GDMConstants.ErrorTextAssetNull);
				return false;
			}

			try
			{
				string dataContent = string.Empty;
				if (encrypted)
					dataContent = DecryptGDEData(dataAsset.bytes);
				else
					dataContent = dataAsset.text;
				
				InitFromText(dataContent);

				isInitialized = true;
			}
			catch(Exception ex)
			{
				Debug.LogError(ex);
				result = false;
			}

			return result;
		}

		/// <summary>
		/// Loads GDE data from a string
		/// </summary>
		/// <param name="dataString">String that contains GDE data</param>
		public static bool InitFromText(string dataString)
		{
			bool result = true;

			if (isInitialized)
				return result;

			try
			{
				dataDictionary = Json.Deserialize(dataString) as Dictionary<string, object>;
				
				BuildDataKeysBySchemaList();
				
				isInitialized = true;
			}
			catch(Exception ex)
			{
				Debug.LogError(ex);
				result = false;
			}

			return result;
		}

		public static string DecryptGDEData(byte[] encryptedContent)
		{
			GDECrypto gdeCrypto = null;
			TextAsset gdeCryptoResource = (TextAsset)Resources.Load(GDMConstants.MetaDataFileName, typeof(TextAsset));
			byte[] bytes = Convert.FromBase64String(gdeCryptoResource.text);
			Resources.UnloadAsset(gdeCryptoResource);

			using (var stream = new MemoryStream(bytes))
			{
				BinaryFormatter bin = new BinaryFormatter();
				gdeCrypto = (GDECrypto)bin.Deserialize(stream);
			}
			
			string content = string.Empty;
			if (gdeCrypto != null)
				content = gdeCrypto.Decrypt(encryptedContent);

			return content;
		}

        /// <summary>
        /// Builds the data keys by schema list for lookups by schema.
        /// </summary>
        private static void BuildDataKeysBySchemaList()
        {
            dataKeysBySchema = new Dictionary<string, List<string>>();
            foreach(KeyValuePair<string, object> pair in dataDictionary)
            {
                if (pair.Key.StartsWith(GDMConstants.SchemaPrefix))
                    continue;

                // Get the schema for the current data set
                string schema;
                Dictionary<string, object> currentDataSet = pair.Value as Dictionary<string, object>;
                currentDataSet.TryGetString(GDMConstants.SchemaKey, out schema);

                // Add it to the list of data keys by type
                List<string> dataKeyList;
                if (dataKeysBySchema.TryGetValue(schema, out dataKeyList))                
                {
                    dataKeyList.Add(pair.Key);
                }
                else
                {
                    dataKeyList = new List<string>();
                    dataKeyList.Add(pair.Key);
                    dataKeysBySchema.Add(schema, dataKeyList);
                }
            }
        }
        #endregion

        #region Data Access Methods
        /// <summary>
        /// Get the data associated with the specified key in a Dictionary<string, object>
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        public static bool Get(string key, out Dictionary<string, object> data)
        {
            if (dataDictionary == null)
            {
                data = null;
                return false;
            }

            bool result = true;
            object temp;

            result = dataDictionary.TryGetValue(key, out temp);
            data = temp as Dictionary<string, object>;

            return result;
        }

        /// <summary>
        /// Returns a subset of the data containing only data sets by the given schema
        /// </summary>
        /// <returns><c>true</c>, if the given schema exists <c>false</c> otherwise.</returns>
        /// <param name="type">Schema.</param>
        /// <param name="data">Subset of the Data Set list containing entries with the specified schema.</param>
        public static bool GetAllDataBySchema(string schema, out Dictionary<string, object> data)
        {
            if (dataDictionary == null)
            {
                data = null;
                return false;
            }

            List<string> dataKeys;
            bool result = true;
            data = new Dictionary<string, object>();

            if (dataKeysBySchema.TryGetValue(schema, out dataKeys))
            {
                foreach(string dataKey in dataKeys)
                {
                    Dictionary<string, object> currentDataSet;
                    if (Get(dataKey, out currentDataSet))
                        data.Add(dataKey.Clone().ToString(), currentDataSet.DeepCopy());
                }
            }
            else
               result = false;

            return result;
        }

        /// <summary>
        /// Gets all data keys by schema.
        /// </summary>
        /// <returns><c>true</c>, if the given schema exists <c>false</c> otherwise.</returns>
        /// <param name="schema">Schema.</param>
        /// <param name="dataKeys">Data Key List.</param>
        public static bool GetAllDataKeysBySchema(string schema, out List<string> dataKeys)
        {
            if (dataDictionary == null)
            {
                dataKeys = null;
                return false;
            }

            return dataKeysBySchema.TryGetValue(schema, out dataKeys);
        }

        public static void ResetToDefault(string itemName, string fieldName)
        {
            PlayerPrefs.DeleteKey(itemName + "_" + fieldName);
        }
        #endregion

		#region Get Saved Data Methods (Basic Types)
		public static string GetString(string key, string defaultVal)
		{
			string retVal = defaultVal;
			
			try
			{
				retVal = PlayerPrefs.GetString(key, retVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<string> GetStringList(string key, List<string> defaultVal)
		{
			List<string> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetStringList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
			
		}
		
		public static List<List<string>> GetStringTwoDList(string key, List<List<string>> defaultVal)
		{
			List<List<string>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DStringList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static int GetInt(string key, int defaultVal)
		{
			int retVal = defaultVal;
			
			try
			{
				retVal = PlayerPrefs.GetInt(key, retVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<int> GetIntList(string key, List<int> defaultVal)
		{
			List<int> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetIntList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<int>> GetIntTwoDList(string key, List<List<int>> defaultVal)
		{
			List<List<int>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DIntList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		
		public static float GetFloat(string key, float defaultVal)
		{
			float retVal = defaultVal;
			
			try
			{
				retVal = PlayerPrefs.GetFloat(key, retVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<float> GetFloatList(string key, List<float> defaultVal)
		{
			List<float> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetFloatList(key, retVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<float>> GetFloatTwoDList(string key, List<List<float>> defaultVal)
		{
			List<List<float>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DFloatList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		
		public static bool GetBool(string key, bool defaultVal)
		{
			bool retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetBool(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<bool> GetBoolList(string key, List<bool> defaultVal)
		{
			List<bool> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetBoolList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<bool>> GetBoolTwoDList(string key, List<List<bool>> defaultVal)
		{
			List<List<bool>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DBoolList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		
		public static Color32 GetColor(string key, Color32 defaultVal)
		{
			Color32 retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetColor(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
			
		}
		
		public static List<Color> GetColorList(string key, List<Color> defaultVal)
		{
			List<Color> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetColorList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<Color>> GetColorTwoDList(string key, List<List<Color>> defaultVal)
		{
			List<List<Color>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DColorList(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static Vector2 GetVector2(string key, Vector2 defaultVal)
		{
			Vector2 retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetVector2(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
			
		}
		
		public static List<Vector2> GetVector2List(string key, List<Vector2> defaultVal)
		{
			List<Vector2> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetVector2List(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<Vector2>> GetVector2TwoDList(string key, List<List<Vector2>> defaultVal)
		{
			List<List<Vector2>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DVector2List(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static Vector3 GetVector3(string key, Vector3 defaultVal)
		{
			Vector3 retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetVector3(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
			
		}
		
		public static List<Vector3> GetVector3List(string key, List<Vector3> defaultVal)
		{
			List<Vector3> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetVector3List(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<Vector3>> GetVector3TwoDList(string key, List<List<Vector3>> defaultVal)
		{
			List<List<Vector3>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DVector3List(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static Vector4 GetVector4(string key, Vector4 defaultVal)
		{
			Vector4 retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetVector4(key, defaultVal);  
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
			
		}
		
		public static List<Vector4> GetVector4List(string key, List<Vector4> defaultVal)
		{
			List<Vector4> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.GetVector4List(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<List<Vector4>> GetVector4TwoDList(string key, List<List<Vector4>> defaultVal)
		{
			List<List<Vector4>> retVal = defaultVal;
			
			try
			{
				retVal = GDEPPX.Get2DVector4List(key, defaultVal);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}

		public static GameObject GetGameObject(string key, GameObject defaultVal)
		{
			return defaultVal;
		}

		public static List<GameObject> GetGameObjectList(string key, List<GameObject> defaultVal)
		{
			return defaultVal;
		}

		public static List<List<GameObject>> GetGameObjectTwoDList(string key, List<List<GameObject>> defaultVal)
		{
			return defaultVal;
		}

		public static Texture2D GetTexture2D(string key, Texture2D defaultVal)
		{
			return defaultVal;
		}
		
		public static List<Texture2D> GetTexture2DList(string key, List<Texture2D> defaultVal)
		{
			return defaultVal;
		}
		
		public static List<List<Texture2D>> GetTexture2DTwoDList(string key, List<List<Texture2D>> defaultVal)
		{
			return defaultVal;
		}

		public static Material GetMaterial(string key, Material defaultVal)
		{
			return defaultVal;
		}
		
		public static List<Material> GetMaterialList(string key, List<Material> defaultVal)
		{
			return defaultVal;
		}
		
		public static List<List<Material>> GetMaterialTwoDList(string key, List<List<Material>> defaultVal)
		{
			return defaultVal;
		}

		public static AudioClip GetAudioClip(string key, AudioClip defaultVal)
		{
			return defaultVal;
		}
		
		public static List<AudioClip> GetAudioClipList(string key, List<AudioClip> defaultVal)
		{
			return defaultVal;
		}
		
		public static List<List<AudioClip>> GetAudioClipTwoDList(string key, List<List<AudioClip>> defaultVal)
		{
			return defaultVal;
		}
		#endregion

		#region Get Saved Data Methods (Custom Types)
		public static T GetCustom<T>(string key, T defaultVal) where T : IGDEData, new()
		{
			T retVal = defaultVal;
			
			try
			{
				string defaultKey = (defaultVal != null)?defaultVal.Key:string.Empty;
				string customKey = GDEDataManager.GetString(key, defaultKey);
				if (customKey != defaultKey)
				{
					// First load defaults for this custom item
					GDEDataManager.DataDictionary.TryGetCustom(customKey, out retVal);

					// Then load any overrides from playerprefs
					retVal.LoadFromSavedData(customKey);
				}
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		
		public static List<T> GetCustomList<T>(string key, List<T> defaultVal) where T : IGDEData, new()
		{
			List<T> retVal = defaultVal;
			
			try
			{
				if (PlayerPrefs.HasKey(key))
				{
					retVal = new List<T>();
					
					List<string> customDataKeys = GDEDataManager.GetStringList(key, null);
					
					if (customDataKeys != null)
					{
						foreach(string customDataKey in customDataKeys)
						{
							T temp;
							if (GDEDataManager.DataDictionary.TryGetCustom(customDataKey, out temp))
								retVal.Add(temp);
						}
					}
				}
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}

		public static List<List<T>> GetCustomTwoDList<T>(string key, List<List<T>> defaultVal) where T : IGDEData, new()
		{
			List<List<T>> retVal = defaultVal;
			
			try
			{
				if (PlayerPrefs.HasKey(key))
				{
					retVal = new List<List<T>>();
					
					List<List<string>> customDataKeys = GDEDataManager.GetStringTwoDList(key, null);
					
					if (customDataKeys != null)
					{
						foreach(var subListKeys in customDataKeys)
						{
							List<T> subList = new List<T>();
							foreach(var customDataKey in subListKeys)
							{
								T temp;
								if (GDEDataManager.DataDictionary.TryGetCustom(customDataKey, out temp))
									subList.Add(temp);
							}
							retVal.Add(subList);
						}
					}
				}
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return retVal;
		}
		#endregion
		
		#region Save Data Methods (Basic Types)
		public static void SetString(string key, string val)
		{
			try
			{
				PlayerPrefs.SetString(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetStringList(string key, List<string> val)
		{
			try
			{
				GDEPPX.SetStringList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetStringTwoDList(string key, List<List<string>> val)
		{
			try
			{
				GDEPPX.Set2DStringList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetInt(string key, int val)
		{
			try
			{
				PlayerPrefs.SetInt(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetIntList(string key, List<int> val)
		{
			try
			{
				GDEPPX.SetIntList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetIntTwoDList(string key, List<List<int>> val)
		{
			try
			{
				GDEPPX.Set2DIntList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetFloat(string key, float val)
		{
			try
			{
				PlayerPrefs.SetFloat(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetFloatList(string key, List<float> val)
		{
			try
			{
				GDEPPX.SetFloatList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetFloatTwoDList(string key, List<List<float>> val)
		{
			try
			{
				GDEPPX.Set2DFloatList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetBool(string key, bool val)
		{
			try
			{
				GDEPPX.SetBool(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetBoolList(string key, List<bool> val)
		{
			try
			{
				GDEPPX.SetBoolList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetBoolTwoDList(string key, List<List<bool>> val)
		{
			try
			{
				GDEPPX.Set2DBoolList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetColor(string key, Color32 val)
		{
			try
			{
				GDEPPX.SetColor(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetColorList(string key, List<Color> val)
		{
			try
			{
				GDEPPX.SetColorList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetColorTwoDList(string key, List<List<Color>> val)
		{
			try
			{
				GDEPPX.Set2DColorList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector2(string key, Vector2 val)
		{
			try
			{
				GDEPPX.SetVector2(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector2List(string key, List<Vector2> val)
		{
			try
			{
				GDEPPX.SetVector2List(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector2TwoDList(string key, List<List<Vector2>> val)
		{
			try
			{
				GDEPPX.Set2DVector2List(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector3(string key, Vector3 val)
		{
			try
			{
				GDEPPX.SetVector3(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector3List(string key, List<Vector3> val)
		{
			try
			{
				GDEPPX.SetVector3List(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector3TwoDList(string key, List<List<Vector3>> val)
		{
			try
			{
				GDEPPX.Set2DVector3List(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector4(string key, Vector4 val)
		{
			try
			{
				GDEPPX.SetVector4(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector4List(string key, List<Vector4> val)
		{
			try
			{
				GDEPPX.SetVector4List(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetVector4TwoDList(string key, List<List<Vector4>> val)
		{
			try
			{
				GDEPPX.Set2DVector4List(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void SetGameObject(string key, GameObject val)
		{
			try
			{
				GDEPPX.SetGameObject(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetGameObjectList(string key, List<GameObject> val)
		{
			try
			{
				GDEPPX.SetGameObjectList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetGameObjectTwoDList(string key, List<List<GameObject>> val)
		{
			try
			{
				GDEPPX.Set2DGameObjectList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void SetTexture2D(string key, Texture2D val)
		{
			try
			{
				GDEPPX.SetTexture2D(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetTexture2DList(string key, List<Texture2D> val)
		{
			try
			{
				GDEPPX.SetTexture2DList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetTexture2DTwoDList(string key, List<List<Texture2D>> val)
		{
			try
			{
				GDEPPX.Set2DTexture2DList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void SetMaterial(string key, Material val)
		{
			try
			{
				GDEPPX.SetMaterial(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetMaterialList(string key, List<Material> val)
		{
			try
			{
				GDEPPX.SetMaterialList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetMaterialTwoDList(string key, List<List<Material>> val)
		{
			try
			{
				GDEPPX.Set2DMaterialList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void SetAudioClip(string key, AudioClip val)
		{
			try
			{
				GDEPPX.SetAudioClip(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetAudioClipList(string key, List<AudioClip> val)
		{
			try
			{
				GDEPPX.SetAudioClipList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		public static void SetAudioClipTwoDList(string key, List<List<AudioClip>> val)
		{
			try
			{
				GDEPPX.Set2DAudioClipList(key, val);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		#endregion

		#region Save Data Methods (Custom Types)
		public static void SetCustom<T>(string key, T val) where T : IGDEData
		{
			GDEDataManager.SetString(key, val.Key);
		}
		
		public static void SetCustomList<T>(string key, List<T> val) where T : IGDEData
		{
			List<string> customKeys = new List<string>();
			val.ForEach(x => customKeys.Add(x.Key));
			GDEDataManager.SetStringList(key, customKeys);
		}
		
		public static void SetCustomTwoDList<T>(string key, List<List<T>> val) where T : IGDEData
		{
			List<List<string>> customKeys = new List<List<string>>();
			foreach(List<T> subList in val)
			{
				List<string> subListKeys = new List<string>();
				subList.ForEach(x => subListKeys.Add(x.Key));
				customKeys.Add(subListKeys);
			}
			GDEDataManager.SetStringTwoDList(key, customKeys);
		}
		#endregion
    }
}
