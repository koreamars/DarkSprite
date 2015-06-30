using UnityEngine;
using UnityEditor;
using SmoothMoves;

namespace SmoothMoves
{
    public class WelcomeScreen : EditorWindow
    {
        private const float WIDTH = 440.0f;
        private const float HEIGHT = 420.0f;

        static private WelcomeScreen _instance;

        private Color OrangeColor;
        private Color PurpleColor;

        private Texture2D _Grey2Texture;
        private Texture2D _Grey7Texture;
        private Texture2D _WhiteTexture;
        private GUIStyle _windowRectBackgroundStyle;
        private GUIStyle _windowRectBackgroundStyleLight;
        private GUIStyle _backgroundStyle;

        private Texture2D _smoothMovesLogo;

        private GUIStyle _noBorderButtonStyle;
        private GUIStyle _wrapLabelStyle;
        private GUIStyle _importantStyle;
        private GUIStyle _importantStyle2;
        private GUIStyle _toggleStyle;

        static public WelcomeScreen Instance
        {
            get
            {
                return _instance;
            }
        }

        static public WelcomeScreen ShowEditor()
        {
            if (_instance == null)
            {
                _instance = (WelcomeScreen)EditorWindow.GetWindow(typeof(WelcomeScreen), true, "SmoothMoves - Welcome");
            }

            _instance.ShowUtility();

            _instance.position = new Rect(50.0f, 50.0f, WIDTH, HEIGHT);

            Style.Reset();

            return _instance;
        }

        void OnDestroy()
        {
            _instance = null;
        }

        void OnGUI()
        {
            LoadStyles();
            LoadTextures();

            GUILayout.BeginVertical(_backgroundStyle);

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10.0f);





            // overall vertical
            GUILayout.BeginVertical();

            GUILayout.Label(_smoothMovesLogo);

            GUILayout.Space(10.0f);

            // text box
            GUILayout.BeginVertical(_windowRectBackgroundStyle);

            GUILayout.Space(10.0f);

            // text inset
            GUILayout.BeginHorizontal();

            GUILayout.Space(10.0f);

            // text layout
            GUILayout.BeginVertical();

            GUILayout.Label("Welcome to SmoothMoves v2!", _importantStyle);

            GUILayout.Space(10.0f);

            GUILayout.Label("There have been quite a few " + 
                             "workflow improvements since version 1, so you should " + 
                             "familiarize yourself by clicking the \"View v2 Upgrade " + 
                             "Guide\" button below.",
                             _wrapLabelStyle,
                             GUILayout.Width(400.0f)
                             );

            GUILayout.Space(10.0f);

            GUILayout.Label("Happy Animating!", 
                             _importantStyle2
                             );

            GUILayout.Space(10.0f);

            // buttons
            GUILayout.BeginHorizontal();

            Style.PushColor(PurpleColor);
            if (GUILayout.Button("View v2 Upgrage Guide", GUILayout.Width(150.0f), GUILayout.Height(40.0f)))
            {
                Application.OpenURL("http://www.echo17.com/support/smoothmoves_v2_upgrade_guide.pdf");
            }
            Style.PopColor();

            GUILayout.Space(10.0f);

            if (GUILayout.Button("User Manual", GUILayout.Width(100.0f), GUILayout.Height(40.0f)))
            {
                Application.OpenURL("http://www.echo17.com/support/smoothmoves_user_manual_v2.pdf");
            }

            GUILayout.Space(10.0f);

            if (GUILayout.Button("Script API", GUILayout.Width(100.0f), GUILayout.Height(40.0f)))
            {
                Application.OpenURL("http://www.echo17.com/support/smoothmoves_api_documentation/index.html");
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            EditorHelper.ShowWelcomeScreen = GUILayout.Toggle(EditorHelper.ShowWelcomeScreen, "Show this window when opening the editor", _toggleStyle);

            // text layout
            GUILayout.EndVertical();

            GUILayout.Space(10.0f);

            // text inset
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            // text box
            GUILayout.EndVertical();

            // overall vertical
            GUILayout.EndVertical();






            GUILayout.Space(10.0f);

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.Space(10.0f);

            GUILayout.EndVertical();
        }

        private void LoadStyles()
        {
            OrangeColor = new Color(0.99f, 0.87f, 0.64f);
            PurpleColor = new Color(0.92f, 0.77f, 1.00f);

            _noBorderButtonStyle = new GUIStyle();
            _noBorderButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            _noBorderButtonStyle.padding = new RectOffset(0, 0, 0, 0);

            _wrapLabelStyle = new GUIStyle();
            _wrapLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            _wrapLabelStyle.alignment = TextAnchor.MiddleLeft;
            _wrapLabelStyle.wordWrap = true;

            _toggleStyle = new GUIStyle(GUI.skin.toggle);
            _toggleStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            _toggleStyle.onNormal.textColor = new Color(0.7f, 0.7f, 0.7f);
            _toggleStyle.active.textColor = new Color(0.7f, 0.7f, 0.7f);
            _toggleStyle.onActive.textColor = new Color(0.7f, 0.7f, 0.7f);
            _toggleStyle.alignment = TextAnchor.MiddleLeft;
            _toggleStyle.wordWrap = true;

            _importantStyle = new GUIStyle();
            _importantStyle.normal.textColor = OrangeColor;
            _importantStyle.alignment = TextAnchor.MiddleLeft;
            _importantStyle.wordWrap = true;

            _importantStyle2 = new GUIStyle();
            _importantStyle2.normal.textColor = PurpleColor;
            _importantStyle2.alignment = TextAnchor.MiddleLeft;
            _importantStyle2.wordWrap = true;

            SetTexture(ref _Grey2Texture, new Color(0.2f, 0.2f, 0.2f));
            SetTexture(ref _Grey7Texture, new Color(0.7f, 0.7f, 0.7f));
            SetTexture(ref _WhiteTexture, Color.white);

            SetStyle(ref _windowRectBackgroundStyle, ref _Grey2Texture, new Color(1.0f, 1.0f, 1.0f), 10, TextAnchor.MiddleCenter, true, 0);
            SetStyle(ref _windowRectBackgroundStyleLight, ref _Grey7Texture, new Color(0.2f, 0.2f, 0.2f), 10, TextAnchor.MiddleCenter, true, 0);
            SetStyle(ref _backgroundStyle, ref _WhiteTexture, new Color(0.8f, 0.8f, 0.8f), 10, TextAnchor.MiddleLeft, true, 0);
        }

        private void LoadTextures()
        {
            GUIHelper.LoadTexture(ref _smoothMovesLogo, "SmoothMoves_Logo.png", 394, 189); 
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
    }
}
