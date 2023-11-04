
using UnityEngine;
using UnityEditor;

public class NewWindow : EditorWindow
{
    [MenuItem("Window/NewWindow")]
    static void OpenWindow()
    {
        NewWindow window = (NewWindow)GetWindow(typeof(NewWindow));
        window.minSize = new Vector2(300, 300);
        window.Show();
    }

    private void OnEnable()
    {

    }
}
