using UnityEngine;
using UnityEditor;
using SmoothMoves;
using System;
using System.IO;
using System.Reflection;

namespace SmoothMoves
{
    public class ControlPanel : EditorWindow
    {
        static public bool AutoUpdate
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_ControlPanel_AutoUpdate", 1) == 1);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_ControlPanel_AutoUpdate", (value ? 1 : 0));
            }
        }

        static public void LoadTexture(ref Texture2D texture, string resourceName, int width, int height)
        {
            if (texture == null)
            {
                texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

                Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SmoothMoves.EmbeddedResources." + resourceName);

                if (myStream != null)
                {
                    texture.LoadImage(ReadToEnd(myStream));
                    myStream.Close();
                }
                else
                {
                    Debug.LogError("Missing Dll resource: " + resourceName);
                }
            }
        }

        static public byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        static private ControlPanel _instance;
        
        private bool _updating = false;

        private Texture2D _Grey2Texture;
        private Texture2D _Grey7Texture;
        private GUIStyle _windowRectBackgroundStyle;
        private GUIStyle _windowRectBackgroundStyleLight;

        private Texture2D _autoBuild_on;
        private Texture2D _autoBuild_off;
        private Texture2D _logConsole_on;
        private Texture2D _logConsole_off;
        private Texture2D _forceBuild;

        private GUIStyle _noBorderButtonStyle;
        private GUIStyle _wrapLabelStyle;
        private GUIStyle _importantStyle;
        private GUIStyle _importantStyle2;

        private bool _runUpdateInAllScenes = false;

        static public ControlPanel Instance
        {
            get
            {
                return _instance;
            }
        }

        static public ControlPanel ShowEditor()
        {
            if (_instance == null)
            {
                _instance = (ControlPanel)EditorWindow.GetWindow(typeof(ControlPanel), false, "SmoothMoves");
            }

            _instance.ShowUtility();

            return _instance;
        }

        void OnEnable()
        {
            EditorApplication.playmodeStateChanged += OnEditorPlaymodeStateChanged;
        }

        void OnDestroy()
        {
            _instance = null;

            EditorApplication.playmodeStateChanged -= OnEditorPlaymodeStateChanged;
        }

        void OnEditorPlaymodeStateChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!EditorApplication.isPlaying)
                {
                    if (AutoUpdate)
                    {
                        if (!_updating)
                        {
                            _updating = true;

                            Repaint();

                            BuildHelper.UpdateBoneAnimationsAndDataInCurrentScene(false);

                            _updating = false;
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (_runUpdateInAllScenes)
            {
                _runUpdateInAllScenes = false;

                BuildHelper.UpdateBoneAnimationsAndDataInAllScenes();
            }
        }

        void OnGUI()
        {
            LoadStyles();
            LoadTextures();

            if (_updating)
            {
                GUILayout.Label("Auto Updating...");
            }

            GUILayout.Space(10.0f);

            GUIContent forceBuildButton = new GUIContent(_forceBuild);
            GUIContent autoBuildButton_on = new GUIContent(_autoBuild_on);
            GUIContent autoBuildButton_off = new GUIContent(_autoBuild_off);
            GUIContent logConsole_on = new GUIContent(_logConsole_on);
            GUIContent logConsole_off = new GUIContent(_logConsole_off);

            GUILayout.BeginVertical(_windowRectBackgroundStyleLight);

            GUILayout.Label("Control Panel");

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.BeginVertical(_windowRectBackgroundStyle);

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(forceBuildButton, _noBorderButtonStyle, GUILayout.Width(_forceBuild.width), GUILayout.Height(_forceBuild.height)))
            {
                if (!BuildHelper.RunningUpdate)
                    _runUpdateInAllScenes = true;
            }
            GUILayout.Space(3.0f);
            GUILayout.Label("Force-build all data, prefabs, and scene BoneAnimations. This will open each scene to update the BoneAnimations within.", _wrapLabelStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(AutoUpdate ? autoBuildButton_on : autoBuildButton_off, _noBorderButtonStyle, GUILayout.Width(_autoBuild_on.width), GUILayout.Height(_autoBuild_on.height)))
            {
                AutoUpdate = !AutoUpdate;
            }
            GUILayout.Space(10.0f);
            if (AutoUpdate)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Auto-build is ON. You don't need to rebuild your animations before playing the scene.", _wrapLabelStyle);
                GUILayout.Space(3.0f);
                GUILayout.Label("NOTE: This control panel must be open for Auto-build to work.", _importantStyle);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("Auto-build is OFF. You will need to rebuild your animations before playing the scene in order to see any changes made.", _wrapLabelStyle);
            }
            GUILayout.Space(10.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(BuildHelper.LogUpdates ? logConsole_on : logConsole_off, _noBorderButtonStyle, GUILayout.Width(_logConsole_on.width), GUILayout.Height(_logConsole_on.height)))
            {
                BuildHelper.LogUpdates = !BuildHelper.LogUpdates;
            }
            GUILayout.Space(10.0f);
            if (BuildHelper.LogUpdates)
                GUILayout.Label("Logging updates to the console is ON.", _wrapLabelStyle);
            else
                GUILayout.Label("Logging updates to the console is OFF.", _wrapLabelStyle);
            GUILayout.Space(10.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.EndVertical();

            GUILayout.Space(10.0f);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(10.0f);

            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(10.0f);
            GUILayout.BeginVertical(_windowRectBackgroundStyle);
            GUILayout.Space(5.0f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            GUILayout.Label("NOTE: Auto-build recreates all animation data and prefabs, " + 
                                "but only the instantiated BoneAnimations in the current scene will be rebuilt. " + 
                                "Be sure to run Force-build to update all scenes before publishing to a platform.", _importantStyle);
            GUILayout.Space(5.0f);
            GUILayout.EndHorizontal();
            GUILayout.Space(5.0f);
            GUILayout.EndVertical();
            GUILayout.Space(10.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            GUILayout.EndVertical();
        }

        private void LoadStyles()
        {
            _noBorderButtonStyle = new GUIStyle();
            _noBorderButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            _noBorderButtonStyle.padding = new RectOffset(0, 0, 0, 0);

            _wrapLabelStyle = new GUIStyle();
            _wrapLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            _wrapLabelStyle.alignment = TextAnchor.MiddleLeft;
            _wrapLabelStyle.wordWrap = true;

            _importantStyle = new GUIStyle();
            _importantStyle.normal.textColor = new Color(0.99f, 0.87f, 0.64f);
            _importantStyle.alignment = TextAnchor.MiddleLeft;
            _importantStyle.wordWrap = true;

            _importantStyle2 = new GUIStyle();
            _importantStyle2.normal.textColor = new Color(0.92f, 0.77f, 1.00f);
            _importantStyle2.alignment = TextAnchor.MiddleLeft;
            _importantStyle2.wordWrap = true;

            SetTexture(ref _Grey2Texture, new Color(0.2f, 0.2f, 0.2f));
            SetTexture(ref _Grey7Texture, new Color(0.7f, 0.7f, 0.7f));

            SetStyle(ref _windowRectBackgroundStyle, ref _Grey2Texture, new Color(1.0f, 1.0f, 1.0f), 10, TextAnchor.MiddleCenter, true, 0);
            SetStyle(ref _windowRectBackgroundStyleLight, ref _Grey7Texture, new Color(0.2f, 0.2f, 0.2f), 10, TextAnchor.MiddleCenter, true, 0);
        }

        private void LoadTextures()
        {
            LoadTexture(ref _autoBuild_on, "AutoBuild_on.png", 64, 64);
            LoadTexture(ref _autoBuild_off, "AutoBuild_off.png", 64, 64);
            LoadTexture(ref _logConsole_on, "LogConsole_on.png", 64, 64);
            LoadTexture(ref _logConsole_off, "LogConsole_off.png", 64, 64);
            LoadTexture(ref _forceBuild, "ForceBuild.png", 64, 64);
        }

        static public void SetTexture(ref Texture2D texture, Color backgroundColor)
        {
            if (texture == null)
            {
                texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            }

            texture.SetPixel(0, 1, backgroundColor);
            texture.Apply();
        }

        private void SetStyle(ref GUIStyle style, ref Texture2D texture, Color foregroundColor, int fontSize, TextAnchor textAnchor, bool wordWrap, int yOffset)
        {
            if (style == null)
            {
                style = new GUIStyle();
            }

            style.normal.background = texture;
            style.fontSize = fontSize;
            style.normal.textColor = foregroundColor;
            style.alignment = textAnchor;
            style.wordWrap = wordWrap;
            style.padding = new RectOffset(0, 0, yOffset, 0);
        }

        void OnHierarchyChange()
        {
            BuildHelper.UpdateSceneBoneAnimationMeshes();
        }
    }
}
