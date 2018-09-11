using UnityEditor;
using UnityEngine;

namespace SmartData.Graph
{
    public static class FindInGraphButton
    {
        [MenuItem("GameObject/Smart Graph/Find this", false, 0)]
        static void FindEvents()
        {
            SmartGraphWindow window = EditorWindow.GetWindow<SmartGraphWindow>();
            if(window != null)
            {
                window.OverrideSelection(Selection.activeInstanceID);
            }
        }

    }

}
