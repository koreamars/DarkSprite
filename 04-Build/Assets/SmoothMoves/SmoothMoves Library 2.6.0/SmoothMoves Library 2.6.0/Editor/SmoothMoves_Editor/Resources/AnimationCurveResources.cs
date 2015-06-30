using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    static public class AnimationCurveResources
    {
        //static private string basePath;

        static public Texture2D gizmoCurveNode;
        static public Texture2D gizmoCurveAddNode;
        static public Texture2D gizmoCurveNodeSelected;
        static public Texture2D gizmoCurveTangentHandle;
        static public Texture2D gizmoCurveTimeBackward;
        static public Texture2D gizmoCurveTimeForward;
        static public Texture2D gizmoCurveValueDown;
        static public Texture2D gizmoCurveValueUp;
		
		static public Texture2D curveEditorSmoothOff;
        static public Texture2D curveEditorSmoothOn;
        static public Texture2D curveEditorBrokenOff;
        static public Texture2D curveEditorBrokenOn;
        static public Texture2D curveEditorFlat;
		static public Texture2D curveEditorSideFreeOff;
        static public Texture2D curveEditorSideFreeOn;
        static public Texture2D curveEditorSideLinearOff;
        static public Texture2D curveEditorSideLinearOn;
        static public Texture2D curveEditorSideConstantOff;
        static public Texture2D curveEditorSideConstantOn;
        static public Texture2D curveEditorSetOne;
		static public Texture2D curveEditorSetZero;
        static public Texture2D curveEditorDeleteNode;

		static public Texture2D degree;

        //static public void OnEnable()
        //{
        //    basePath = PlayerPrefs.GetString("SmoothMoves_AssetsBasePath", Resources.BASE_PATH);
        //    PlayerPrefs.SetString("SmoothMoves_AssetsBasePath", basePath);
        //}

        static public void LoadTextures()
        {
            GUIHelper.LoadTexture(ref gizmoCurveNode, "GizmoCurveNode.png", 10, 10);
            GUIHelper.LoadTexture(ref gizmoCurveAddNode, "GizmoCurveAddNode.png", 15, 15);
            GUIHelper.LoadTexture(ref gizmoCurveNodeSelected, "GizmoCurveNodeSelected.png", 10, 10);
            GUIHelper.LoadTexture(ref gizmoCurveTangentHandle, "GizmoCurveTangentHandle.png", 100, 10);
            GUIHelper.LoadTexture(ref gizmoCurveTimeBackward, "GizmoCurveTimeBackward.png", 10, 20);
            GUIHelper.LoadTexture(ref gizmoCurveTimeForward, "GizmoCurveTimeForward.png", 10, 20);
            GUIHelper.LoadTexture(ref gizmoCurveValueDown, "GizmoCurveValueDown.png", 5, 50);
            GUIHelper.LoadTexture(ref gizmoCurveValueUp, "GizmoCurveValueUp.png", 5, 50);

            GUIHelper.LoadTexture(ref curveEditorSmoothOff, "CurveEditorSmoothOff.png", 49, 49);
            GUIHelper.LoadTexture(ref curveEditorSmoothOn, "CurveEditorSmoothOn.png", 49, 49);
            GUIHelper.LoadTexture(ref curveEditorBrokenOff, "CurveEditorBrokenOff.png", 49, 49);
            GUIHelper.LoadTexture(ref curveEditorBrokenOn, "CurveEditorBrokenOn.png", 49, 49);
            GUIHelper.LoadTexture(ref curveEditorFlat, "CurveEditorFlat.png", 100, 26);
            GUIHelper.LoadTexture(ref curveEditorSideFreeOff, "CurveEditorSideFreeOff.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSideFreeOn, "CurveEditorSideFreeOn.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSideLinearOff, "CurveEditorSideLinearOff.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSideLinearOn, "CurveEditorSideLinearOn.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSideConstantOff, "CurveEditorSideConstantOff.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSideConstantOn, "CurveEditorSideConstantOn.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSetOne, "CurveEditorSetOne.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorSetZero, "CurveEditorSetZero.png", 34, 34);
            GUIHelper.LoadTexture(ref curveEditorDeleteNode, "CurveEditorDeleteNode.png", 34, 34);

            GUIHelper.LoadTexture(ref degree, "Degree.png", 8, 9);
			
            //AssetDatabase.SaveAssets();
        }
		
        //static private void LoadTexture(ref Texture2D texture, string path)
        //{
        //    if (texture == null)
        //    {
        //        VerifyTextureFormat(path);
        //        texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        //    }			
        //}

        //static private void VerifyTextureFormat(string texturePath)
        //{
        //    TextureImporter importer;
        //    importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
        //    if (importer.textureFormat != TextureImporterFormat.ARGB32 || importer.npotScale != TextureImporterNPOTScale.None || importer.mipmapEnabled)
        //    {
        //        importer.textureFormat = TextureImporterFormat.ARGB32;
        //        importer.npotScale = TextureImporterNPOTScale.None;
        //        importer.mipmapEnabled = false;
        //        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
        //    }
        //}
    }
}
