using UnityEngine;
using UnityEditor;
using System;

namespace SmoothMoves
{
    static public class Resources
    {
        //public const string BASE_PATH = "Assets/Editor/SmoothMoves/Resources/";
        //public const string BASE_PATH_2 = "Assets/SmoothMoves/Editor/SmoothMoves/Resources/";

        //static private string basePath;

        static public Texture2D gizmoRotationRingTexture;
        static public Texture2D gizmoMoveArrowTexture;
        static public Texture2D gizmoScaleBarTexture;
        static public Texture2D gizmoMoveFreeBoxTexture;
		static public Texture2D gizmoDepthUpTexture;
		static public Texture2D gizmoDepthDownTexture;

        static public Texture2D pivotButton;
		static public Texture2D arrow;

        static public Texture2D buttonDown;
        static public Texture2D buttonUp;
        static public Texture2D buttonPlay;
        static public Texture2D buttonStop;
        static public Texture2D buttonAdvanceFrameForward;
        static public Texture2D buttonAdvanceFrameBackward;
        static public Texture2D buttonCenter;
        static public Texture2D buttonZoomOne;
        static public Texture2D buttonSettings;
        static public Texture2D buttonAddLarge;
        static public Texture2D buttonAdd;
        static public Texture2D buttonSubtract;
        static public Texture2D buttonUpdateMesh;
        static public Texture2D buttonContrast;
        static public Texture2D buttonGridOff;
        static public Texture2D buttonGridOn;
        static public Texture2D buttonBoundsOff;
        static public Texture2D buttonBoundsOn;

        static public Texture2D buttonEmptyOff;
        static public Texture2D buttonEmptyOn;

        static public Texture2D buttonArrowBackwardSmall;
        static public Texture2D buttonArrowForwardSmall;
        static public Texture2D buttonDoubleArrowBackwardSmall;
        static public Texture2D buttonDoubleArrowForwardSmall;
        static public Texture2D buttonRemoveBlankKeyframes;

        static public Texture2D buttonPivotOff;
        static public Texture2D buttonPivotOn;

        static public Texture2D buttonLockPivotDefaultOff;
        static public Texture2D buttonLockPivotDefaultOn;

        static public Texture2D warning;

        static public Texture2D eyeOpen;
        static public Texture2D eyeClosed;
        static public Texture2D boneInactive;

        static public Texture2D localScaleButtonOff;
        static public Texture2D localScaleButtonOn;
        static public Texture2D imageScaleButtonOff;
        static public Texture2D imageScaleButtonOn;

        static public Texture2D moveSelectedFramesForward_Off;
        static public Texture2D moveSelectedFramesForward_On;
        static public Texture2D moveSelectedFramesBackward_Off;
        static public Texture2D moveSelectedFramesBackward_On;

        //static public string BasePath
        //{
        //    get
        //    {
        //        basePath = PlayerPrefs.GetString("SmoothMoves_AssetsBasePath", BASE_PATH);

        //        try
        //        {
        //            Texture texTemp = (Texture2D)AssetDatabase.LoadAssetAtPath(basePath + "GizmoRotateRing.png", typeof(Texture2D));
        //            if (texTemp == null)
        //                basePath = BASE_PATH_2;
        //        }
        //        catch
        //        {
        //            basePath = BASE_PATH_2;
        //        }

        //        return basePath;
        //    }
        //}

        //static public void OnEnable()
        //{
        //    PlayerPrefs.SetString("SmoothMoves_AssetsBasePath", BasePath);
        //}

