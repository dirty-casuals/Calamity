using UnityEngine;
using System;
using OfficeOpenXml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GameDataEditor
{
	public class GDEExcelDataHelper
	{
		const string KEYS_RANGE = "a:a";

		public const string IgnoreToken = "gde_ignore";
		public const string FieldNameToken = "gde_field_names";
		public const string FieldTypeToken = "gde_field_types";

		const string LIST_PREFIX = "list_";
		const string LIST2D_PREFIX = "list2d_";

		const int NameRow = 1;
		const int TypeRow = 2;

		public delegate void UpdateProgressAction(string title, string msg, float progress);
		public UpdateProgressAction OnUpdateProgress;

		string progressMessage = "";
		int processedItems = 0;
		int totalItems = 1;

		class FieldInfo
		{
			public string name;
			public BasicFieldType type;
			public string customType;
			
			public bool isList;
			public bool is2DList;
			public bool isCustom;
			public bool skip;
			
			public string cellCol;
			public string cellRow;
			public string cellID
			{
				get{ return cellCol + cellRow; }
				set{}
			}

			public FieldInfo()
			{
				name = string.Empty;
				type = BasicFieldType.Undefined;
				customType = string.Empty;
                
                isList = false;
				is2DList = false;
				isCustom = false;
				skip = false;

				cellCol = string.Empty;
				cellRow = string.Empty;
			}
		}

		ExcelPackage _excelPackage;
		
		public GDEExcelDataHelper(string filePath) : this(filePath, false)
		{
		}
		
		public GDEExcelDataHelper(string filePath, bool useNewWorkbook)
		{
			FileInfo newFile = new FileInfo(filePath);
			if (newFile.Exists && useNewWorkbook)
			{
				newFile.Delete();  // ensures we create a new workbook
				newFile = new FileInfo(filePath);
			}
			_excelPackage = new ExcelPackage(newFile);
		}
		
		public List<string> GetSheetNames()
		{
			List<string> sheetNames = new List<string>();
			
			foreach (var ws in _excelPackage.Workbook.Worksheets)
				sheetNames.Add(ws.Name);
			
			return sheetNames;
		}

		public ExcelRange GetSheetData(string name)
		{
			ExcelWorksheet sheet = _excelPackage.Workbook.Worksheets[name];

			if (sheet == null)
				return null;

			return sheet.Cells;
		}

		public ExcelCellAddress GetFieldNameRowForSheet(string name)
		{
			return GetRowForSheet(name, FieldNameToken);
		}

		public ExcelCellAddress GetFieldTypeRowForSheet(string name)
		{
			return GetRowForSheet(name, FieldTypeToken);
		}

		ExcelCellAddress GetRowForSheet(string name, string rowToken)
		{
			ExcelWorksheet sheet = _excelPackage.Workbook.Worksheets[name];
			
			if (sheet == null)
				return null;
			
			var nameCell = sheet.Cells[KEYS_RANGE].First(cell => cell != null && !string.IsNullOrEmpty(cell.Text) &&
			                                        cell.Text.Trim().ToLower().Equals(rowToken));

			ExcelCellAddress address = new ExcelCellAddress(nameCell.Address);
			return address;
		}

		List<FieldInfo> GetFields(string sheetName, ExcelRange sheetCells)
		{
			List<FieldInfo> fields = new List<FieldInfo>();
			
			var fieldTypeCell = GetFieldTypeRowForSheet(sheetName);
			var fieldNameCell = GetFieldNameRowForSheet(sheetName);
			
			var nameRow = sheetCells["b"+fieldNameCell.Row+":az"+fieldNameCell.Row];
			
			foreach(var cell in nameRow)
			{
				FieldInfo fieldInfo;
				string cellAddress = GDEExcelDataHelper.GetExcelColumnName(cell.End.Column)+"1";
				if (sheetCells[cellAddress].Text.Trim().ToLower().Equals(GDEExcelDataHelper.IgnoreToken))
				{
					fieldInfo = new FieldInfo();
					fieldInfo.skip = true;
					fields.Add(fieldInfo);
				}
				else if (!string.IsNullOrEmpty(cell.Text))
				{
					cellAddress = GDEExcelDataHelper.GetExcelColumnName(cell.End.Column)+fieldTypeCell.Row;
					var typeCell = sheetCells[cellAddress];
					ParseFieldType(typeCell.Text, out fieldInfo);
					fieldInfo.name = cell.Text.Trim();
					
					fieldInfo.cellCol = GDEExcelDataHelper.GetExcelColumnName(cell.End.Column);
					fieldInfo.cellRow = cell.End.Row.ToString();
					
					fields.Add(fieldInfo);
				}
			}
			
			return fields;
		}

		public void ProcessSheet()
		{
			try
			{
				// Clear any data already loaded into the GDEItemManager
				GDEItemManager.ClearAll();
				GDEItemManager.Save();
				
				List<string> sheetNames = GetSheetNames();
				Dictionary<string, List<FieldInfo>> allFields = new Dictionary<string, List<FieldInfo>>();
				Dictionary<string, ExcelRange> allSheetData = new Dictionary<string, ExcelRange>();
				
				// Calculate Progressbar settings
				progressMessage = GDEConstants.ImportingScehemasLbl;
				processedItems = 0;
				totalItems = sheetNames.Count;
				
				// Create all the schemas first
				foreach(string name in sheetNames)
				{
					if (OnUpdateProgress != null)
						OnUpdateProgress(GDEConstants.ImportingGameDataLbl, progressMessage, processedItems/totalItems);
					
					ExcelRange sheetRows = GetSheetData(name);
					allSheetData.Add(name, sheetRows);
					
					List<FieldInfo> fields = GetFields(name, sheetRows);
					allFields.Add(name, fields);
					
					CreateSchema(name, fields);
					
					processedItems++;
				}
				
				// Calculate Progressbar settings
				progressMessage = GDEConstants.ImportingItemsLbl;
				processedItems = 0;
				totalItems = 0;

				// Then create all the items for each schema       
				foreach(string name in sheetNames)
				{
					ExcelRange sheetRows = allSheetData[name];
					List<FieldInfo> fields = allFields[name];
					
					CreateItems(name, fields, sheetRows);
				}
				
				GDEItemManager.Save();
			}
			catch(Exception ex)
			{
				Debug.LogError(ex);
			}
		}

		void CreateSchema(string name, List<FieldInfo> fields)
		{
			string error;
			Dictionary<string, object> schemaData = new Dictionary<string, object>();
			GDEItemManager.AddSchema(name, schemaData, out error, true);
			
			foreach(FieldInfo field in fields)
			{
				if (field.skip)
					continue;
				else if (field.type != BasicFieldType.Undefined)
				{
					GDEItemManager.AddBasicFieldToSchema(field.type, name, schemaData, field.name, out error, field.isList, 
					                                     field.is2DList, GDEItemManager.GetDefaultValueForType(field.type));
				}
				else
				{
					GDEItemManager.AddCustomFieldToSchema(field.customType, name, schemaData, field.name, field.isList, field.is2DList, out error);
				}
				
				if (error != string.Empty)
					Debug.LogError(string.Format(GDEConstants.ErrorInSheet, name, error, field.cellID));
			}
		}
		
		void CreateItems(string schemaName, List<FieldInfo> fields, ExcelRange sheetCells)
		{
			Dictionary<string, object> schemaData = null;       
			GDEItemManager.AllSchemas.TryGetValue(schemaName, out schemaData);
			
			// If schema failed to parse (schema data is null), we can't parse any items
			if (schemaData == null)
				return;
			
			string itemName;
			string error;
			Dictionary<string, object> itemData;
			
			var itemKeysColumn = (from cell in sheetCells["a:a"]
			                      where cell != null &&
			                      !string.IsNullOrEmpty(cell.Text) &&
			                      !cell.Text.Trim().ToLower().Equals(GDEExcelDataHelper.IgnoreToken) &&
			                      !cell.Text.Trim().ToLower().Equals(GDEExcelDataHelper.FieldNameToken) &&
			                      !cell.Text.Trim().ToLower().Equals(GDEExcelDataHelper.FieldTypeToken)
			                      select cell);

			// Add to the total item count
			totalItems += itemKeysColumn.Count();

			foreach(var keyCell in itemKeysColumn)
			{
				itemName = keyCell.Text.Trim();
				itemData = schemaData.DeepCopy();
				itemData.Add(GDMConstants.SchemaKey, schemaName);
				
				for(int x=0;  x<fields.Count;  x++)
				{
					try
					{
						string cellText = sheetCells[fields[x].cellCol+keyCell.End.Row].Text;
						List<object> matches;
						if (fields[x].skip)
							continue;
						else if (fields[x].isList)
						{
							if (fields[x].type.Equals(BasicFieldType.String) || 
							    fields[x].type.Equals(BasicFieldType.Vector2) ||
							    fields[x].type.Equals(BasicFieldType.Vector3) ||
							    fields[x].type.Equals(BasicFieldType.Vector4) ||
							    fields[x].type.Equals(BasicFieldType.Color) ||
							    GDEItemManager.IsUnityType(fields[x].type) ||
							    fields[x].isCustom)
							{
								matches = GDEParser.Parse(cellText);
							}
							else
							{
								matches = new List<object>(cellText.Split(','));
							}
							
							itemData[fields[x].name] = ConvertListValueToType(fields[x].type, matches);
						}
						else if (fields[x].is2DList)
						{
							if (fields[x].type.Equals(BasicFieldType.GameObject))
								itemData[fields[x].name] = LoadUnityType2DList<GameObject>(cellText);
							else if (fields[x].type.Equals(BasicFieldType.Texture2D))
								itemData[fields[x].name] = LoadUnityType2DList<Texture2D>(cellText);
							else if (fields[x].type.Equals(BasicFieldType.Material))
								itemData[fields[x].name] = LoadUnityType2DList<Material>(cellText);
							else if (fields[x].type.Equals(BasicFieldType.AudioClip))
								itemData[fields[x].name] = LoadUnityType2DList<AudioClip>(cellText);
							else
								itemData[fields[x].name] = Json.Deserialize(cellText);
						}
						else
						{
							if (fields[x].type.Equals(BasicFieldType.Vector2) ||
							    fields[x].type.Equals(BasicFieldType.Vector3) ||
							    fields[x].type.Equals(BasicFieldType.Vector4) ||
							    fields[x].type.Equals(BasicFieldType.Color))
							{
								matches = new List<object>(cellText.Split(','));
								matches.ForEach(obj => obj = obj.ToString().Trim());
								itemData[fields[x].name] = ConvertValueToType(fields[x].type, matches);
							}
							else
								itemData[fields[x].name] = ConvertValueToType(fields[x].type, cellText);
						}
					}
					catch
					{
						Debug.LogError(string.Format(GDEConstants.ErrorParsingCellFormat, schemaName, fields[x].cellCol+keyCell.End.Row));
						object defaultValue = GDEItemManager.GetDefaultValueForType(fields[x].type);
						if (fields[x].isList || fields[x].is2DList)
							itemData[fields[x].name] = new List<object>();
						else
							itemData[fields[x].name] = defaultValue;
					}
					processedItems++;

					if (OnUpdateProgress != null)
						OnUpdateProgress(GDEConstants.ImportingGameDataLbl, progressMessage, processedItems/totalItems);
				}

				GDEItemManager.AddItem(itemName, itemData, out error);
			}
		}

		public void ExportToSheet(Dictionary<string, List<string>> sortedItems)
		{
			foreach(var itemGroup in sortedItems)
			{
				string schemaName = itemGroup.Key;
				List<string> itemKeys = itemGroup.Value;

				// Load the Field Info list
				List<FieldInfo> fieldInfoList = GetFieldInfoFromSchema(schemaName, GDEItemManager.AllSchemas[schemaName]);

				// Create the sheet
				ExcelWorksheet worksheet = _excelPackage.Workbook.Worksheets.Add(schemaName);

				// Write the field tokens
				worksheet.Cells[NameRow, 1].Value = FieldNameToken.ToUpper();
				worksheet.Cells[TypeRow, 1].Value = FieldTypeToken.ToUpper();

				// Write the field name and type rows
				for(int i=0;  i<fieldInfoList.Count;  i++)
				{
					worksheet.Cells[NameRow, i+2].Value = fieldInfoList[i].name;
					worksheet.Cells[TypeRow, i+2].Value = ConvertFieldTypeToSheetFormat(fieldInfoList[i]);
				}

				// Add the items
				for(int i=0;  i<itemKeys.Count;  i++)
				{
					Dictionary<string, object> itemData = GDEItemManager.AllItems[itemKeys[i]];

					// +1 for starting at 1
					// +2 to skip the name and type rows
					int row = i + 3;

					// +1 for starting at 1
					int col = 1;

					// Write the key
					worksheet.Cells[row, col].Value = itemKeys[i];

					// Iterate over the field list and convert the values
					// to spreadsheet format
					for(int x = 0;  x < fieldInfoList.Count;  x++)
					{
						// +1 for starting at 1
						// +1 for skipping the key column
						col = x + 2;

						FieldInfo field = fieldInfoList[x];
						object fieldValue;

						if (field.isList)
							fieldValue = ConvertListToSheetFormat(field.type, itemData[field.name]);
						else if (field.is2DList)
							fieldValue = Convert2DListToSheetFormat(field.type, itemData[field.name]);
						else
							fieldValue = ConvertToSheetFormat(field.type, itemData[field.name]);

						worksheet.Cells[row, col].Value = fieldValue;
					}
				}

				try
				{
					// Autosize columns where gdiplus is available
					// Not working on osx
					worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
				}
				catch
				{
					// Do nothing
				}

			}

			_excelPackage.Save();
			Debug.Log(GDEConstants.ExportCompleteLbl);
		}

		List<FieldInfo> GetFieldInfoFromSchema(string schemaKey, Dictionary<string, object> schemaData)
		{
			List<FieldInfo> fieldInfos = new List<FieldInfo>();
			List<string> fieldKeys = GDEItemManager.SchemaFieldKeys(schemaKey, schemaData);

			foreach(var fieldKey in fieldKeys)
			{
				int listDimension;

				FieldInfo field = new FieldInfo();

				field.name = fieldKey;
				string isListKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldKey);
				schemaData.TryGetInt(isListKey, out listDimension);

				// Set the list dimension
				if (listDimension == 0)
				{
					field.isList = false;
					field.is2DList = false;
				}
				else if (listDimension == 1)
				{
					field.isList = true;
					field.is2DList = false;
				}
				else
				{
					field.isList = false;
					field.is2DList = true;
				}

				// Parse the field type
				string customType;
				field.type = ParseFieldType(fieldKey, schemaData, out customType);
				if (field.type.Equals(BasicFieldType.Undefined))
				{
					field.isCustom = true;
					field.customType = customType;
				}

				fieldInfos.Add(field);
			}

			return fieldInfos;
		}

		#region Helper Methods
		public static string GetExcelColumnName(int columnNumber)
		{
			int dividend = columnNumber;
			string columnName = String.Empty;
			int modulo;
			
			while (dividend > 0)
			{
				modulo = (dividend - 1) % 26;
				columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
				dividend = (int)((dividend - modulo) / 26);
			} 
			
			return columnName;
		}
		
		bool ParseFieldType(string type, out FieldInfo fieldInfo)
		{
			bool result = true;
			
			fieldInfo = new FieldInfo();
			
			try
			{
				if(type.StartsWith(LIST_PREFIX))
				{
					type = type.Replace(LIST_PREFIX, string.Empty);
					fieldInfo.isList = true;
				}
				else if (type.StartsWith(LIST2D_PREFIX))
				{
					type = type.Replace(LIST2D_PREFIX, string.Empty);
					fieldInfo.is2DList = true;
				}
				
				fieldInfo.type = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), type, true);
			}
			catch
			{
				result = false;
				fieldInfo.type = BasicFieldType.Undefined;
				fieldInfo.customType = type;
				fieldInfo.isCustom = true;
			}
			
			return result;
		}

		List<object> ConvertListValueToType(BasicFieldType type, List<object> values)
		{
			List<object> convertedValues = new List<object>();
			
			if (type.Equals(BasicFieldType.String) || type.Equals(BasicFieldType.Undefined))
				values.ForEach(val => convertedValues.Add(val));
			else        
				values.ForEach(val => convertedValues.Add(ConvertValueToType(type, val)));
			
			return convertedValues;
		}

		object ConvertValueToType(BasicFieldType type, object value)
		{
			object convertedValue = 0;
			
			switch(type)
			{
				case BasicFieldType.Bool:
				{
					string b = value.ToString().Trim();
					if (b.ToString().Equals("0"))
						convertedValue = false;
					else if (b.ToString().Equals("1"))
						convertedValue = true;
					else
						convertedValue = Convert.ToBoolean(b);
					
					break;
				}
				case BasicFieldType.Int:
				{
					convertedValue = Convert.ToInt32(value);
					break;
				}
				case BasicFieldType.Float:
				{
					convertedValue = Convert.ToSingle(value);
					break;
				}
				case BasicFieldType.String:
				{
					convertedValue = value.ToString();
					break;
				}    
				case BasicFieldType.Color:
				{
					List<object> colorValues = value as List<object>;
					Dictionary<string, object> colorDict = new Dictionary<string, object>();
					colorDict.TryAddOrUpdateValue("r", Convert.ToSingle(colorValues[0])/255f);
					colorDict.TryAddOrUpdateValue("g", Convert.ToSingle(colorValues[1])/255f);
					colorDict.TryAddOrUpdateValue("b", Convert.ToSingle(colorValues[2])/255f);
					colorDict.TryAddOrUpdateValue("a", Convert.ToSingle(colorValues[3]));
					
					convertedValue = colorDict;
					
					break;
				}
				case BasicFieldType.Vector2:
				{
					List<object> vectValues = value as List<object>;
					Dictionary<string, object> vectDict = new Dictionary<string, object>();
					vectDict.TryAddOrUpdateValue("x", Convert.ToSingle(vectValues[0]));
					vectDict.TryAddOrUpdateValue("y", Convert.ToSingle(vectValues[1]));
					
					convertedValue = vectDict;
					
					break;
				}
				case BasicFieldType.Vector3:
				{
					List<object> vectValues = value as List<object>;
					Dictionary<string, object> vectDict = new Dictionary<string, object>();
					vectDict.TryAddOrUpdateValue("x", Convert.ToSingle(vectValues[0]));
					vectDict.TryAddOrUpdateValue("y", Convert.ToSingle(vectValues[1]));
					vectDict.TryAddOrUpdateValue("z", Convert.ToSingle(vectValues[2]));
					
					convertedValue = vectDict;
					
					break;
				}
				case BasicFieldType.Vector4:
				{
					List<object> vectValues = value as List<object>;
					Dictionary<string, object> vectDict = new Dictionary<string, object>();
					vectDict.TryAddOrUpdateValue("x", Convert.ToSingle(vectValues[0]));
					vectDict.TryAddOrUpdateValue("y", Convert.ToSingle(vectValues[1]));
					vectDict.TryAddOrUpdateValue("z", Convert.ToSingle(vectValues[2]));
					vectDict.TryAddOrUpdateValue("w", Convert.ToSingle(vectValues[3]));
					
					convertedValue = vectDict;
					
					break;
				}
				case BasicFieldType.GameObject:
				{
					convertedValue = GDEItemManager.GetTypeFromJson<GameObject>(value.ToString());
					break;
				}
				case BasicFieldType.Texture2D:
				{
					convertedValue = GDEItemManager.GetTypeFromJson<Texture2D>(value.ToString());
					break;
				}
				case BasicFieldType.Material:
				{
					convertedValue = GDEItemManager.GetTypeFromJson<Material>(value.ToString());
					break;
				}
				case BasicFieldType.AudioClip:
				{
					convertedValue = GDEItemManager.GetTypeFromJson<AudioClip>(value.ToString());
					break;
				}
				case BasicFieldType.Undefined:
				{
					if (value != null)
						convertedValue = value.ToString();
					break;
				}
			}
			
			return convertedValue;
		}

		object ConvertToSheetFormat(BasicFieldType type, object value)
		{
			object result;

			switch(type)
			{
				case BasicFieldType.Color:
				{
					Dictionary<string, object> dict = value as Dictionary<string, object>;
					result = (Convert.ToSingle(dict["r"])*255f).ToString() + ",";
					result += (Convert.ToSingle(dict["g"])*255f).ToString() + ",";
					result += (Convert.ToSingle(dict["b"])*255f).ToString() + ",";
					result += Convert.ToSingle(dict["a"]).ToString();
					break;
				}
				case BasicFieldType.Vector2:
				{
					Dictionary<string, object> dict = value as Dictionary<string, object>;
					result = (Convert.ToSingle(dict["x"])).ToString() + ",";
					result += (Convert.ToSingle(dict["y"])).ToString();
					break;
				}
				case BasicFieldType.Vector3:
				{
					Dictionary<string, object> dict = value as Dictionary<string, object>;
					result = (Convert.ToSingle(dict["x"])).ToString() + ",";
					result += (Convert.ToSingle(dict["y"])).ToString() + ",";
					result += (Convert.ToSingle(dict["z"])).ToString();
					break;
				}
				case BasicFieldType.Vector4:
				{
					Dictionary<string, object> dict = value as Dictionary<string, object>;
					result = (Convert.ToSingle(dict["x"])).ToString() + ",";
					result += (Convert.ToSingle(dict["y"])).ToString() + ",";
					result += (Convert.ToSingle(dict["z"])).ToString() + ",";
					result += (Convert.ToSingle(dict["w"])).ToString();
					break;
				}
				case BasicFieldType.GameObject:
				case BasicFieldType.Texture2D:
				case BasicFieldType.Material:
				case BasicFieldType.AudioClip:
				{
					result = GDEItemManager.GetJsonRepresentation(value as UnityEngine.Object);
					break;
				}
				default:
				{
					result = value;
					break;
				}
			}

			return result;
		}

		string ConvertListToSheetFormat(BasicFieldType type, object value)
		{
			string result = string.Empty;
			bool isFirst = true;

			switch(type)
			{
				case BasicFieldType.Color:
				{
					List<object> dict = value as List<object>;
					dict.ForEach(x => {
						Dictionary<string, object> entry = x as Dictionary<string, object>;
						if (!isFirst)
							result += ",";
						else
							isFirst = false;

						result += "(";
						result += (Convert.ToSingle(entry["r"])*255f).ToString() + ",";
						result += (Convert.ToSingle(entry["g"])*255f).ToString() + ",";
						result += (Convert.ToSingle(entry["b"])*255f).ToString() + ",";
						result += Convert.ToSingle(entry["a"]).ToString();
						result += ")";
					});
					
					break;
				}
				case BasicFieldType.Vector2:
				{
					List<object> dict = value as List<object>;
					dict.ForEach(x => {
						Dictionary<string, object> entry = x as Dictionary<string, object>;
						
						if (!isFirst)
							result += ",";
						else
							isFirst = false;
						
						result += "(";
						result += (Convert.ToSingle(entry["x"])).ToString() + ",";
						result += (Convert.ToSingle(entry["y"])).ToString();
						result += ")";
					});
					break;
				}
				case BasicFieldType.Vector3:
				{
					List<object> dict = value as List<object>;
					dict.ForEach(x => {
						Dictionary<string, object> entry = x as Dictionary<string, object>;

						if (!isFirst)
							result += ",";
						else
							isFirst = false;
						
						result += "(";
						result += (Convert.ToSingle(entry["x"])).ToString() + ",";
						result += (Convert.ToSingle(entry["y"])).ToString() + ",";
						result += (Convert.ToSingle(entry["z"])).ToString();
						result += ")";
					});
					break;
				}
				case BasicFieldType.Vector4:
				{
					List<object> dict = value as List<object>;
					dict.ForEach(x => {
						Dictionary<string, object> entry = x as Dictionary<string, object>;
						
						if (!isFirst)
							result += ",";
						else
							isFirst = false;
						
						result += "(";
						result += (Convert.ToSingle(entry["x"])).ToString() + ",";
						result += (Convert.ToSingle(entry["y"])).ToString() + ",";
						result += (Convert.ToSingle(entry["z"])).ToString() + ",";
						result += (Convert.ToSingle(entry["w"])).ToString();
						result += ")";
					});
					break;
				}
				case BasicFieldType.GameObject:
				case BasicFieldType.Texture2D:
				case BasicFieldType.Material:
				case BasicFieldType.AudioClip:
				{
					IList list = value as IList;
					foreach(UnityEngine.Object entry in list)
					{
						if (!isFirst)
							result += ",";
						else
							isFirst = false;
						
						result += "\"";
						result += GDEItemManager.GetJsonRepresentation(entry);
						result += "\"";
					}
					break;
				}
				case BasicFieldType.String:
				case BasicFieldType.Undefined:
				{
					IList list = value as IList;
					foreach(var entry in list)
					{
						if (!isFirst)
							result += ",";
						else
							isFirst = false;
						
						result += "\"";
						if (entry != null)
							result += entry.ToString();
						result += "\"";
					}
					break;
				}
				default:
				{
					IList list = value as IList;
					foreach(var entry in list)
					{
						if (!isFirst)
							result += ",";
						else
							isFirst = false;

						if (entry != null)
							result += entry.ToString();
					}
					break;
				}
			}
			
			return result;
		}

		string Convert2DListToSheetFormat(BasicFieldType type, object value)
		{
			string result = string.Empty;

			if (GDEItemManager.IsUnityType(type))
			{
				var pathList = new List<List<string>>();
				IList goList = value as IList;
				foreach(var sublist in goList)
				{
					IList list = sublist as IList;
					var pathSubList = new List<string>();
					foreach(var go in list)
						pathSubList.Add(GDEItemManager.GetJsonRepresentation(go as UnityEngine.Object));
					pathList.Add(pathSubList);
				}

				// Now convert the path 2d list to json
				result = Json.Serialize(pathList);
			}
			else
				result = Json.Serialize(value);

			return result;
		}

		string ConvertFieldTypeToSheetFormat(FieldInfo info)
		{
			string result = string.Empty;

			if (info.isCustom)
				result = info.customType;
			else
				result = info.type.ToString().ToLower();

			// If its a list, prepend the list token
			if (info.isList)
				result = LIST_PREFIX+result;
			else if (info.is2DList)
				result = LIST2D_PREFIX+result;

			return result;
		}

		BasicFieldType ParseFieldType(string fieldKey, Dictionary<string, object> schemaData, out string customType)
		{
			BasicFieldType result = BasicFieldType.Undefined;
			customType = string.Empty;

			try
			{
				string typeKey = string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey);
				customType = schemaData[typeKey].ToString();

				result = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), customType, true);
			}
			catch
			{
				result = BasicFieldType.Undefined;
			}

			return result;
		}

		List<List<T>> LoadUnityType2DList<T>(string jsonText) where T : UnityEngine.Object
		{
			var goList = new List<List<T>>();
			
			var paths = Json.Deserialize(jsonText) as IList;
			foreach(var sublist in paths)
			{
				var goSubList = new List<T>();
				IList list = sublist as IList;
				foreach(var path in list)
					goSubList.Add(GDEItemManager.GetTypeFromJson<T>(path.ToString()));
				goList.Add(goSubList);
			}
			
			return goList;
		}
		#endregion
	}
}