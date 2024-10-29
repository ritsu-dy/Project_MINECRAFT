using UnityEditor;
using UnityEngine;

public class ExcelScriptableObjectCreator : EditorWindow
{
    private Object selectedExcelFile;

    [MenuItem("Window/Excel ScriptableObject Creator")]
    public static void ShowWindow()
    {
        GetWindow<ExcelScriptableObjectCreator>("Excel ScriptableObject Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Excel ScriptableObject Creator", EditorStyles.boldLabel);

        selectedExcelFile = EditorGUILayout.ObjectField("Select Excel File", selectedExcelFile, typeof(Object), false);

        if (GUILayout.Button("Create ScriptableObject"))
        {
            if (selectedExcelFile != null)
            {
                string filePath = AssetDatabase.GetAssetPath(selectedExcelFile);
                if (filePath.EndsWith(".xlsx"))
                {
                    ExcelImporterEditor.CreateClassFileFromExcel(null); // MenuCommand �Ű������� null�� ��ü
                }
                else
                {
                    Debug.LogError("���õ� ������ ���� ������ �ƴմϴ�.");
                }
            }
            else
            {
                Debug.LogError("���� ������ �����ϼ���.");
            }
        }
    }
}
