using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindMissingScripts));
    }

    void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts In Scene"))
        {
            FindInScene();
        }
    }

    public static void FindInScene()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        foreach (GameObject g in go)
        {
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.Log("Missing script found in: " + FullPath(g), g);
                    count++;
                }
            }
        }
        Debug.Log("Found " + count + " missing scripts.");
    }

    static string FullPath(GameObject go)
    {
        return go.transform.parent == null ? go.name : FullPath(go.transform.parent.gameObject) + "/" + go.name;
    }
}
