using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameDataEditor
{
	[Serializable]
	public class GDESettings
	{
		const string ACCESS_TOKEN_KEY = "gde_at";
		const string REFRESH_TOKEN_KEY = "gde_rt";
		const string ACCESS_TOKEN_TIMEOUT_KEY = "gde_t";
		const string CreateDataColorKey = "gde_createdatacolor";
		const string DefineDataColorKey = "gde_definedatacolor";
		const string HighlightColorKey = "gde_highlightcolor";
		const string DataFileKey = "gde_datafile";
		const string WorkbookFilePathKey = "gde_workbookpath";

		public string ImportedLocalSpreadsheetName;
		public string ExportedLocalSpreadsheetName;

		public string ImportedGoogleSpreadsheetName;
		public string ExportedGoogleSpreadsheetPath;

		public ImportExportType ImportType;
		public ImportExportType ExportType;
        public string ExportFileMD5;

		public string AccessTokenTimeout;
		public string AccessTokenKey;
		public string RefreshTokenKey;

		bool _prettyJson;
		public bool PrettyJson
		{
			get { return _prettyJson; }
			set
			{
				_prettyJson = value;
			}
		}

		string _dataFilePath;
		public string DataFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(_dataFilePath))
					_dataFilePath = FullRootDir + "/" + GDEConstants.DefaultDataFilePath + "/" + GDEConstants.DataFile;

				return _dataFilePath;
			}
			set
			{
				_dataFilePath = value;
			}
		}

		static string _fullRootDir;
		public static string FullRootDir
		{
			get
			{
				if (!Directory.Exists(_fullRootDir))
				{
					var results = AssetDatabase.FindAssets(GDEConstants.RootDir);
					if (results != null && results.Length > 0)
					{
						string assetPath = AssetDatabase.GUIDToAssetPath(results[0]);
						string dataPath = Application.dataPath.Replace("Assets", string.Empty);

						_fullRootDir = dataPath + assetPath;
					}
					else
						_fullRootDir = Application.dataPath + GDEConstants.RootDir;
				}

				return _fullRootDir;
			}
		}

		public static string RelativeRootDir
		{
			get
			{
				return FullRootDir.Replace(Application.dataPath, "Assets");
			}
		}

		public static string DefaultDataFilePath
		{
			get
			{
				return FullRootDir + "/" + GDEConstants.DefaultDataFilePath + "/" + GDEConstants.DataFile;
			}
		}

		public string CreateDataColor;
		public string DefineDataColor;
		public string HighlightColor;

		static string settingsPath = FullRootDir + Path.DirectorySeparatorChar + GDEConstants.SettingsPath;

		static GDESettings _instance;
		public static GDESettings Instance
		{
			get
			{
				if (_instance == null)
					Load();
				return _instance;
			}
		}

		GDESettings()
		{
			ImportedLocalSpreadsheetName = EditorPrefs.GetString(WorkbookFilePathKey, string.Empty);
			ExportedLocalSpreadsheetName = string.Empty;

			ImportedGoogleSpreadsheetName = string.Empty;
			ExportedGoogleSpreadsheetPath = string.Empty;

			AccessTokenKey = string.Empty;
			RefreshTokenKey = string.Empty;
			AccessTokenTimeout = string.Empty;

			_dataFilePath = EditorPrefs.GetString(DataFileKey, DefaultDataFilePath);

			if (EditorGUIUtility.isProSkin)
			{
				CreateDataColor = EditorPrefs.GetString(CreateDataColorKey, GDEConstants.CreateDataColorPro);
				DefineDataColor = EditorPrefs.GetString(DefineDataColorKey, GDEConstants.DefineDataColorPro);
			}
			else
			{
				CreateDataColor = EditorPrefs.GetString(CreateDataColorKey, GDEConstants.CreateDataColor);
				DefineDataColor = EditorPrefs.GetString(DefineDataColor, GDEConstants.DefineDataColor);
			}

			HighlightColor = EditorPrefs.GetString(HighlightColorKey, GDEConstants.HighlightColor);
            ImportType = ImportExportType.None;
            ExportType = ImportExportType.None;
            ExportFileMD5 = string.Empty;

			// Delete the editor prefs keys if they exist
			if (EditorPrefs.HasKey(DataFileKey))
				EditorPrefs.DeleteKey(DataFileKey);

			if (EditorPrefs.HasKey(ACCESS_TOKEN_KEY))
				EditorPrefs.DeleteKey(ACCESS_TOKEN_KEY);

			if (EditorPrefs.HasKey(REFRESH_TOKEN_KEY))
				EditorPrefs.DeleteKey(REFRESH_TOKEN_KEY);

			if (EditorPrefs.HasKey(ACCESS_TOKEN_TIMEOUT_KEY))
				EditorPrefs.DeleteKey(ACCESS_TOKEN_TIMEOUT_KEY);

			if (EditorPrefs.HasKey(CreateDataColorKey))
				EditorPrefs.DeleteKey(CreateDataColorKey);

			if (EditorPrefs.HasKey(DefineDataColorKey))
				EditorPrefs.DeleteKey(DefineDataColorKey);

			if (EditorPrefs.HasKey(HighlightColorKey))
				EditorPrefs.DeleteKey(HighlightColorKey);

			if (EditorPrefs.HasKey(WorkbookFilePathKey))
				EditorPrefs.DeleteKey(WorkbookFilePathKey);
		}

		public void Save()
		{
			using (var stream = new MemoryStream())
			{
				BinaryFormatter bin = new BinaryFormatter();
				bin.Serialize(stream, this);

				File.WriteAllBytes(settingsPath, stream.ToArray());
			}
		}

		static void Load()
		{
			if (File.Exists(settingsPath))
			{
				byte[] bytes = File.ReadAllBytes(settingsPath);

				using (var stream = new MemoryStream(bytes))
				{
					BinaryFormatter bin = new BinaryFormatter();
					_instance = bin.Deserialize(stream) as GDESettings;
				}
			}
			else
			{
				_instance = new GDESettings();
			}
		}
	}
}
