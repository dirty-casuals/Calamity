using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace GameDataEditor
{
    public class GDEItemManager
    {
        #region Item Dictionary
		public static bool ShouldReloadItems;
		public static bool ShouldReloadSchemas;

        static string _dataFileMD5;
        public static bool ItemsNeedSave;
        public static string DataFilePath
        {
			get { return GDESettings.Instance.DataFilePath; }
        }

		public static string EncryptedFilePath
		{
			get
			{
				return Path.GetDirectoryName(GDESettings.FullRootDir + Path.DirectorySeparatorChar + GDECodeGenConstants.CryptoFilePath) + Path.DirectorySeparatorChar +
					Path.GetFileNameWithoutExtension(GDEItemManager.DataFilePath)+
					GDECodeGenConstants.EncryptedDataFileSuffix;
			}
		}

        static Dictionary<string, Dictionary<string, object>> _allItems;
        public static Dictionary<string, Dictionary<string, object>> AllItems
        {
            set
            {
                _allItems = value;
            }

            get
            {
                if (_allItems == null)
                    _allItems = new Dictionary<string, Dictionary<string, object>>();
                return _allItems;
            }
        }
        #endregion

        #region Schema Dictionary
        public static bool SchemasNeedSave;
        static Dictionary<string, Dictionary<string, object>> _schema;
        public static Dictionary<string, Dictionary<string, object>> AllSchemas
        {
            set
            {
                _schema = value;
            }

            get
            {
                if (_schema == null)
                    _schema = new Dictionary<string, Dictionary<string, object>>();
                return _schema;
            }
        }
        static string[] _filterSchemaKeyArray;
        public static string[] FilterSchemaKeyArray
        {
            set
            {
                _filterSchemaKeyArray = value;
            }

            get
            {
                if (_filterSchemaKeyArray == null)
                    _filterSchemaKeyArray = BuildSchemaFilterKeyArray();

                return _filterSchemaKeyArray;
            }
        }
        static string[] _schemaKeyArray;
        public static string[] SchemaKeyArray
        {
            set
            {
                _schemaKeyArray = value;
            }

            get
            {
                if (_schemaKeyArray == null)
                    _schemaKeyArray = BuildSchemaKeyArray();

                return _schemaKeyArray;
            }
        }

        #endregion

        #region Sorting and Lookup
        // Basic Field Type string[]
        static string[] _basicFieldTypeStringArray = null;
        public static string[] BasicFieldTypeStringArray
        {
            set { _basicFieldTypeStringArray = value; }
            get
            {
                if (_basicFieldTypeStringArray == null)
                    _basicFieldTypeStringArray = BuildBasicTypeStringArray();

                return _basicFieldTypeStringArray;
            }
        }

        //Basic Field Type List
        static List<BasicFieldType> _basicFieldTypes = null;
        public static List<BasicFieldType> BasicFieldTypes
        {
            set { _basicFieldTypes = value; }
            get
            {
                if (_basicFieldTypes == null)
                    _basicFieldTypes = BuildBasicTypeList();

                return _basicFieldTypes;
            }
        }

        // Key: field name, List: contains the schema keys that contain that field name
        static Dictionary<string, List<string>> _listByFieldName;
        static Dictionary<string, List<string>> ListByFieldName
        {
            set { _listByFieldName = value; }
            get
            {
                if (_listByFieldName == null)
                    _listByFieldName = new Dictionary<string, List<string>>();
                return _listByFieldName;
            }
        }

        static Dictionary<string, List<string>> _itemListBySchema;
        public static Dictionary<string, List<string>> ItemListBySchema
        {
            private set { _itemListBySchema = value; }
            get
            {
                if (_itemListBySchema == null)
                    _itemListBySchema = new Dictionary<string, List<string>>();
                return _itemListBySchema;
            }
        }

        static string[] BuildSchemaFilterKeyArray()
        {
            List<string> temp = GDEItemManager.AllSchemas.Keys.ToList();
            temp.Add("_All");
            temp.Sort();
            return temp.ToArray();
        }

        static string[] BuildSchemaKeyArray()
        {
            return GDEItemManager.AllSchemas.Keys.ToArray();
        }

        static List<BasicFieldType> BuildBasicTypeList()
        {
            List<BasicFieldType> basicTypes = Enum.GetValues(typeof(BasicFieldType)).Cast<BasicFieldType>().ToList();
            basicTypes.Remove(BasicFieldType.Undefined);
            return basicTypes;
        }

        static string[] BuildBasicTypeStringArray()
        {
            string[] basicTypeArray = new string[BasicFieldTypes.Count];
            for(int index=0; index<basicTypeArray.Length;  index++)
                basicTypeArray[index] = BasicFieldTypes[index].ToString();

            return basicTypeArray;
        }

        public static string GetSchemaForItem(string itemKey)
        {
            string schema = "";
            Dictionary<string, object> itemData;
            if (AllItems.TryGetValue(itemKey, out itemData))
            {
                object temp;
                if (itemData.TryGetValue(GDMConstants.SchemaKey, out temp))
                    schema = temp as string;
            }

            return schema;
        }

        public static List<string> GetItemsOfSchemaType(string schemaType)
        {
            List<string> itemList;
            ItemListBySchema.TryGetValue(schemaType, out itemList);

            if (itemList == null)
                itemList = new List<string>();

            return itemList;
        }

		public static List<string> ItemFieldKeysOfType(string itemKey, string fieldType, int listDimension)
        {
            return FieldKeysOfType(itemKey, fieldType, AllItems, listDimension);
        }

		public static List<string> ItemCustomFieldKeys(string itemKey, int listDimension)
        {
            return CustomFieldKeys(itemKey, AllItems, listDimension);
        }

		public static List<string> SchemaFieldKeysOfType(string schemaKey, string fieldType, int listDimension)
        {
            return FieldKeysOfType(schemaKey, fieldType, AllSchemas, listDimension);
        }

		public static List<string> SchemaCustomFieldKeys(string schemaKey, int listDimension)
        {
            return CustomFieldKeys(schemaKey, AllSchemas, listDimension);
        }

		public static List<string> SchemaFieldKeys(string schemaKey, Dictionary<string, object> schemaData)
		{
			List<string> allFieldKeys = new List<string>();

			foreach(KeyValuePair<string, object> field in schemaData)
			{
				if (field.Key.StartsWith(GDMConstants.IsListPrefix) ||
				    field.Key.StartsWith(GDMConstants.TypePrefix) ||
				    field.Key.StartsWith(GDMConstants.SchemaKey))
					continue;

				allFieldKeys.Add(field.Key);
			}

			return allFieldKeys;
		}

		// listDimension = -1, returns all keys matching type
		// listDimension = 0, returns single keys matching type
		// listDimension = 1, returns List<> keys matching type
		// listDimension = 2, returns 2D List keys matching type
        static List<string> FieldKeysOfType(string key, string fieldType, Dictionary<string, Dictionary<string, object>> dict, int listDimension)
        {
            List<string> fieldKeys = new List<string>();
            Dictionary<string, object> data;
            string fieldName;
            string isListKey;
			int currentListDimension;

            if (dict.TryGetValue(key, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (!field.Key.StartsWith(GDMConstants.TypePrefix))
                        continue;

                    fieldName = field.Key.Replace(GDMConstants.TypePrefix, string.Empty);
                    isListKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName);

					data.TryGetInt(isListKey, out currentListDimension);

                    if (field.Value.ToString().ToLower().Equals(fieldType.ToLower()) && (listDimension == currentListDimension || listDimension == -1))
                        fieldKeys.Add(fieldName);
                }
            }

            return fieldKeys;
        }

		// listDimension = -1, returns all keys matching type
		// listDimension = 0, returns single keys matching type
		// listDimension = 1, returns List<> keys matching type
		// listDimension = 2, returns 2D List keys matching type
        static List<string> CustomFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict, int listDimension)
        {
            List<string> fieldKeys = new List<string>();
            Dictionary<string, object> data;
            string fieldName;
            string isListKey;
			int currentListDimension;

            if (dict.TryGetValue(key, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (!field.Key.StartsWith(GDMConstants.TypePrefix))
                        continue;

                    fieldName = field.Key.Replace(GDMConstants.TypePrefix, string.Empty);
                    isListKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName);

					data.TryGetInt(isListKey, out currentListDimension);

                    if (!Enum.IsDefined(typeof(BasicFieldType), field.Value) && (listDimension == currentListDimension || listDimension == -1))
                        fieldKeys.Add(fieldName);
                }
            }

            return fieldKeys;
        }

        // Returns a list of item keys that are of the List<>  or List<List<>> type
        public static List<string> ItemListFieldKeys(string itemKey)
        {
            List<string> fieldKeys = new List<string>();
            Dictionary<string, object> data;

            if (AllItems.TryGetValue(itemKey, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (field.Key.StartsWith(GDMConstants.IsListPrefix))
                        fieldKeys.Add(field.Key.Replace(GDMConstants.IsListPrefix, ""));
                }
            }

            return fieldKeys;
        }

        static List<string> GetAllFieldKeys(string key, Dictionary<string, Dictionary<string, object>> dict)
        {
            List<string> allFields = new List<string>();
            Dictionary<string, object> data;

            if (dict.TryGetValue(key, out data))
            {
                foreach(KeyValuePair<string, object> field in data)
                {
                    if (field.Key.StartsWith(GDMConstants.IsListPrefix) ||
                        field.Key.StartsWith(GDMConstants.TypePrefix) ||
                        field.Key.StartsWith(GDMConstants.SchemaKey))
                        continue;

                    allFields.Add(field.Key);
                }
            }

            return allFields;
        }

        static void BuildSortingAndLookupListFor(string schemaKey, Dictionary<string, object> schemaData, bool rebuildArrays = true)
        {
            // Parse and add to list by field name
            foreach(KeyValuePair<string, object> field in schemaData)
            {
                // Skip over any metadata
                if (field.Key.StartsWith(GDMConstants.TypePrefix) ||
                    field.Key.StartsWith(GDMConstants.IsListPrefix))
                    continue;

                AddFieldToListByFieldName(field.Key, schemaKey);
            }

            // Create empty list for the Item by Schema list
            ItemListBySchema.Add(schemaKey, new List<string>());

            if (rebuildArrays)
            {
                SchemaKeyArray = BuildSchemaKeyArray();
                FilterSchemaKeyArray = BuildSchemaFilterKeyArray();
            }
        }

        static void AddFieldToListByFieldName(string fieldKey, string schemaKey)
        {
            List<string> schemaKeyList;
            if (ListByFieldName.TryGetValue(fieldKey, out schemaKeyList))
            {
                schemaKeyList.Add(schemaKey);
            }
            else
            {
                schemaKeyList = new List<string>();
                schemaKeyList.Add(schemaKey);
                ListByFieldName.Add(fieldKey, schemaKeyList);
            }
        }

        static void AddItemToListBySchema(string itemKey, string schemaKey)
        {
            List<string> itemList;
            if (ItemListBySchema.TryGetValue(schemaKey, out itemList))
            {
                if (!itemList.Contains(itemKey))
                    itemList.Add(itemKey);
            }
            else
            {
                itemList = new List<string>();
                itemList.Add(itemKey);
                ItemListBySchema.Add(schemaKey, itemList);
            }
        }
        #endregion

        #region Save/Load Methods
        public static void Load(bool forceLoad = false)
        {
			bool result = true;

            if (!CreateFileIfMissing(DataFilePath))
                return;

            bool fileChangedOnDisk = FileChangedOnDisk(DataFilePath, _dataFileMD5);

            if (forceLoad || SchemasNeedSave || fileChangedOnDisk)
                result = LoadSchemas();

            if ((forceLoad || ItemsNeedSave || fileChangedOnDisk) && result)
                LoadItems();
        }

        public static void Save()
        {
            try
            {
				Dictionary<string, object> allData = GetJsonReadyDataCopy();

				string rawJson = Json.Serialize(allData);
                string prettyJson = JsonHelper.FormatJson(rawJson);

                File.WriteAllText(DataFilePath, prettyJson);

                ItemsNeedSave = false;
                SchemasNeedSave = false;

				AssetDatabase.Refresh();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public static void ClearAll()
        {
            AllItems.Clear();

            AllSchemas.Clear();
            ListByFieldName.Clear();
            ItemListBySchema.Clear();
            FilterSchemaKeyArray = null;
            SchemaKeyArray = null;

			ShouldReloadItems = true;
			ShouldReloadSchemas = true;

            ItemsNeedSave = true;
            SchemasNeedSave = true;
        }

        static bool LoadItems()
        {
			bool result = true;
            try
            {
                string json = File.ReadAllText(DataFilePath);
                _dataFileMD5 = json.Md5Sum();

				Dictionary<string, object> data;
				try
				{
					data = Json.Deserialize(json) as Dictionary<string, object>;
				}
				catch
				{
					Debug.LogError(string.Format(GDEConstants.ErrorParsingJson, DataFilePath));
					return false;
				}

                AllItems.Clear();

                string error;
                foreach(var pair in data)
                {
                    if (pair.Key.StartsWith(GDMConstants.SchemaPrefix))
                        continue;

					// Post process for any Prefabs
					LoadSpecialTypes(pair.Value as Dictionary<string, object>);

                    AddItem(pair.Key, pair.Value as Dictionary<string, object>, out error);
                }

                ItemsNeedSave = false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
				result = false;
            }

			return result;
        }

        static bool LoadSchemas()
        {
			bool result = true;

            try
            {
                string json = File.ReadAllText(DataFilePath);
                _dataFileMD5 = json.Md5Sum();

				Dictionary<string, object> data;
				try
				{
	                data = Json.Deserialize(json) as Dictionary<string, object>;
				}
				catch
				{
					Debug.LogError(string.Format(GDEConstants.ErrorParsingJson, DataFilePath));
					return false;
				}

                // Clear all schema related lists
                AllSchemas.Clear();
                ListByFieldName.Clear();
                ItemListBySchema.Clear();
                FilterSchemaKeyArray = null;
                SchemaKeyArray = null;

                string error;
                string schemaName;
				var keys = new List<string>(data.Keys);
                foreach(var key in keys)
                {
                    if (!key.StartsWith(GDMConstants.SchemaPrefix))
                        continue;

                    Dictionary<string, object> schemaData = data[key] as Dictionary<string, object>;
                    schemaName = key.Replace(GDMConstants.SchemaPrefix, "");

					// Post process for any Prefabs
					LoadSpecialTypes(schemaData);

                    AddSchema(schemaName, schemaData, out error, false);
                }

                SchemaKeyArray = BuildSchemaKeyArray();
                FilterSchemaKeyArray = BuildSchemaFilterKeyArray();

                SchemasNeedSave = false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
				result = false;
            }

			return result;
        }

        public static bool CreateFileIfMissing(string path)
        {
            bool result = true;
			string dir = Path.GetDirectoryName(path);

            try
            {
                if (!Directory.Exists(dir) || !File.Exists(path))
                {
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

                    StreamWriter writer = File.CreateText(path);
                    writer.WriteLine("{}");
                    writer.Close();

					AssetDatabase.Refresh();
                }
            }
            catch(DirectoryNotFoundException)
            {
                EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, string.Format(GDEConstants.DirectoryNotFound, path), GDEConstants.OkLbl);
                result = false;
            }

            return result;
        }

        static bool SchemaExistsForItem(string itemKey, Dictionary<string, object> itemData)
        {
            bool result = false;
            object schemaType;

            if (itemData.TryGetValue(GDMConstants.SchemaKey, out schemaType))
            {
                string schemaKey = schemaType as string;
                if (AllSchemas.ContainsKey(schemaKey))
                    result = true;
            }

            return result;

        }

        public static bool FileChangedOnDisk(string filePath, string cachedMD5)
        {
            bool hasChanged = true;

            try
            {
                string currentMD5 = File.ReadAllText(filePath).Md5Sum();
                hasChanged = cachedMD5 != currentMD5;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }

            return hasChanged;
        }
        #endregion

        #region Add/Remove Methods
        public static bool AddItem(string key, Dictionary<string, object> data, out string error)
        {
            bool result = true;
            error = "";

            if (IsItemNameValid(key, out error) && SchemaExistsForItem(key, data))
                result = AllItems.TryAddValue(key, data);
            else
                result = false;

            if (result)
            {
                AddItemToListBySchema(key, GetSchemaForItem(key));
                ItemsNeedSave = true;
				ShouldReloadItems = true;
            }

            return result;
        }

		public static bool CloneItem(string key, out string newKey, out string error)
		{
			newKey = key + "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
			var clonedData = AllItems[key].DeepCopy();
			return AddItem(newKey, clonedData, out error);
		}

        public static void RemoveItem(string key)
        {
            string schemaKey = GetSchemaForItem(key);
            List<string> itemList;

            // Remove from the Item list by schema
            if(ItemListBySchema.TryGetValue(schemaKey, out itemList))
            {
                itemList.Remove(key);
            }

            AllItems.Remove(key);

            ItemsNeedSave = true;
			ShouldReloadItems = true;
        }

        public static bool AddSchema(string name, Dictionary<string, object> data, out string error, bool rebuildArrays = true)
        {
            bool result = true;

            if (IsSchemaNameValid(name, out error))
                result = AllSchemas.TryAddValue(name, data);
            else
                result = false;

            if (result)
            {
                BuildSortingAndLookupListFor(name, data, rebuildArrays);
                SchemasNeedSave = true;
				ShouldReloadSchemas = true;
            }

            return result;
        }

		public static bool CloneSchema(string key, out string newKey, out string error, bool rebuildArrays = true)
		{
			newKey = key + "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
			var clonedData = AllSchemas[key].DeepCopy();
			return AddSchema(newKey, clonedData, out error, rebuildArrays);
		}

        public static void RemoveSchema(string key, bool deleteItems = true)
        {
            if (deleteItems)
            {
                // Delete the items with this schema
                List<string> itemList;
                if (ItemListBySchema.TryGetValue(key, out itemList))
                {
                    List<string> itemListCopy = new List<string>(itemList);
                    foreach(string itemKey in itemListCopy)
                        RemoveItem(itemKey);
                }
            }
            ItemListBySchema.Remove(key);

            // Remove all the fields so the lookup lists get updated
            List<string> allFields = GetAllFieldKeys(key, AllSchemas);
            foreach(string field in allFields)
                RemoveFieldFromSchema(key, field, deleteItems);

            AllSchemas.Remove(key);
            SchemasNeedSave = true;
			ShouldReloadSchemas = true;

            SchemaKeyArray = BuildSchemaKeyArray();
            FilterSchemaKeyArray = BuildSchemaFilterKeyArray();
        }
        #endregion

        #region Add/Remove Schema Field Methods
        public static bool AddBasicFieldToSchema(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, out string error, bool isList = false, bool is2DList = false, object defaultValue = null)
        {
            if (schemaData == null)
                schemaData = AllSchemas[schemaKey];

            string typeKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName);
            error = "";
            bool result = IsFieldNameValid(schemaKey, newFieldName, out error);

            if (result)
            {
                if (isList)
                {
					if (IsUnityType(type))
					{
						if (type.Equals(BasicFieldType.GameObject))
							defaultValue = new List<GameObject>();
						else if (type.Equals(BasicFieldType.Texture2D))
							defaultValue = new List<Texture2D>();
						else if (type.Equals(BasicFieldType.Material))
							defaultValue = new List<Material>();
						else if (type.Equals(BasicFieldType.AudioClip))
							defaultValue = new List<AudioClip>();
					}
					else
						defaultValue = new List<object>();

                    schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), 1);
                }
				else if (is2DList)
				{
					if (IsUnityType(type))
					{
						if (type.Equals(BasicFieldType.GameObject))
							defaultValue = new List<List<GameObject>>();
						else if (type.Equals(BasicFieldType.Texture2D))
							defaultValue = new List<List<Texture2D>>();
						else if (type.Equals(BasicFieldType.Material))
							defaultValue = new List<List<Material>>();
						else if (type.Equals(BasicFieldType.AudioClip))
							defaultValue = new List<List<AudioClip>>();
					}
					else
						defaultValue = new List<object>();

					schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), 2);
				}

				schemaData.Add(typeKey, type);
				result = schemaData.TryAddValue(newFieldName, defaultValue);

                AddFieldToListByFieldName(newFieldName, schemaKey);
                AddBasicFieldToItems(type, schemaKey, newFieldName, isList, is2DList, defaultValue);
            }

            return result;
        }

        public static bool AddCustomFieldToSchema(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList, bool is2DList, out string error)
        {
            bool result = IsFieldNameValid(schemaKey, newFieldName, out error);

            if (result)
            {
                if (isList)
                {
                    result = schemaData.TryAddValue(newFieldName, new List<object>());
                    schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), customType);
                    schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), 1);
                }
				else if (is2DList)
				{
					result = schemaData.TryAddValue(newFieldName, new List<object>());
					schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), customType);
					schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), 2);
				}
                else
                {
                    result = schemaData.TryAddValue(newFieldName, "null");
                    schemaData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), customType);
                }

                AddFieldToListByFieldName(newFieldName, schemaKey);
                AddCustomFieldToItems(customType, schemaKey, newFieldName, isList, is2DList);
            }

            return result;
        }

        public static void RemoveFieldFromSchema(string schemaKey, string deletedFieldKey, bool deleteFromItem = true)
        {
            Dictionary<string, object> schemaData;
            if (AllSchemas.TryGetValue(schemaKey, out schemaData))
                RemoveFieldFromSchema(schemaKey, schemaData, deletedFieldKey, deleteFromItem);
        }

        public static void RemoveFieldFromSchema(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey, bool deleteFromItem = true)
        {
            schemaData.Remove(deletedFieldKey);
            schemaData.Remove(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, deletedFieldKey));
            schemaData.Remove(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, deletedFieldKey));

            // Remove the schema key from the listbyfieldname List
            List<string> schemaKeyList;
            if(ListByFieldName.TryGetValue(deletedFieldKey, out schemaKeyList))
            {
                schemaKeyList.Remove(schemaKey);
                if (schemaKeyList.Count == 0)
                    ListByFieldName.Remove(deletedFieldKey);
            }

            if (deleteFromItem)
                RemoveFieldFromItems(schemaKey, deletedFieldKey);
        }
        #endregion

        #region Add/Remove Item Field Methods
        static void AddBasicFieldToItems(BasicFieldType type, string schemaKey, string newFieldName, bool isList, bool is2DList, object defaultValue)
        {
            List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
            Dictionary<string, object> itemData;
			int listDimension = 0;

            foreach(string itemKey in itemKeys)
            {
                if (AllItems.TryGetValue(itemKey, out itemData))
                {
                    if (isList)
						listDimension = 1;
					else if(is2DList)
						listDimension = 2;
                    else
						listDimension = 0;

					itemData.Add(newFieldName, defaultValue.DeepCopyCollection());
					itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), type);
					itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), listDimension);
                    ItemsNeedSave = true;
                }
            }
        }

        static void AddCustomFieldToItems(string customType, string schemaKey, string newFieldName, bool isList, bool is2DList)
        {
            List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
            Dictionary<string, object> itemData;

            foreach(string itemKey in itemKeys)
            {
                if (AllItems.TryGetValue(itemKey, out itemData))
                {
                    if (isList)
                    {
                        itemData.Add(newFieldName, new List<object>());
                        itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), customType);
                        itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), 1);
                    }
					else if(is2DList)
					{
						itemData.Add(newFieldName, new List<object>());
						itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), customType);
						itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldName), 2);
					}
                    else
                    {
                        itemData.Add(newFieldName, "null");
                        itemData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldName), customType);
                    }

                    ItemsNeedSave = true;
                }
            }
        }

        static void RemoveFieldFromItems(string schemaKey, string deleteFieldName)
        {
            List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
            Dictionary<string, object> itemData;

            foreach(string itemKey in itemKeys)
            {
                if (AllItems.TryGetValue(itemKey, out itemData))
                {
                    itemData.Remove(deleteFieldName);
                    itemData.Remove(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, deleteFieldName));
                    itemData.Remove(string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, deleteFieldName));

                    ItemsNeedSave = true;
                }
            }
        }
        #endregion

        #region Rename Methods
        public static bool RenameSchema(string oldSchemaKey, string newSchemaKey, out string error)
        {
            bool result = true;
            if (IsSchemaNameValid(newSchemaKey, out error))
            {
                Dictionary<string, object> schemaData;
                if (AllSchemas.TryGetValue(oldSchemaKey, out schemaData))
                {
                    List<string> itemsWithSchema = GetItemsOfSchemaType(oldSchemaKey);
                    Dictionary<string, object> schemaDataCopy = schemaData.DeepCopy();

                    // First remove the schema from the dictionary
                    RemoveSchema(oldSchemaKey, false);

                    // Then add the schema data under the new schema key
                    if(AddSchema(newSchemaKey, schemaDataCopy, out error))
                    {
                        List<string> itemBySchemaList;
                        ItemListBySchema.TryGetValue(newSchemaKey, out itemBySchemaList);

                        // Update the schema key on any existing items
                        foreach(string itemKey in itemsWithSchema)
                        {
                            Dictionary<string, object> itemData;
                            if (AllItems.TryGetValue(itemKey, out itemData))
                                itemData.TryAddOrUpdateValue(GDMConstants.SchemaKey, newSchemaKey);
                            itemBySchemaList.Add(itemKey);
                        }

                        // Update any custom fields in schemas that had the old schema name
                        foreach(string curSchemaKey in AllSchemas.Keys)
                        {
                            List<string> fieldsOfSchemaType = SchemaFieldKeysOfType(curSchemaKey, oldSchemaKey, -1);

                            if (fieldsOfSchemaType.Count > 0)
                            {
                                Dictionary<string, object> curSchemaData;
                                AllSchemas.TryGetValue(curSchemaKey, out curSchemaData);

                                if (curSchemaData == null)
                                    continue;

                                foreach(string schemaFieldKey in fieldsOfSchemaType)
                                    curSchemaData.TryAddOrUpdateValue(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, schemaFieldKey), newSchemaKey);
                            }
                        }

                        // Lastly, update any custom fields that had the old schema name
                        foreach(string curItemKey in AllItems.Keys)
                        {
                            List<string> fieldsOfSchemaType = ItemFieldKeysOfType(curItemKey, oldSchemaKey, -1);

                            if (fieldsOfSchemaType.Count > 0)
                            {
                                Dictionary<string, object> curItemData;
                                AllItems.TryGetValue(curItemKey, out curItemData);

                                if (curItemData == null)
                                    continue;

                                foreach(string itemFieldKey in fieldsOfSchemaType)
                                    curItemData.TryAddOrUpdateValue(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, itemFieldKey), newSchemaKey);
                            }
                        }
                    }
                    else
                    {
                        // Add the schema back under the old key if this step failed
                        AddSchema(oldSchemaKey, schemaDataCopy, out error);
                        result = false;
                    }
                }
                else
                {
                    error = GDEConstants.FailedToReadScehmaData + " " + oldSchemaKey;
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            SchemasNeedSave |= result;
            ItemsNeedSave |= result;

            return result;
        }

        public static bool RenameItem(string oldItemKey, string newItemKey, Dictionary<string, object> data, out string error)
        {
            bool result = true;
            if (IsItemNameValid(newItemKey, out error))
            {
                Dictionary<string, object> itemData;
                if (AllItems.TryGetValue(oldItemKey, out itemData))
                {
                    Dictionary<string, object> itemDataCopy = itemData.DeepCopy();

                    // First remove the item from the dictionary
                    RemoveItem(oldItemKey);

                    // Then add the item data under the new item key
                    if (!AddItem(newItemKey, itemDataCopy, out error))
                    {
                        // Add the item back under the old key if this step failed
                        AddItem(oldItemKey, itemDataCopy, out error);
                        result = false;
                    }

                    // Update any items that have a reference to this item
                    string itemSchemaType;
                    itemDataCopy.TryGetString(GDMConstants.SchemaKey, out itemSchemaType);
                    foreach(string curItemKey in AllItems.Keys)
                    {
                        Dictionary<string, object> curItemData;
                        AllItems.TryGetValue(curItemKey, out curItemData);

                        if (curItemData == null)
                            continue;

                        // Update any single field references: ex. custom_type myField = "oldKey"
                        List<string> fieldsOfSchemaType = ItemFieldKeysOfType(curItemKey, itemSchemaType, 0);
                        foreach(string itemFieldKey in fieldsOfSchemaType)
                        {
                            string curItemFieldValue;
                            curItemData.TryGetString(itemFieldKey, out curItemFieldValue);

                            if (!string.IsNullOrEmpty(curItemFieldValue) && curItemFieldValue.Equals(oldItemKey))
                                curItemData.TryAddOrUpdateValue(itemFieldKey, newItemKey);
                        }

                        // Update any references that are part of a list
                        fieldsOfSchemaType = ItemFieldKeysOfType(curItemKey, itemSchemaType, 1);
                        foreach(string itemFieldKey in fieldsOfSchemaType)
                        {
                            List<object> valueList;
                            curItemData.TryGetList(itemFieldKey, out valueList);

                            if (valueList != null)
                            {
                                List<int> indexes = valueList.AllIndexesOf(oldItemKey);
                                foreach(int index in indexes)
                                    valueList[index] = newItemKey;
                            }
                        }

						// Update any references tha are part of a 2D list
						fieldsOfSchemaType = ItemFieldKeysOfType(curItemKey, itemSchemaType, 2);
						foreach(string itemFieldKey in fieldsOfSchemaType)
						{
							List<List<object>> valueList;
							curItemData.TryGetTwoDList(itemFieldKey, out valueList);

							if (valueList != null)
							{
								foreach(List<object> sublist in valueList)
								{
									List<int> indexes = sublist.AllIndexesOf(oldItemKey);
									foreach(int index in indexes)
										sublist[index] = newItemKey;
								}
							}
						}
                    }
                }
                else
                {
                    error = GDEConstants.FailedToReadItemData + " " + oldItemKey;
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            ItemsNeedSave |= result;

            return result;
        }

        public static bool RenameSchemaField(string oldFieldKey, string newFieldKey, string schemaKey, Dictionary<string, object> schemaData, out string error)
        {
            bool result = true;

            if (!IsFieldNameValid(schemaData, newFieldKey, out error))
            {
                result = false;
            }
            else if (schemaData.ContainsKey(newFieldKey))
            {
                result = false;
                error = GDEConstants.FieldNameExists + " " + newFieldKey;
            }
            else
            {
                // Do rename
                RenameField(oldFieldKey, newFieldKey, schemaData);

                // Remove the schema key from the listbyfieldname List
                List<string> schemaKeyList;
                if(ListByFieldName.TryGetValue(oldFieldKey, out schemaKeyList))
                {
                    schemaKeyList.Remove(schemaKey);
                    if (schemaKeyList.Count == 0)
                        ListByFieldName.Remove(oldFieldKey);
                }

                // Add the schema key to the listbyfieldname List under the new field name
                if (ListByFieldName.TryGetValue(newFieldKey, out schemaKeyList))
                    schemaKeyList.Add(schemaKey);
                else
                {
                    List<string> newListByFieldName = new List<string>(){schemaKey};
                    ListByFieldName.Add(newFieldKey, newListByFieldName);
                }

                // Rename the fields in any existing items with this schema
                List<string> itemKeys = GetItemsOfSchemaType(schemaKey);
                foreach(string itemKey in itemKeys)
                {
                    Dictionary<string, object> itemData;
                    if (AllItems.TryGetValue(itemKey, out itemData))
                        RenameField(oldFieldKey, newFieldKey, itemData);
                }
            }

            ItemsNeedSave |= result;
            SchemasNeedSave |= result;

            return result;
        }

        static void RenameField(string oldFieldKey, string newFieldKey, Dictionary<string, object> data)
        {
            object value;
            if (data.TryGetValue(oldFieldKey, out value))
            {
                data.Add(newFieldKey, value);
                data.Remove(oldFieldKey);
            }

            string oldKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, oldFieldKey);
            string newKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, newFieldKey);
            if (data.TryGetValue(oldKey, out value))
            {
                data.Add(newKey, value);
                data.Remove(oldKey);
            }

            oldKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, oldFieldKey);
            newKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, newFieldKey);
            if (data.TryGetValue(oldKey, out value))
            {
                data.Add(newKey, value);
                data.Remove(oldKey);
            }
        }
        #endregion

        #region Filter Methods
        // Returns false if any fields in the given schema start with the given field name
        // Returns true otherwise
        public static bool ShouldFilterByField(string schemaKey, string fieldName)
        {
            List<string> schemaKeyList = null;
            foreach(KeyValuePair<string, List<string>> pair in ListByFieldName)
            {
                if (pair.Key.Contains(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    schemaKeyList = pair.Value;
					if (schemaKeyList.Contains(schemaKey))
                        return false;
                }
            }

            return true;
        }
        #endregion

        #region Validation Methods
        public static bool IsSchemaNameValid(string name, out string error)
        {
            bool result = true;
            error = "";

            if (AllSchemas.ContainsKey(name))
            {
                error = GDEConstants.SchemaNameExists + " " + name;
                result = false;
            }
            else if (!GDEValidateIdentifier.IsValidIdentifier(name))
            {
                error = GDEConstants.SchemaNameInvalid + " " + name;
                result = false;
            }

            return result;
        }

        public static bool IsFieldNameValid(string schemaKey, string fieldName, out string error)
        {
            bool result = true;
            error = "";

            Dictionary<string, object> data;
            if (AllSchemas.TryGetValue(schemaKey, out data))
            {
                result = IsFieldNameValid(data, fieldName, out error);
            }
            else
            {
                result = false;
                error = GDEConstants.ErrorReadingSchema + " " + schemaKey;
            }

            return result;
        }

        public static bool IsFieldNameValid(Dictionary<string, object> data, string fieldName, out string error)
        {
            bool result = true;
            error = "";

            if (data.ContainsKey(fieldName))
            {
                error = GDEConstants.FieldNameExists + " " + fieldName;
                result = false;
            }
            else if (!GDEValidateIdentifier.IsValidIdentifier(fieldName))
            {
                error = GDEConstants.FieldNameInvalid + " " + fieldName;
                result = false;
            }

            return result;
        }

        public static bool IsItemNameValid(string name, out string error)
        {
            bool result = true;
            error = "";

            if(string.IsNullOrEmpty(name))
            {
                error = GDEConstants.ItemNameInvalid + " " + name;
                result = false;
            }
            else if(AllItems.ContainsKey(name))
            {
                error = GDEConstants.ItemNameExists + " " + name;
                result = false;
            }

            return result;
        }
        #endregion

        #region Helper Methods
        public static object GetDefaultValueForType(BasicFieldType type)
        {
            object defaultValue = 0;
            if (type.IsSet(BasicFieldType.Vector2))
            {
                defaultValue = new Dictionary<string, object>()
                {
                    {"x", 0f},
                    {"y", 0f}
                };
            }
            else if (type.IsSet(BasicFieldType.Vector3))
            {
                defaultValue = new Dictionary<string, object>()
                {
                    {"x", 0f},
                    {"y", 0f},
                    {"z", 0f}
                };
            }
            else if (type.IsSet(BasicFieldType.Vector4))
            {
                defaultValue = new Dictionary<string, object>()
                {
                    {"x", 0f},
                    {"y", 0f},
                    {"z", 0f},
                    {"w", 0f}
                };
            }
            else if (type.IsSet(BasicFieldType.String))
            {
                defaultValue = "";
            }
            else if (type.IsSet(BasicFieldType.Color))
            {
                defaultValue = new Dictionary<string, object>()
                {
                    {"r", 1f},
                    {"g", 1f},
                    {"b", 1f},
                    {"a", 1f},
                };
            }
			else if (GDEItemManager.IsUnityType(type))
			{
				defaultValue = null;
			}
			else
                defaultValue = 0;

            return defaultValue;
        }

		/// <summary>
		/// Clones the current DataDictionary and unloads any special
		/// types
		/// </summary>
		/// <returns>A DataDictionary copy that's ready to be saved as json or exported to excel</returns>
		public static Dictionary<string, object> GetJsonReadyDataCopy()
		{
			Dictionary<string, object> allData = new Dictionary<string, object>();

			List<string> keys = new List<string>(AllSchemas.Keys);
			foreach(var key in keys)
			{
				var schemaData = AllSchemas[key].DeepCopy();
				UnloadSpecialTypes(schemaData);

				allData.Add(string.Format(GDMConstants.MetaDataFormat, GDMConstants.SchemaPrefix, key), schemaData);
			}

			keys = new List<string>(AllItems.Keys);
			foreach(var key in keys)
			{
				var itemData = AllItems[key].DeepCopy();
				UnloadSpecialTypes(itemData);

				allData.Add(key, itemData);
			}

			return allData;
		}

		/// <summary>
		/// Iterates over all the entries in data and replaces special
		/// types with their instances (i.e. GameObjects)
		/// </summary>
		/// <param name="data">The dictionary to process</param>
		public static void LoadSpecialTypes(Dictionary<string, object> data)
		{
			string fieldType;
			string fieldName;
			string isListKey;
			int listDimension;

			var keys = new List<string>(data.Keys);

			// Post process for any GameObjects
			foreach(string key in keys)
			{
				// If its not a type key, move on
				if (!key.StartsWith(GDMConstants.TypePrefix))
					continue;

				// If its not a Unity type, move on
				fieldType = data[key].ToString();
				if (!IsUnityType(fieldType))
					continue;


				fieldName = key.Replace(GDMConstants.TypePrefix, string.Empty);
				isListKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName);
				data.TryGetInt(isListKey, out listDimension);

				if (listDimension == 0)
				{
					if (fieldType.Equals(BasicFieldType.GameObject.ToString()))
					{
						GameObject go;
						data.TryGetGameObject(fieldName, out go);
						data.TryAddOrUpdateValue(fieldName, go);
					}
					else if (fieldType.Equals(BasicFieldType.Texture2D.ToString()))
					{
						Texture2D tex;
						data.TryGetTexture2D(fieldName, out tex);
						data.TryAddOrUpdateValue(fieldName, tex);
					}
					else if (fieldType.Equals(BasicFieldType.Material.ToString()))
					{
						Material mat;
						data.TryGetMaterial(fieldName, out mat);
						data.TryAddOrUpdateValue(fieldName, mat);
					}
					else if (fieldType.Equals(BasicFieldType.AudioClip.ToString()))
					{
						AudioClip aud;
						data.TryGetAudioClip(fieldName, out aud);
						data.TryAddOrUpdateValue(fieldName, aud);
					}
				}
				else if (listDimension == 1)
				{
					if (fieldType.Equals(BasicFieldType.GameObject.ToString()))
					{
						List<GameObject> goList;
						data.TryGetGameObjectList(fieldName, out goList);
						data.TryAddOrUpdateValue(fieldName, goList);
					}
					else if (fieldType.Equals(BasicFieldType.Texture2D.ToString()))
					{
						List<Texture2D> texList;
						data.TryGetTexture2DList(fieldName, out texList);
						data.TryAddOrUpdateValue(fieldName, texList);
					}
					else if (fieldType.Equals(BasicFieldType.Material.ToString()))
					{
						List<Material> matList;
						data.TryGetMaterialList(fieldName, out matList);
						data.TryAddOrUpdateValue(fieldName, matList);
					}
					else if (fieldType.Equals(BasicFieldType.AudioClip.ToString()))
					{
						List<AudioClip> audList;
						data.TryGetAudioClipList(fieldName, out audList);
						data.TryAddOrUpdateValue(fieldName, audList);
					}
				}
				else if (listDimension == 2)
				{
					if (fieldType.Equals(BasicFieldType.GameObject.ToString()))
					{
						List<List<GameObject>> goTwoDList;
						data.TryGetGameObjectTwoDList(fieldName, out goTwoDList);
						data.TryAddOrUpdateValue(fieldName, goTwoDList);
					}
					else if (fieldType.Equals(BasicFieldType.Texture2D.ToString()))
					{
						List<List<Texture2D>> texTwoDList;
						data.TryGetTexture2DTwoDList(fieldName, out texTwoDList);
						data.TryAddOrUpdateValue(fieldName, texTwoDList);
					}
					else if (fieldType.Equals(BasicFieldType.Material.ToString()))
					{
						List<List<Material>> matTwoDList;
						data.TryGetMaterialTwoDList(fieldName, out matTwoDList);
						data.TryAddOrUpdateValue(fieldName, matTwoDList);
					}
					else if (fieldType.Equals(BasicFieldType.AudioClip.ToString()))
					{
						List<List<AudioClip>> audTwoDList;
						data.TryGetAudioClipTwoDList(fieldName, out audTwoDList);
						data.TryAddOrUpdateValue(fieldName, audTwoDList);
					}
				}
			}
		}

		/// <summary>
		/// Iterates over all the entries in data and replaces special
		/// types with their json ready representations (i.e. GameObjects)
		/// </summary>
		/// <param name="data">The dictionary to process</param>
		public static void UnloadSpecialTypes(Dictionary<string, object> data)
		{
			string fieldName;
			string isListKey;
			int listDimension;
			string path;

			var keys = new List<string>(data.Keys);

			// Post process for any GameObjects
			foreach(var key in keys)
			{
				if (key.StartsWith(GDMConstants.TypePrefix) && IsUnityType(data[key].ToString()))
				{
					fieldName = key.Replace(GDMConstants.TypePrefix, string.Empty);
					isListKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName);
					data.TryGetInt(isListKey, out listDimension);

					if (listDimension == 0)
					{
						path = GetJsonRepresentation(data[fieldName]);
						data.TryAddOrUpdateValue(fieldName, path);
					}
					else if (listDimension == 1)
					{
						var goList = data[fieldName] as IList;
						var pathList = new List<string>();
						foreach(var go in goList)
						{
							path = GetJsonRepresentation(go);
							pathList.Add(path);
						}

						data.TryAddOrUpdateValue(fieldName, pathList);
					}
					else if (listDimension == 2)
					{
						var goTwoDList = data[fieldName] as IList;
						var pathTwoDList = new List<List<string>>();
						foreach(var obj in goTwoDList)
						{
							var sublist = obj as IList;
							var pathSubList = new List<string>();
							foreach(var go in sublist)
							{
								path = GetJsonRepresentation(go);
								pathSubList.Add(path);
							}
							pathTwoDList.Add(pathSubList);
						}

						data.TryAddOrUpdateValue(fieldName, pathTwoDList);
					}
				}
			}
		}

		public static T GetTypeFromJson<T>(string value) where T : UnityEngine.Object
		{
			return Resources.Load<T>(value);
		}

		public static string GetJsonRepresentation(object value)
		{
			string result = string.Empty;

			if (value != null && value.DerivesFromUnityObject())
				result = AssetDatabase.GetAssetPath(value as UnityEngine.Object).StripAssetPath();

			return result;
		}

		public static string GetVariableTypeFor(BasicFieldType type)
		{
			string result = type.ToString();

			if (type.Equals(BasicFieldType.Bool) ||
			    type.Equals(BasicFieldType.Float) ||
			    type.Equals(BasicFieldType.Int) ||
			    type.Equals(BasicFieldType.String))
				result = result.ToLower();

			return result;
		}

		public static bool IsUnityType(BasicFieldType type)
		{
			return type.Equals(BasicFieldType.GameObject) ||
				type.Equals(BasicFieldType.Texture2D) ||
				type.Equals(BasicFieldType.Material) ||
				type.Equals(BasicFieldType.AudioClip);
		}

		public static bool IsUnityType(string type)
		{
			return type.Equals(BasicFieldType.GameObject.ToString()) ||
				type.Equals(BasicFieldType.Texture2D.ToString()) ||
				type.Equals(BasicFieldType.Material.ToString()) ||
				type.Equals(BasicFieldType.AudioClip.ToString());
		}
        #endregion
    }
}
