
using UnityEngine;
using UnityEditor;

public class SlideBuilder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SlideBuilder))]
class SlideBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var slideBuilder = (SlideBuilder)target;
        if (slideBuilder == null)
            return;


    }
}
#endif
