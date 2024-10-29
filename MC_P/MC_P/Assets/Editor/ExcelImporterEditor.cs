using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Linq;
using System.Collections;

public class ExcelImporterEditor : MonoBehaviour
{
    [MenuItem("Assets/Create Class from Excel", false, 10)]
    public static void CreateClassFileFromExcel(MenuCommand command)
    {
        string filePath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (!filePath.EndsWith(".xlsx"))
        {
            Debug.LogError("엑셀 파일이 아닙니다.");
            return;
        }

        string tableName = Path.GetFileNameWithoutExtension(filePath);
        XSSFWorkbook workbook;
        using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            workbook = new XSSFWorkbook(file);
        }
        ISheet sheet = workbook.GetSheetAt(0);
        IRow headerRow = sheet.GetRow(0);
        IRow typeRow = sheet.GetRow(1);

        List<int> ignoreColumns = new List<int>();
        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            if (headerRow.GetCell(i) != null && headerRow.GetCell(i).ToString().StartsWith("#"))
            {
                ignoreColumns.Add(i);
            }
        }

        List<string> fieldNames = new List<string>();
        List<string> fieldTypes = new List<string>();

        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            if (!ignoreColumns.Contains(i))
            {
                fieldNames.Add(headerRow.GetCell(i).ToString());
                fieldTypes.Add(typeRow.GetCell(i) != null ? typeRow.GetCell(i).ToString() : "string");
            }
        }

        string classFilePath = Path.Combine(Application.dataPath, "04_TableDatas", tableName + "Data.cs");
        string soClassFilePath = Path.Combine(Application.dataPath, "04_TableDatas", tableName + ".cs");

        // 이미 존재하는지 확인하여 업데이트할지, 생성할지 결정
        if (File.Exists(classFilePath))
        {
            Debug.Log($"{tableName}Data.cs 파일이 존재합니다. 업데이트합니다.");
        }
        else
        {
            string classCode = GenerateClassCode(tableName, fieldNames, fieldTypes);
            File.WriteAllText(classFilePath, classCode);
            Debug.Log($"{tableName}Data.cs 파일이 생성되었습니다.");
        }

        if (File.Exists(soClassFilePath))
        {
            Debug.Log($"{tableName}.cs 파일이 존재합니다. 업데이트합니다.");
        }
        else
        {
            string soClassCode = GenerateScriptableObjectClassCode(tableName, fieldNames);
            File.WriteAllText(soClassFilePath, soClassCode);
            Debug.Log($"{tableName}.cs 파일이 생성되었습니다.");
        }

        AssetDatabase.ImportAsset(classFilePath);
        AssetDatabase.ImportAsset(soClassFilePath);
    }

    [MenuItem("Assets/Create ScriptableObject from Excel", false, 11)]
    public static void CreateScriptableObjectFromExcel(MenuCommand command)
    {
        string filePath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (!filePath.EndsWith(".xlsx"))
        {
            Debug.LogError("엑셀 파일이 아닙니다.");
            return;
        }

        string tableName = Path.GetFileNameWithoutExtension(filePath);
        XSSFWorkbook workbook;
        using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            workbook = new XSSFWorkbook(file);
        }
        ISheet sheet = workbook.GetSheetAt(0);
        IRow headerRow = sheet.GetRow(0);
        IRow typeRow = sheet.GetRow(1);

        List<int> ignoreColumns = new List<int>();
        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            if (headerRow.GetCell(i) != null && headerRow.GetCell(i).ToString().StartsWith("#"))
            {
                ignoreColumns.Add(i);
            }
        }

        // 스크립터블 오브젝트 생성 및 업데이트
        UpdateScriptableObject(tableName, sheet, ignoreColumns, typeRow);
    }

    private static string FormatFieldName(string fieldName)
    {
        string[] parts = fieldName.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
        }
        return string.Join("_", parts);
    }

    private static string GenerateClassCode(string className, List<string> fieldNames, List<string> fieldTypes)
    {
        string code = "using System;\nusing UnityEngine;\n\n";
        code += $"[Serializable]\n";
        code += $"public class {className}Data\n{{\n";

        for (int i = 0; i < fieldNames.Count; i++)
        {
            code += $"    [SerializeField] private {fieldTypes[i]} {fieldNames[i]};\n";
            code += $"    public {fieldTypes[i]} {FormatFieldName(fieldNames[i])} {{ get => {fieldNames[i]}; set => {fieldNames[i]} = value; }}\n";
        }

        code += "}\n";
        return code;
    }

    private static string GenerateScriptableObjectClassCode(string className, List<string> fieldNames)
    {
        string code = "using System;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
        code += $"[CreateAssetMenu(fileName = \"{className}\", menuName = \"ScriptableObjects/{className}\")]\n";
        code += $"public class {className} : ScriptableObject\n{{\n";
        code += $"    [SerializeField] private List<{className}Data> _dataList;\n";
        code += $"    public List<{className}Data> DataList => _dataList;\n";
        code += $"    public void InitializeData(List<{className}Data> data)\n{{\n";
        code += $"        _dataList = data;\n";
        code += $"    }}\n";
        code += "}\n";
        return code;
    }

    private static void UpdateScriptableObject(string tableName, ISheet sheet, List<int> ignoreColumns, IRow typeRow)
    {
        string assetPath = $"Assets/06_ScriptableObjects/{tableName}.asset";
        var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

        if (scriptableObject == null)
        {
            scriptableObject = ScriptableObject.CreateInstance(tableName);
            AssetDatabase.CreateAsset(scriptableObject, assetPath);
            Debug.Log($"'{tableName}' 스크립터블 오브젝트가 생성되었습니다.");
        }
        else
        {
            Debug.Log($"'{tableName}' 스크립터블 오브젝트가 존재합니다. 업데이트합니다.");
        }

        Type dataType = GetTypeByName($"{tableName}Data");
        if (dataType == null)
        {
            Debug.LogError($"'{tableName}Data' 클래스를 찾을 수 없습니다.");
            return;
        }

        var tempDataList = new List<object>();
        int lastRowNum = sheet.LastRowNum;

        for (int rowIndex = 2; rowIndex <= lastRowNum; rowIndex++) // 데이터는 3행부터 시작
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null || (row.GetCell(0) != null && row.GetCell(0).ToString().StartsWith("#")))
                continue; // #로 시작하는 행은 무시

            var data = Activator.CreateInstance(dataType);

            for (int colIndex = 0; colIndex < row.LastCellNum; colIndex++)
            {
                if (!ignoreColumns.Contains(colIndex))
                {
                    var property = data.GetType().GetProperty(FormatFieldName(sheet.GetRow(0).GetCell(colIndex).ToString()));
                    if (property != null)
                    {
                        string fieldType = typeRow.GetCell(colIndex) != null ? typeRow.GetCell(colIndex).ToString() : "string";
                        object value = GetCellValue(row.GetCell(colIndex), fieldType);
                        if (value != null && property.PropertyType.IsAssignableFrom(value.GetType()))
                        {
                            property.SetValue(data, value);
                        }
                    }
                }
            }
            tempDataList.Add(data);
        }

        var dataList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType));
        foreach (var item in tempDataList)
        {
            dataList.Add(item);
        }

        var soField = scriptableObject.GetType().GetField("_dataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        soField.SetValue(scriptableObject, dataList);

        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();
        Debug.Log($"'{tableName}' 스크립터블 오브젝트가 업데이트되었습니다.");
    }

    private static object GetCellValue(ICell cell, string fieldType)
    {
        if (cell == null)
            return null;

        switch (fieldType)
        {
            case "string":
                return cell.ToString();
            case "int":
                return (int)cell.NumericCellValue;
            case "float":
                return (float)cell.NumericCellValue;
            case "double":
                return cell.NumericCellValue;
            default:
                return cell.ToString();
        }
    }

    private static Type GetTypeByName(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(a => a.GetTypes())
               .FirstOrDefault(t => t.Name == typeName);
    }
}
