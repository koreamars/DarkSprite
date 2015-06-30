using UnityEditor;
using UnityEngine;

namespace SmoothMoves
{
    static public class Menus
    {
        // Tools
        [MenuItem("Tools/SmoothMoves/Tools/Welcome Screen")]
        static public void ShowWelcomeScreen()
        {
            WelcomeScreen.ShowEditor();
        }

        // Docs
        [MenuItem("Tools/SmoothMoves/Documentation/User Manual")]
        static public void GotoUserManual()
        {
            Application.OpenURL("http://www.echo17.com/support/smoothmoves_user_manual_v2.pdf");
        }

        // Docs
        [MenuItem("Tools/SmoothMoves/Documentation/v2 Upgrade Guide")]
        static public void GotoV2UpgradeGuide()
        {
            Application.OpenURL("http://www.echo17.com/support/smoothmoves_v2_upgrade_guide.pdf");
        }

        // Docs
        [MenuItem("Tools/SmoothMoves/Documentation/Video Tutorials")]
        static public void GotoVideoTutorials()
        {
            Application.OpenURL("http://www.youtube.com/user/echo17software/videos?view=1");
        }

        // GameObjects
        [MenuItem("Tools/SmoothMoves/Create GameObject/Bone Animation")]
        [MenuItem("GameObject/Create Other/SmoothMoves/Bone Animation")]
        static public void CreateBoneAnimation()
        {
            AnimationHelper.CreateBoneAnimation();
        }

        [MenuItem("Tools/SmoothMoves/Create GameObject/Sprite")]
        [MenuItem("GameObject/Create Other/SmoothMoves/Sprite")]
        static public void CreateSprite()
        {
            GameObject go;
            Sprite s;

            go = new GameObject("Sprite");
            s = (Sprite)go.AddComponent(typeof(Sprite));
            s.useDefaultPivot = true;
            s.Initialize();

            Selection.activeGameObject = go;
        }


        // Assets
        [MenuItem("Tools/SmoothMoves/Create Asset/Texture Atlas Data")]
        [MenuItem("Assets/Create/SmoothMoves/Texture Atlas Data")]
        static public void CreateTextureAtlas()
        {
            EditorHelper.CreateAsset<TextureAtlas>("New Atlas");
        }

        [MenuItem("Tools/SmoothMoves/Create Asset/Bone Animation Data")]
        [MenuItem("Assets/Create/SmoothMoves/Bone Animation Data")]
        static public void CreateBoneAnimationData()
        {
            EditorHelper.CreateAsset<BoneAnimationData>("New Bone Animation");
        }
    }
}
