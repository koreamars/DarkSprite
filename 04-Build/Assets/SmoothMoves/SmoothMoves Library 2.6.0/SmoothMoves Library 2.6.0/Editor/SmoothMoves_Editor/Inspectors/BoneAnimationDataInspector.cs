using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    [CustomEditor(typeof(BoneAnimationData))]
    public class BoneAnimationDataInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginVertical();

            if (GUILayout.Button("Open Animation Editor", GUILayout.Height(40.0f)))
            {
                BoneAnimationDataEditorWindow.ShowEditorUtility();
                BoneAnimationDataEditorWindow.Instance.title = "SmoothMoves Animation Editor v" + EditorHelper.VERSION + " - " + target.name;
                Selection.activeGameObject = null;
            }

            GUILayout.Space(10.0f);

            BoneAnimationData animationData = (BoneAnimationData)target;

            if (animationData != null)
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
                GUILayout.Label("  Bone Count ", Style.selectedInformationValueStyle, GUILayout.Width(100.0f), GUILayout.Height(15.0f));
                GUILayout.Label(": " + animationData.boneDataList.Count.ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Clip Count ", Style.selectedInformationValueStyle, GUILayout.Width(100.0f), GUILayout.Height(15.0f));
                GUILayout.Label(": " + animationData.animationClips.Count.ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Data Version ", Style.selectedInformationValueStyle, GUILayout.Width(100.0f), GUILayout.Height(15.0f));
                GUILayout.Label(": " + animationData.dataVersion.ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.Space(10.0f);

                GUILayout.EndVertical();

                GUILayout.Space(5.0f);

                GUILayout.EndHorizontal();

                GUILayout.Space(10.0f);

                GUILayout.BeginHorizontal();
                float newMeshScale = EditorGUILayout.FloatField(" Mesh Import Scale:", animationData.importScale);
                if (newMeshScale != animationData.importScale)
                {
                    animationData.importScale = newMeshScale;
                    animationData.GenerateBuildID();
                    EditorUtility.SetDirty(animationData);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10.0f);
            }

            GUILayout.EndVertical();
        }

        public void OnEnable()
        {
        }
    }
}
