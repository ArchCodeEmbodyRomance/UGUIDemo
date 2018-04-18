using UnityEditor;

[CustomEditor(typeof(PicText), true)]
[CanEditMultipleObjects]
public class PicTextEditor : UnityEditor.UI.TextEditor
{
    protected SerializedProperty _textures;
    protected override void OnEnable()
    {
        base.OnEnable();
        _textures = serializedObject.FindProperty("Textures");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(_textures, true);
    }
}
