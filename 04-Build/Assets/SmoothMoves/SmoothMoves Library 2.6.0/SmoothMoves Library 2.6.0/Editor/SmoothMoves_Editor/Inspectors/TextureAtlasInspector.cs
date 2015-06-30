using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    [CustomEditor(typeof(TextureAtlas))]
    public class TextureAtlasInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginVertical();

            if (GUILayout.Button("Open Atlas Editor", GUILayout.Height(40.0f)))
            {
                TextureAtlasEditorWindow.ShowEditor();
                TextureAtlasEditorWindow.Instance.SetTextureAtlasAsset(Selection.activeObject);
            }

            GUILayout.Space(10.0f);

            TextureAtlas atlas = (TextureAtlas)target;

            if (atlas != null)
            {
                Style.Reset();

                GUILayout.BeginHorizontal();

                GUILayout.Space(5.0f);

                GUILayout.BeginVertical(Style.windowRectBackgroundStyle);

                GUILayout.Space(10.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Name ", Style.selectedInformationValueStyle, GUILayout.Width(100.0f), GUILayout.Height(15.0f));
                GUILayout.Label(": " + target.name, Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Texture Count ", Style.selectedInformationValueStyle, GUILayout.Width(100.0f), GUILayout.Height(15.0f));
                GUILayout.Label(": " + atlas.textureGUIDs.Count.ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Data Version ", Style.selectedInformationValueStyle, GUILayout.Width(100.0f), GUILayout.Height(15.0f));
                GUILayout.Label(": " + atlas.dataVersion.ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.Space(10.0f);

                GUILayout.EndVertical();

                GUILayout.Space(5.0f);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        public void OnEnable()
        {
            if (Selection.activeObject.GetType() == typeof(TextureAtlas))
            {
                if (TextureAtlasEditorWindow.Instance != null)
                    TextureAtlasEditorWindow.Instance.SetTextureAtlasAsset(Selection.activeObject);
            }
        }

    }
}
