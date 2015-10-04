using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace GameDataEditor
{
    public class GDECodeGen
    {
        public static void GenClasses(Dictionary<string, Dictionary<string, object>> allSchemas)
        {
            foreach (KeyValuePair<string, Dictionary<string, object>> pair in allSchemas)
            {
                GenClass(pair.Key, pair.Value);
            }
        }

		public static void GenStaticKeysClass(Dictionary<string, Dictionary<string, object>> allSchemas)
		{
			Debug.Log(GDEConstants.GeneratingLbl + " " + GDECodeGenConstants.StaticKeysFilePath);
			StringBuilder sb = new StringBuilder();

			sb.Append(GDECodeGenConstants.AutoGenMsg);
            sb.Append("\n");
			sb.Append(GDECodeGenConstants.StaticKeyClassHeader);

			foreach (KeyValuePair<string, Dictionary<string, object>> pair in allSchemas)
			{
				string schema = pair.Key;

				List<string> items = GDEItemManager.GetItemsOfSchemaType(schema);
				foreach(string item in items)
				{
					sb.Append("\n");
					sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
					sb.AppendFormat(GDECodeGenConstants.StaticKeyFormat, schema, item);
				}
			}

			sb.Append("\n");
			sb.Append("}".PadLeft(GDECodeGenConstants.IndentLevel1+1));
			sb.Append("\n");
			sb.Append("}");
			sb.Append("\n");

			File.WriteAllText(GDESettings.FullRootDir + Path.DirectorySeparatorChar + GDECodeGenConstants.StaticKeysFilePath, sb.ToString());

			Debug.Log(GDEConstants.DoneGeneratingLbl + " " + GDECodeGenConstants.StaticKeysFilePath);
		}

        private static void GenClass(string schemaKey, Dictionary<string, object> schemaData)
        {
            StringBuilder sb = new StringBuilder();
            string className = string.Format(GDECodeGenConstants.DataClassNameFormat, schemaKey);
            string filePath = string.Format(GDECodeGenConstants.ClassFilePathFormat, className);
            Debug.Log(GDEConstants.GeneratingLbl + " " + filePath);

            // Add the auto generated comment at the top of the file
            sb.Append(GDECodeGenConstants.AutoGenMsg);
            sb.Append("\n");

            // Append all the using statements
            sb.Append(GDECodeGenConstants.DataClassHeader);
            sb.Append("\n");

            // Append the class declaration
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel1));
            sb.AppendFormat(GDECodeGenConstants.ClassDeclarationFormat, className);
            sb.Append("\n");
            sb.Append("{".PadLeft(GDECodeGenConstants.IndentLevel1+1));
            sb.Append("\n");

            // Append all the data variables
			AppendVariableDeclarations(sb, schemaKey, schemaData);
            sb.Append("\n");

			// Append constructors
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
			sb.AppendFormat(GDECodeGenConstants.ClassConstructorsFormat, className);
			sb.Append("\n");

            // Append the load from dict method
            AppendLoadDictMethod(sb, schemaKey, schemaData);
            sb.Append("\n");

			// Append the load from saved data method
			AppendLoadFromSavedMethod(sb, schemaKey, schemaData);
			sb.Append("\n");

            // Append the reset variable methods
            AppendResetVariableMethods(sb, schemaKey, schemaData);
            sb.Append("\n");

            // Append the reset all method
            AppendResetAllMethod(sb, schemaKey, schemaData);
            sb.Append("\n");

            // Append the close class brace
            sb.Append("}".PadLeft(GDECodeGenConstants.IndentLevel1+1));
            sb.Append("\n");

            // Append the close namespace brace
            sb.Append("}");
            sb.Append("\n");

            File.WriteAllText(GDESettings.FullRootDir + Path.DirectorySeparatorChar + filePath, sb.ToString());
            Debug.Log(GDEConstants.DoneGeneratingLbl + " " + filePath);
        }

        private static void AppendVariableDeclarations(StringBuilder sb, string schemaKey, Dictionary<string, object> schemaData)
        {
            bool didAppendSpaceForSection = false;
            bool shouldAppendSpace = false;
			bool isFirstSection = true;

			string variableType;

            // Append all the single variables first
            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
				variableType = GDEItemManager.GetVariableTypeFor(fieldType);

                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), 0);
                foreach(string fieldKey in fieldKeys)
                {
                    if (shouldAppendSpace)
                        sb.Append("\n");

                    AppendVariable(sb, variableType, fieldKey);
                    shouldAppendSpace = true;
					isFirstSection = false;
                }
            }

            // Append the custom types
            foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, 0))
            {
                if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
                {
                    sb.Append("\n");
                }

                schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
                variableType = string.Format(GDECodeGenConstants.DataClassNameFormat, variableType);
                AppendCustomVariable(sb, variableType, fieldKey);

                shouldAppendSpace = true;
				isFirstSection = false;
				didAppendSpaceForSection = true;
            }
            didAppendSpaceForSection = false;

            // Append the basic lists
			for(int dimension=1;  dimension <=2;  dimension++)
			{
	            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
	            {
	                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), dimension);
					variableType = GDEItemManager.GetVariableTypeFor(fieldType);

	                foreach(string fieldKey in fieldKeys)
	                {
	                    if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
	                    {
	                        sb.Append("\n");
	                    }

	                    AppendListVariable(sb, variableType, fieldKey, dimension);

	                    shouldAppendSpace = true;
						didAppendSpaceForSection = true;
						isFirstSection = false;
	                }
	            }
	            didAppendSpaceForSection = false;
			}

            // Append the custom lists
			for(int dimension = 1;  dimension <= 2;  dimension++)
			{
	            foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, dimension))
	            {
	                if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
	                {
	                    sb.Append("\n");
	                }

	                schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
	                variableType = string.Format(GDECodeGenConstants.DataClassNameFormat, variableType);
	                AppendCustomListVariable(sb, variableType, fieldKey, dimension);

	                shouldAppendSpace = true;
					isFirstSection = false;
					didAppendSpaceForSection = true;
	            }
			}
        }

        private static void AppendLoadDictMethod(StringBuilder sb, string schemaKey, Dictionary<string, object> schemaData)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
            sb.Append(GDECodeGenConstants.LoadDictMethod);
            sb.Append("\n");

            bool shouldAppendSpace = false;
            bool didAppendSpaceForSection = false;
			bool isFirstSection = true;

            string variableType;

            // Append all the single variables first
            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
                variableType = fieldType.ToString();
                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), 0);
                foreach(string fieldKey in fieldKeys)
                {
                    AppendLoadVariable(sb, variableType, fieldKey);
                    shouldAppendSpace = true;
					isFirstSection = false;
                }
            }

            // Append the custom types
            bool appendTempKeyDeclaration = true;
            foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, 0))
            {
                if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
                {
                    sb.Append("\n");
                }

                schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
                variableType = string.Format(GDECodeGenConstants.DataClassNameFormat, variableType);

                if (appendTempKeyDeclaration)
                {
                    sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel4));
                    sb.Append(GDECodeGenConstants.TempStringKeyDeclaration);
                    sb.Append("\n");
                    appendTempKeyDeclaration = false;
                }

                AppendLoadCustomVariable(sb, variableType, fieldKey);
                shouldAppendSpace = true;
				didAppendSpaceForSection = true;
				isFirstSection = false;
            }
            didAppendSpaceForSection = false;

            // Append the basic lists
			for(int dimension = 1;  dimension <= 2;  dimension++)
			{
	            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
	            {
	                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), dimension);
	                variableType = fieldType.ToString();

	                foreach(string fieldKey in fieldKeys)
	                {
	                    if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
	                    {
	                        sb.Append("\n");
	                    }

	                    AppendLoadListVariable(sb, variableType, fieldKey, dimension);
	                    shouldAppendSpace = true;
						didAppendSpaceForSection = true;
						isFirstSection = false;
	                }
	            }
	            didAppendSpaceForSection = false;
			}

            // Append the custom lists
			for(int dimension = 1;  dimension <= 2;  dimension++)
			{
	            foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, dimension))
	            {
	                if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
	                {
	                    sb.Append("\n");
	                }

	                schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
					variableType = "Custom";
	                AppendLoadListVariable(sb, variableType, fieldKey, dimension);

					shouldAppendSpace = true;
					isFirstSection = false;
					didAppendSpaceForSection = true;
	            }
			}

			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel4));
			sb.Append(GDECodeGenConstants.LoadDictMethodEnd);
            sb.Append("\n");
        }

		private static void AppendLoadFromSavedMethod(StringBuilder sb, string schemaKey, Dictionary<string, object> schemaData)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
			sb.Append(GDECodeGenConstants.LoadFromSavedMethod);
			sb.Append("\n");

			bool shouldAppendSpace = false;
			bool didAppendSpaceForSection = false;
			bool isFirstSection = true;

			string variableType;

			// Append all the single variables first
			foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
			{
				variableType = fieldType.ToString();
				List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), 0);
				foreach(string fieldKey in fieldKeys)
				{
					AppendLoadFromSavedVariable(sb, variableType, fieldKey);
					shouldAppendSpace = true;
					isFirstSection = false;
				}
			}

			// Append the custom types
			foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, 0))
			{
				if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
				{
					sb.Append("\n");
				}

				schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
				variableType = "Custom";

				AppendLoadFromSavedCustomVariable(sb, variableType, fieldKey);
				shouldAppendSpace = true;
				didAppendSpaceForSection = true;
				isFirstSection = false;
			}
			didAppendSpaceForSection = false;

			// Append the basic lists
			for(int dimension = 1;  dimension <= 2;  dimension++)
			{
				foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
				{
					List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), dimension);
					variableType = fieldType.ToString();

					foreach(string fieldKey in fieldKeys)
					{
						if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
						{
							sb.Append("\n");
						}

						AppendLoadSavedListVariable(sb, variableType, fieldKey, dimension);
						shouldAppendSpace = true;
						didAppendSpaceForSection = true;
						isFirstSection = false;
					}
				}
				didAppendSpaceForSection = false;
			}

			// Append the custom lists
			for(int dimension = 1;  dimension <= 2;  dimension++)
			{
				foreach(string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, dimension))
				{
					if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
					{
						sb.Append("\n");
					}

					schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
					variableType = "Custom";
					AppendLoadSavedListVariable(sb, variableType, fieldKey, dimension);

					shouldAppendSpace = true;
					isFirstSection = false;
					didAppendSpaceForSection = true;
				}
			}

			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2+1));
			sb.Append(GDECodeGenConstants.LoadFromSavedMethodEnd);
			sb.Append("\n");
		}

        private static void AppendResetVariableMethods(StringBuilder sb, string schemaKey, Dictionary<string, object> schemaData)
        {
			bool shouldAppendSpace = false;
			bool didAppendSpaceForSection = false;
			bool isFirstSection = true;

            string variableType;

            // Append all the single variables first
            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
				variableType = fieldType.ToString();
                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.ToString(), 0);
                foreach(string fieldKey in fieldKeys)
                {
					if (shouldAppendSpace)
						sb.Append("\n");

                    AppendResetVariableMethod(sb, fieldKey, variableType);
					shouldAppendSpace = true;
					isFirstSection = false;
                }
            }

			// Append all list variables
			for(int dimension = 1;  dimension <= 2;  dimension++)
			{
				foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
				{
					variableType = fieldType.ToString();
					List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, variableType, dimension);
					foreach(string fieldKey in fieldKeys)
					{
						if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
						{
							sb.Append("\n");
						}

						AppendResetListVariableMethod(sb, fieldKey, variableType, dimension);
						shouldAppendSpace = true;
						didAppendSpaceForSection = true;
						isFirstSection = false;
					}
				}
				didAppendSpaceForSection = false;
			}

			// Append all custom variables
			for(int dimension = 0;  dimension <= 2;  dimension++)
			{
				List<string> fieldKeys = GDEItemManager.SchemaCustomFieldKeys(schemaKey, dimension);
				foreach(string fieldKey in fieldKeys)
				{
					if (shouldAppendSpace && !didAppendSpaceForSection && !isFirstSection)
					{
						sb.Append("\n");
					}

					schemaData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out variableType);
					variableType = string.Format(GDECodeGenConstants.DataClassNameFormat, variableType);

					AppendResetCustomVariableMethod(sb, fieldKey, variableType, dimension);
					shouldAppendSpace = true;
					didAppendSpaceForSection = true;
					isFirstSection = false;
				}

				didAppendSpaceForSection = false;
			}
        }

        #region Gen Variable Declaration Methods
        private static void AppendCustomVariable(StringBuilder sb, string type, string name)
        {
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
			sb.AppendFormat(GDECodeGenConstants.VariableFormat, type, name, "Custom");
			sb.Append("\n");
		}

		private static void AppendVariable(StringBuilder sb, string type, string name)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
            sb.AppendFormat(GDECodeGenConstants.VariableFormat, type, name, type.UppercaseFirst());
            sb.Append("\n");
        }

        private static void AppendListVariable(StringBuilder sb, string type, string name, int dimension)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));

			if (dimension == 1)
				sb.AppendFormat(GDECodeGenConstants.OneDListVariableFormat, type, name, type.UppercaseFirst());
			else
				sb.AppendFormat(GDECodeGenConstants.TwoDListVariableFormat, type, name, type.UppercaseFirst()+GDECodeGenConstants.TwoDListSuffix);

            sb.Append("\n");
        }

		private static void AppendCustomListVariable(StringBuilder sb, string type, string name, int dimension)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));

			if (dimension == 1)
				sb.AppendFormat(GDECodeGenConstants.OneDListVariableFormat, type, name, "Custom");
			else
				sb.AppendFormat(GDECodeGenConstants.TwoDListVariableFormat, type, name, "Custom"+GDECodeGenConstants.TwoDListSuffix);

			sb.Append("\n");
		}
		#endregion

        #region Gen Load Variable Methods
        private static void AppendLoadVariable(StringBuilder sb, string type, string name)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel4));
            sb.AppendFormat(GDECodeGenConstants.LoadVariableFormat, type, name, type.UppercaseFirst());
            sb.Append("\n");
        }

		private static void AppendLoadFromSavedVariable(StringBuilder sb, string type, string name)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel3));
			sb.AppendFormat(GDECodeGenConstants.LoadSavedVariableFormat, type, name, type.UppercaseFirst());
			sb.Append("\n");
		}

        private static void AppendLoadCustomVariable(StringBuilder sb, string type, string name)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel4));
            sb.AppendFormat(GDECodeGenConstants.LoadCustomVariableFormat, type, name);
            sb.Append("\n");
        }

		private static void AppendLoadFromSavedCustomVariable(StringBuilder sb, string type, string name)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel3));
			sb.AppendFormat(GDECodeGenConstants.LoadSavedCustomVariableFormat, type, name);
			sb.Append("\n");
		}

        private static void AppendLoadListVariable(StringBuilder sb, string type, string name, int dimension)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel4));

			if (dimension == 1)
	      		sb.AppendFormat(GDECodeGenConstants.LoadVariableListFormat, type, name);
			else
				sb.AppendFormat(GDECodeGenConstants.LoadVariableListFormat, type+GDECodeGenConstants.TwoDListSuffix, name);

            sb.Append("\n");
        }

		private static void AppendLoadSavedListVariable(StringBuilder sb, string type, string name, int dimension)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel3));

			if (dimension == 1)
				sb.AppendFormat(GDECodeGenConstants.LoadSavedVariableListFormat, type, name);
			else
				sb.AppendFormat(GDECodeGenConstants.LoadSavedVariableListFormat, type+GDECodeGenConstants.TwoDListSuffix, name);

			sb.Append("\n");
		}
        #endregion

        #region Gen Reset Methods
        private static void AppendResetAllMethod(StringBuilder sb, string schemaKey, Dictionary<string, object> schemaData)
        {
            bool shouldAppendSpace = false;

            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
            sb.Append(GDECodeGenConstants.ResetAllDeclaration);
            sb.Append("\n");

			// Reset all fields
            List<string> fields = GDEItemManager.SchemaFieldKeys(schemaKey, schemaData);
            foreach(string fieldName in fields)
            {
                if (shouldAppendSpace)
                    sb.Append("\n");

                sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel3));
                sb.AppendFormat(GDECodeGenConstants.ResetToDefaultFormat, fieldName);
                shouldAppendSpace = true;
            }

			if (shouldAppendSpace)
				sb.Append("\n");

			// Call reset on any custom types
			for(int dimension=0;  dimension<= 2;  dimension++)
			{
				fields = GDEItemManager.SchemaCustomFieldKeys(schemaKey, dimension);
				foreach(string fieldName in fields)
				{
					if (shouldAppendSpace)
					    sb.Append("\n");

					sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel3));

					if (dimension == 0)
						sb.AppendFormat(GDECodeGenConstants.CustomResetAllFormat, fieldName);
					else
						sb.AppendFormat(GDECodeGenConstants.CustomResetAllFormat, fieldName);

					shouldAppendSpace = true;
				}
			}

            sb.Append("\n");
            sb.Append("\n");

            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel3));
            sb.Append(GDECodeGenConstants.ResetAllEndMethod);
        }

        private static void AppendResetVariableMethod(StringBuilder sb, string name, string type)
        {
            sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));
            sb.AppendFormat(GDECodeGenConstants.ResetVariableDeclarationFormat, name, type);
            sb.Append("\n");
        }

		private static void AppendResetListVariableMethod(StringBuilder sb, string name, string type, int dimension)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));

			if (dimension == 1)
				sb.AppendFormat(GDECodeGenConstants.ResetListVariableDeclarationFormat, name, type);
			else
				sb.AppendFormat(GDECodeGenConstants.ResetListVariableDeclarationFormat, name, type+GDECodeGenConstants.TwoDListSuffix);

			sb.Append("\n");
		}

		private static void AppendResetCustomVariableMethod(StringBuilder sb, string name, string type, int dimension)
		{
			sb.Append("".PadLeft(GDECodeGenConstants.IndentLevel2));

			if (dimension == 0)
				sb.AppendFormat(GDECodeGenConstants.ResetCustomFormat, type, name);
			else if (dimension == 1)
				sb.AppendFormat(GDECodeGenConstants.ResetCustom1DListFormat, type, name);
			else
				sb.AppendFormat(GDECodeGenConstants.ResetCustom2DListFormat, type+GDECodeGenConstants.TwoDListSuffix, name);

			sb.Append("\n");
		}
        #endregion
    }
}
