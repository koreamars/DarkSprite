//using UnityEngine;
//using UnityEditor;
//using System;

//namespace SmoothMoves
//{
//    [InitializeOnLoad]
//    public class SmoothMovesStartup
//    {
//        private static readonly EditorApplication.HierarchyWindowItemCallback hiearchyItemCallback;
//        private static readonly EditorApplication.ProjectWindowItemCallback projectItemCallback;

//        private static Texture2D boneAnimationIcon;
//        private static Texture2D BoneAnimationIcon
//        {
//            get
//            {
//                if (SmoothMovesStartup.boneAnimationIcon == null)
//                {
//                    Texture2D texture = null;
//                    GUIHelper.LoadTexture(ref texture, "SmoothMovesIcon_BoneAnimation_16.png", 16, 16);

//                    SmoothMovesStartup.boneAnimationIcon = texture;
//                }
//                return SmoothMovesStartup.boneAnimationIcon;
//            }
//        }

//        private static Texture2D animationDataIcon;
//        private static Texture2D AnimationDataIcon
//        {
//            get
//            {
//                if (SmoothMovesStartup.animationDataIcon == null)
//                {
//                    Texture2D texture = null;
//                    GUIHelper.LoadTexture(ref texture, "SmoothMovesIcon_AnimationData_16.png", 16, 16);

//                    SmoothMovesStartup.animationDataIcon = texture;
//                }
//                return SmoothMovesStartup.animationDataIcon;
//            }
//        }

//        private static Texture2D textureAtlasIcon;
//        private static Texture2D TextureAtlasIcon
//        {
//            get
//            {
//                if (SmoothMovesStartup.textureAtlasIcon == null)
//                {
//                    Texture2D texture = null;
//                    GUIHelper.LoadTexture(ref texture, "SmoothMovesIcon_TextureAtlas_16.png", 16, 16);

//                    SmoothMovesStartup.textureAtlasIcon = texture;
//                }
//                return SmoothMovesStartup.textureAtlasIcon;
//            }
//        }

//        // constructor
//        static SmoothMovesStartup()
//        {
//            SmoothMovesStartup.hiearchyItemCallback = new EditorApplication.HierarchyWindowItemCallback(SmoothMovesStartup.DrawHierarchyIcon);
//            EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, SmoothMovesStartup.hiearchyItemCallback);

//            SmoothMovesStartup.projectItemCallback = new EditorApplication.ProjectWindowItemCallback(SmoothMovesStartup.DrawProjectIcon);
//            EditorApplication.projectWindowItemOnGUI = (EditorApplication.ProjectWindowItemCallback)Delegate.Combine(EditorApplication.projectWindowItemOnGUI, SmoothMovesStartup.projectItemCallback);
//        }

//        private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
//        {
//            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
//            if (gameObject != null && gameObject.GetComponent<BoneAnimation>() != null)
//            {
//                Rect rect = new Rect(selectionRect.x + selectionRect.width - 16f, selectionRect.y, 16f, 16f);
//                GUI.DrawTexture(rect, SmoothMovesStartup.BoneAnimationIcon);
//            }
//        }

//        private static void DrawProjectIcon(string guid, Rect selectionRect)
//        {
//            BoneAnimationData animationData = (BoneAnimationData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(BoneAnimationData));
//            if (animationData != null)
//            {
//                Rect rect = new Rect(selectionRect.x + selectionRect.width - 16f, selectionRect.y, 16f, 16f);
//                GUI.DrawTexture(rect, SmoothMovesStartup.AnimationDataIcon);
//            }
//            else
//            {
//                TextureAtlas textureAtlas = (TextureAtlas)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(TextureAtlas));
//                if (textureAtlas != null)
//                {
//                    Rect rect = new Rect(selectionRect.x + selectionRect.width - 16f, selectionRect.y, 16f, 16f);
//                    GUI.DrawTexture(rect, SmoothMovesStartup.TextureAtlasIcon);
//                }
//                else
//                {
//                    BoneAnimation boneAnimation = (BoneAnimation)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(BoneAnimation));
//                    if (boneAnimation != null)
//                    {
//                        Rect rect = new Rect(selectionRect.x + selectionRect.width - 16f, selectionRect.y, 16f, 16f);
//                        GUI.DrawTexture(rect, SmoothMovesStartup.BoneAnimationIcon);
//                    }
//                }
//            }
//        }
//    }
//}