        static public void LoadTextures()
        {
            GUIHelper.LoadTexture(ref gizmoRotationRingTexture, "GizmoRotateRing.png", 128, 128);
            GUIHelper.LoadTexture(ref gizmoMoveArrowTexture, "GizmoMoveArrow.png", 18, 200);
            GUIHelper.LoadTexture(ref gizmoScaleBarTexture, "GizmoScaleBar.png", 18, 200);
            GUIHelper.LoadTexture(ref gizmoMoveFreeBoxTexture, "GizmoMoveFreeBox.png", 82, 82);
            GUIHelper.LoadTexture(ref gizmoDepthUpTexture, "GizmoDepthUp.png", 22, 11);
            GUIHelper.LoadTexture(ref gizmoDepthDownTexture, "GizmoDepthDown.png", 22, 11);

            GUIHelper.LoadTexture(ref pivotButton, "PivotButton.png", 64, 64);
            GUIHelper.LoadTexture(ref arrow, "Arrow.png", 32, 32);

            GUIHelper.LoadTexture(ref buttonDown, "ButtonDown.png", 40, 25);
            GUIHelper.LoadTexture(ref buttonUp, "ButtonUp.png", 40, 25);
            GUIHelper.LoadTexture(ref buttonPlay, "ButtonPlay.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonStop, "ButtonStop.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonAdvanceFrameForward, "ButtonAdvanceFrameForward.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonAdvanceFrameBackward, "ButtonAdvanceFrameBackward.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonCenter, "ButtonCenter.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonZoomOne, "ButtonZoomOne.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonSettings, "ButtonSettings.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonAddLarge, "ButtonAddLarge.png", 40, 25);
            GUIHelper.LoadTexture(ref buttonAdd, "ButtonAdd.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonSubtract, "ButtonSubtract.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonUpdateMesh, "ButtonUpdateMesh.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonContrast, "ButtonContrast.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonGridOff, "ButtonGridOff.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonGridOn, "ButtonGridOn.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonBoundsOff, "ButtonBoundsOff.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonBoundsOn, "ButtonBoundsOn.png", 23, 23);

            GUIHelper.LoadTexture(ref buttonEmptyOff, "ButtonEmptyOff.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonEmptyOn, "ButtonEmptyOn.png", 18, 18);

            GUIHelper.LoadTexture(ref buttonArrowBackwardSmall, "ButtonArrowBackwardSmall.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonArrowForwardSmall, "ButtonArrowForwardSmall.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonDoubleArrowBackwardSmall, "ButtonDoubleArrowBackwardSmall.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonDoubleArrowForwardSmall, "ButtonDoubleArrowForwardSmall.png", 18, 18);
            GUIHelper.LoadTexture(ref buttonRemoveBlankKeyframes, "ButtonRemoveBlankKeyframes.png", 18, 18);

            GUIHelper.LoadTexture(ref buttonPivotOff, "ButtonPivotOff.png", 23, 23);
            GUIHelper.LoadTexture(ref buttonPivotOn, "ButtonPivotOn.png", 23, 23);

            GUIHelper.LoadTexture(ref buttonLockPivotDefaultOff, "ButtonLockPivotDefaultOff.png", 51, 52);
            GUIHelper.LoadTexture(ref buttonLockPivotDefaultOn, "ButtonLockPivotDefaultOn.png", 51, 52);

            GUIHelper.LoadTexture(ref warning, "Warning.png", 16, 16);

            GUIHelper.LoadTexture(ref eyeOpen, "EyeOpen.png", 20, 20);
            GUIHelper.LoadTexture(ref eyeClosed, "EyeClosed.png", 20, 20);
            GUIHelper.LoadTexture(ref boneInactive, "BoneInactive.png", 20, 20);

            GUIHelper.LoadTexture(ref localScaleButtonOff, "LocalScaleButtonOff.png", 73, 16);
            GUIHelper.LoadTexture(ref localScaleButtonOn, "LocalScaleButtonOn.png", 73, 16);
            GUIHelper.LoadTexture(ref imageScaleButtonOff, "ImageScaleButtonOff.png", 73, 16);
            GUIHelper.LoadTexture(ref imageScaleButtonOn, "ImageScaleButtonOn.png", 73, 16);

            GUIHelper.LoadTexture(ref moveSelectedFramesBackward_Off, "MoveSelectedFramesBackward_Off.png", 30, 45);
            GUIHelper.LoadTexture(ref moveSelectedFramesBackward_On, "MoveSelectedFramesBackward_On.png", 30, 45);
            GUIHelper.LoadTexture(ref moveSelectedFramesForward_Off, "MoveSelectedFramesForward_Off.png", 30, 45);
            GUIHelper.LoadTexture(ref moveSelectedFramesForward_On, "MoveSelectedFramesForward_On.png", 30, 45);
        }
    }
}
