using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    static public class AnimationControlsWindow
    {
        public const float ANIMATION_CONTROLS_HEIGHT_LONG = 30.0f;
        public const float ANIMATION_CONTROLS_HEIGHT_SHORT = 50.0f;
        public const float ZOOM_MIN = 0.1f;
        public const float ZOOM_MAX = 10.0f;
        public const float LONG_WIDTH = 850.0f;

        static private Rect _areaRect;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static private bool UsingLongWidth { get { return _areaRect.width >= LONG_WIDTH; } }

        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public float Height
        {
            get
            {
                if (UsingLongWidth)
                    return ANIMATION_CONTROLS_HEIGHT_LONG;
                else
                    return ANIMATION_CONTROLS_HEIGHT_SHORT;
            }
        }

        static public void OnEnable()
        {
        }

        static public void OnGUI(bool playing)
        {
            GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

            if (ClipBrowserWindow.CurrentClip != null && !editor.ModalPopup)
            {
                bool newPlayingAnimationClip;

                if (!UsingLongWidth)
                {
                    GUILayout.BeginVertical();
                }

                GUILayout.BeginHorizontal();

                GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);

                if (!playing)
                {
                    if (GUILayout.Button(new GUIContent(Resources.buttonAdvanceFrameBackward, "Advance Frame Backward"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonAdvanceFrameBackward.width), GUILayout.Height(Resources.buttonAdvanceFrameBackward.height)))
                    {
                        TimelineWindow.AdvanceFrame(-1);
                    }
                }
                else
                {
                    GUILayout.Space(Resources.buttonAdvanceFrameBackward.width);
                }


                GUILayout.Space(2.0f);

                newPlayingAnimationClip = GUILayout.Toggle(TimelineWindow.IsPlaying, new GUIContent((TimelineWindow.IsPlaying ? Resources.buttonStop : Resources.buttonPlay), "Play or Stop Animation Preview"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonPlay.width), GUILayout.Height(Resources.buttonPlay.height));
                if (newPlayingAnimationClip != TimelineWindow.IsPlaying)
                {
                    if (newPlayingAnimationClip)
                    {
                        TimelineWindow.Play(
                                                            ((TimelineWindow.CurrentFrame > ClipBrowserWindow.CurrentClip.maxFrame)
                                                                &&
                                                              (ClipBrowserWindow.CurrentClip.wrapMode == WrapMode.Loop || ClipBrowserWindow.CurrentClip.wrapMode == WrapMode.PingPong))
                                                            ||
                                                            ((TimelineWindow.CurrentFrame >= ClipBrowserWindow.CurrentClip.maxFrame)
                                                                &&
                                                              (ClipBrowserWindow.CurrentClip.wrapMode == WrapMode.Once || ClipBrowserWindow.CurrentClip.wrapMode == WrapMode.ClampForever))
                                                            );
                    }
                    else
                    {
                        TimelineWindow.Stop();
                    }

                    TimelineWindow.ResetSelectedFrames();
                }

                GUILayout.Space(2.0f);

                if (!playing)
                {
                    if (GUILayout.Button(new GUIContent(Resources.buttonAdvanceFrameForward, "Advance Frame Forward"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonAdvanceFrameForward.width), GUILayout.Height(Resources.buttonAdvanceFrameForward.height)))
                    {
                        TimelineWindow.AdvanceFrame(1);
                    }
                }
                else
                {
                    GUILayout.Space(Resources.buttonAdvanceFrameForward.width);
                }

                GUILayout.Space(20.0f);

                GUILayout.Label("Time: " + TimelineWindow.RealTimeString, Style.normalLabelStyle, GUILayout.Width(100.0f));

                GUILayout.Space(20.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonCenter, "Center On Origin"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonCenter.width), GUILayout.Height(Resources.buttonCenter.height)))
                {
                    AnimationWindow.CenterOrigin();
                }

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonZoomOne, "Set Zoom to One"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonZoomOne.width), GUILayout.Height(Resources.buttonZoomOne.height)))
                {
                    AnimationWindow.Zoom = 1.0f;
                }

                GUILayout.Space(2.0f);

                if (!playing)
                {
                    if (GUILayout.Button(new GUIContent(Resources.buttonSettings, "Change Settings"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonSettings.width), GUILayout.Height(Resources.buttonSettings.height)))
                    {
                        TimelineWindow.ResetSelectedFrames();

                        SettingsWindow.ShowWindow();
                    }
                }

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonContrast, "Toggle Contrast"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonSettings.width), GUILayout.Height(Resources.buttonSettings.height)))
                {
                    SettingsWindow.AnimationContrastDark = !SettingsWindow.AnimationContrastDark;
                }

                //GUILayout.Space(2.0f);

                //if (!playing)
                //{
                //    if (GUILayout.Button(new GUIContent(Resources.buttonUpdateMesh, "Update Mesh In Scene"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonUpdateMesh.width), GUILayout.Height(Resources.buttonUpdateMesh.height)))
                //    {
                //        AnimationHelper.UpdateBoneAnimationsAndData(editor.boneAnimationData, false);

                //        EditorUtility.DisplayDialog("Success", "Updated meshes in scene", "OK");
                //    }
                //}

                GUILayout.Space(20.0f);

                if (!UsingLongWidth)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                GUILayout.Label("Zoom: ", Style.normalLabelStyle);

                float newZoom;
                newZoom = GUILayout.HorizontalSlider(AnimationWindow.Zoom, ZOOM_MIN, ZOOM_MAX, GUILayout.Width(100.0f));
                if (newZoom != AnimationWindow.Zoom)
                {
                    AnimationWindow.Zoom = newZoom;
                }

                GUILayout.Label("x " + EditorHelper.RoundFloat(AnimationWindow.Zoom, 2).ToString(), Style.normalLabelStyle, GUILayout.Width(50.0f));

                if (!playing)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Mesh Color: ", Style.normalLabelStyle);
                    Color newMeshColor = EditorGUILayout.ColorField(editor.boneAnimationData.meshColor, GUILayout.Width(100.0f));
                    if (newMeshColor != editor.boneAnimationData.meshColor)
                    {
                        editor.SetWillBeDirty();

                        editor.boneAnimationData.meshColor = newMeshColor;
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();


                GUILayout.EndHorizontal();

                if (!UsingLongWidth)
                {
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndArea();
        }
    }
}
