using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    public class TexturePivotWindow
    {
        private const float TOP_CONTROL_AREA_HEIGHT = 70.0f;
        private const float BOTTOM_CONTROL_AREA_HEIGHT = 40.0f;
        private const float PADDING = 10.0f;
        private const float CROSSHAIR_BOX_SIZE = 10.0f;
        private const float SCROLL_WHEEL_SENSITIVITY = 0.03f;
        private const float ZOOM_MIN = 0.01f;
        private const float ZOOM_MAX = 10.0f;

        private Event _evt;
        private Rect _areaRect;
        private Rect _editorRect;
        private Rect _textureRect;
        private Rect _pivotRect;
        private Vector2 _pivotPosition;
        private Texture2D _texture;
        private Vector2 _textureSize;
        private Vector2 _spriteSize;
        private bool _needsResized;
        private float _zoom = 1.0f;
        private Vector2 _dragOriginOffset;
        private Vector2 _dragStartPosition;
        private Vector2 _dragStartOrigin;
        private bool _dragging;
        private float _bottomSpace;
        private TextureAtlasEditorWindow _atlasEditor;

        private bool ShowGrid
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_TexturePivotShowGrid", 0) == 1 ? true : false);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_TexturePivotShowGrid", (value ? 1 : 0));
            }
        }

        private bool ShowBounds
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_TexturePivotShowBounds", 1) == 1 ? true : false);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_TexturePivotShowBounds", (value ? 1 : 0));
            }
        }

        public void OnEnable()
        {
        }

        public void SetBlankTexture()
        {
            SetTexture(null);
        }

        public void SetTextureByGUID(string guid)
        {
            if (guid != "")
            {
                TextureDictionaryEntry tde = TextureManager.TextureDictionary[guid];
                SetTexture(tde.texture);
            }
            else
            {
                SetBlankTexture();
            }
        }

        public void SetTexture(Texture2D texture)
        {
			if (_texture != texture)
			{
	            _texture = texture;
	
	            if (_texture != null)
	            {
	                _textureSize = new Vector2(_texture.width, _texture.height);
	            }
	            else
	            {
	                _textureSize = Vector2.one;
	            }
				
	            _needsResized = true;
			}
        }

        public bool OnGUI(ref Vector2 pivotOffset,
                                    Event evt,
                                    Rect areaRect,
                                    float bottomSpace,
                                    bool showDefaultChoice,
                                    ref bool usingDefault,
                                    Vector2 defaultPivotOffset)
        {
            _evt = evt;
            _areaRect = areaRect;
            _bottomSpace = bottomSpace;

            bool oldUsingDefault = usingDefault;
            Vector2 oldPivotOffset = pivotOffset;
            bool needsRepainted = false;

            if (_texture != null)
            {
                SetRects(pivotOffset, usingDefault, defaultPivotOffset);

                GUILayout.BeginVertical(Style.windowRectBackgroundStyle);
                DrawTopControls(ref pivotOffset, showDefaultChoice, ref usingDefault, defaultPivotOffset);
                DrawEditor(ref pivotOffset, usingDefault, defaultPivotOffset);
                DrawBottomControls();
                GUILayout.EndVertical();

                needsRepainted = GetInput(ref pivotOffset, evt, usingDefault);
            }

            if (oldUsingDefault)
            {
                return !usingDefault;
            }
            else
            {
                if (usingDefault)
                {
                    return true;
                }
                else
                {
                    return ((oldPivotOffset != pivotOffset) || needsRepainted);
                }
            }
        }

        private void SetRects(Vector2 pivotOffset, bool usingDefault, Vector2 defaultPivotOffset)
        {
            _editorRect = new Rect(
                                    PADDING,
                                    _areaRect.y + TOP_CONTROL_AREA_HEIGHT + PADDING,
                                    _areaRect.width - (PADDING * 2.0f),
                                    _areaRect.height - (TOP_CONTROL_AREA_HEIGHT + BOTTOM_CONTROL_AREA_HEIGHT + (PADDING * 2.0f))
                                    );

            _spriteSize.x = _textureSize.x * _zoom;
            _spriteSize.y = _textureSize.y * _zoom;

            _textureRect = new Rect(
                                    _dragOriginOffset.x - (_spriteSize.x / 2.0f),
                                    _dragOriginOffset.y - (_spriteSize.y / 2.0f),
                                    _spriteSize.x,
                                    _spriteSize.y
                                    );

            if (usingDefault)
            {
                _pivotPosition = new Vector2(_dragOriginOffset.x + (defaultPivotOffset.x * _spriteSize.x),
                                              _dragOriginOffset.y - (defaultPivotOffset.y * _spriteSize.y)
                                              );
            }
            else
            {
                _pivotPosition = new Vector2(_dragOriginOffset.x + (pivotOffset.x * _spriteSize.x),
                                              _dragOriginOffset.y - (pivotOffset.y * _spriteSize.y)
                                              );
            }


            _pivotRect = new Rect(
                                   _pivotPosition.x - (CROSSHAIR_BOX_SIZE / 2.0f),
                                   _pivotPosition.y - (CROSSHAIR_BOX_SIZE / 2.0f),
                                   CROSSHAIR_BOX_SIZE,
                                   CROSSHAIR_BOX_SIZE
                                   );
        }

        private void DrawTopControls(ref Vector2 pivotOffset, bool showDefaultChoice, ref bool usingDefault, Vector2 defaultPivotOffset)
        {
            float pivotOffsetX = pivotOffset.x;
            float pivotOffsetY = pivotOffset.y;

            GUILayout.BeginHorizontal(GUILayout.Height(TOP_CONTROL_AREA_HEIGHT));

                GUILayout.Space(15.0f);

                if (showDefaultChoice)
                {
                    GUIContent guiContent;
                    if (usingDefault)
                    {
                        guiContent = new GUIContent(Resources.buttonLockPivotDefaultOn, "Unlock Pivot");
                    }
                    else
                    {
                        guiContent = new GUIContent(Resources.buttonLockPivotDefaultOff, "Lock Pivot to Default");
                    }

                    if (GUILayout.Button(guiContent, Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonLockPivotDefaultOn.width), GUILayout.Height(Resources.buttonLockPivotDefaultOn.height)))
                    {
                        usingDefault = !usingDefault;
                    }
                }

                if (!usingDefault)
                {
                    GUILayout.Space(10.0f);

                    GUILayout.BeginVertical();
                    GUILayout.Label("   Pivot:", Style.normalLabelStyle);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("X: ", Style.normalLabelStyle);
                    pivotOffsetX = EditorGUILayout.FloatField(pivotOffset.x, GUILayout.Width(100.0f));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Y: ", Style.normalLabelStyle);
                    pivotOffsetY = EditorGUILayout.FloatField(pivotOffset.y, GUILayout.Width(100.0f));
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    pivotOffset = new Vector2(pivotOffsetX, pivotOffsetY);

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.TopLeft);
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.TopCenter);
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.TopRight);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.MiddleLeft);
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.MiddleCenter);
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.MiddleRight);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.BottomLeft);
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.BottomCenter);
                    DrawPivotButton(ref pivotOffset, Sprite.PIVOT_PRESET.BottomRight);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.Space(PADDING);
                }
                else
                {
                    pivotOffset = defaultPivotOffset;

                    GUILayout.Space(10.0f);

                    GUILayout.Label("The pivot has been locked to the default set in the texture atlas. You can either unlock the pivot and change it here, or open the atlas and change the default there.", Style.wordWrapStyle, GUILayout.Width(_areaRect.width - 20.0f - Resources.buttonLockPivotDefaultOff.width - 15.0f));
                }

            GUILayout.EndHorizontal();
        }


        private void DrawPivotButton(ref Vector2 pivotOffset, Sprite.PIVOT_PRESET pivotPreset)
        {
            bool highlight;

            highlight = (
                         (pivotOffset == new Vector2(-0.5f, 0.5f) && pivotPreset == Sprite.PIVOT_PRESET.TopLeft)
                         ||
                         (pivotOffset == new Vector2(0, 0.5f) && pivotPreset == Sprite.PIVOT_PRESET.TopCenter)
                         ||
                         (pivotOffset == new Vector2(0.5f, 0.5f) && pivotPreset == Sprite.PIVOT_PRESET.TopRight)
                         ||
                         (pivotOffset == new Vector2(-0.5f, 0) && pivotPreset == Sprite.PIVOT_PRESET.MiddleLeft)
                         ||
                         (pivotOffset == new Vector2(0, 0) && pivotPreset == Sprite.PIVOT_PRESET.MiddleCenter)
                         ||
                         (pivotOffset == new Vector2(0.5f, 0) && pivotPreset == Sprite.PIVOT_PRESET.MiddleRight)
                         ||
                         (pivotOffset == new Vector2(-0.5f, -0.5f) && pivotPreset == Sprite.PIVOT_PRESET.BottomLeft)
                         ||
                         (pivotOffset == new Vector2(0, -0.5f) && pivotPreset == Sprite.PIVOT_PRESET.BottomCenter)
                         ||
                         (pivotOffset == new Vector2(0.5f, -0.5f) && pivotPreset == Sprite.PIVOT_PRESET.BottomRight)
                        );

            //if (highlight)
            //    Style.PushColor(Style.setValueStyle);

            string tooltip = "";

            switch (pivotPreset)
            {
                case Sprite.PIVOT_PRESET.TopLeft:
                    tooltip = "Upper Left";
                    break;

                case Sprite.PIVOT_PRESET.TopCenter:
                    tooltip = "Upper Center";
                    break;

                case Sprite.PIVOT_PRESET.TopRight:
                    tooltip = "Upper Right";
                    break;

                case Sprite.PIVOT_PRESET.MiddleLeft:
                    tooltip = "Middle Left";
                    break;

                case Sprite.PIVOT_PRESET.MiddleCenter:
                    tooltip = "Middle Center";
                    break;

                case Sprite.PIVOT_PRESET.MiddleRight:
                    tooltip = "Middle Right";
                    break;

                case Sprite.PIVOT_PRESET.BottomLeft:
                    tooltip = "Bottom Left";
                    break;

                case Sprite.PIVOT_PRESET.BottomCenter:
                    tooltip = "Bottom Center";
                    break;

                case Sprite.PIVOT_PRESET.BottomRight:
                    tooltip = "Bottom Right";
                    break;
            }

            if (GUILayout.Button(new GUIContent((highlight ? Resources.buttonEmptyOn : Resources.buttonEmptyOff), tooltip), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonEmptyOff.width), GUILayout.Height(Resources.buttonEmptyOff.height)))
            {
                switch (pivotPreset)
                {
                    case Sprite.PIVOT_PRESET.TopLeft:
                        pivotOffset = new Vector2(-0.5f, 0.5f);
                        break;

                    case Sprite.PIVOT_PRESET.TopCenter:
                        pivotOffset = new Vector2(0, 0.5f);
                        break;

                    case Sprite.PIVOT_PRESET.TopRight:
                        pivotOffset = new Vector2(0.5f, 0.5f);
                        break;

                    case Sprite.PIVOT_PRESET.MiddleLeft:
                        pivotOffset = new Vector2(-0.5f, 0);
                        break;

                    case Sprite.PIVOT_PRESET.MiddleCenter:
                        pivotOffset = new Vector2(0, 0);
                        break;

                    case Sprite.PIVOT_PRESET.MiddleRight:
                        pivotOffset = new Vector2(0.5f, 0);
                        break;

                    case Sprite.PIVOT_PRESET.BottomLeft:
                        pivotOffset = new Vector2(-0.5f, -0.5f);
                        break;

                    case Sprite.PIVOT_PRESET.BottomCenter:
                        pivotOffset = new Vector2(0, -0.5f);
                        break;

                    case Sprite.PIVOT_PRESET.BottomRight:
                        pivotOffset = new Vector2(0.5f, -0.5f);
                        break;
                }
            }

            //if (highlight)
            //    Style.PopColor();
        }

        private void DrawEditor(ref Vector2 pivotOffset, bool usingDefault, Vector2 defaultPivotOffset)
        {
            Vector2 tempPivotOffset;

            if (usingDefault)
                tempPivotOffset = defaultPivotOffset;
            else
                tempPivotOffset = pivotOffset;

            if (_needsResized && _editorRect.width > 0 && _editorRect.height > 0)
            {
                CenterDragOriginOffset();

                float widthRatio = (_editorRect.width / Mathf.Max(_textureSize.x * 1.5f, _textureSize.x * Mathf.Abs(tempPivotOffset.x) * 1.5f));
                float heightRatio = (_editorRect.height / Mathf.Max(_textureSize.y * 1.5f, _textureSize.y * Mathf.Abs(tempPivotOffset.y) * 1.5f));

                _zoom = Mathf.Clamp(Mathf.Min(widthRatio, heightRatio), ZOOM_MIN, ZOOM_MAX); // *0.5f;

                _needsResized = false;
            }


                GUIStyle gridLineStyle;

                if (SettingsWindow.AnimationContrastDark)
                {
                    GUI.BeginGroup(_editorRect, Style.windowRectDarkBackgroundStyle);
                    gridLineStyle = Style.gridLineStyle;
                }
                else
                {
                    GUI.BeginGroup(_editorRect, Style.gridBackgroundStyle);
                    gridLineStyle = Style.lightGridLineStyle;
                }


                if (ShowBounds)
                    GUIHelper.DrawBox(_textureRect, Style.boundsStyle, true);

                GUI.DrawTexture(_textureRect, _texture, ScaleMode.StretchToFill);

                GUIHelper.DrawBox(_pivotRect, Style.setValueFaintStyle, false);

                if (_pivotPosition.x > 0 && _pivotPosition.x < _editorRect.width)
                {
                    GUIHelper.DrawVerticalLine(new Vector2(_pivotPosition.x, 1.0f), _editorRect.height - 1.0f, 1.0f, Style.setValueFaintStyle);
                }

                if (_pivotPosition.y > 0 && _pivotPosition.y < _editorRect.height)
                {
                    GUIHelper.DrawHorizontalLine(new Vector2(1.0f, _pivotPosition.y), _editorRect.width - 1.0f, 1.0f, Style.setValueFaintStyle);
                }

                if (_zoom > 0)
                {
                    if ((_zoom > 0.3f || SettingsWindow.AnimationGridSize > 20.0f) && ShowGrid)
                    {
                        // horizontal lines, centered on the offset
                        for (float y = _dragOriginOffset.y + (SettingsWindow.AnimationGridSize * _zoom);
                             y <= _editorRect.height;
                            y += (SettingsWindow.AnimationGridSize * _zoom)
                            )
                        {
                            GUIHelper.DrawHorizontalLine(new Vector2(0, y), _editorRect.width, 1.0f, gridLineStyle);
                        }
                        for (float y = _dragOriginOffset.y - (SettingsWindow.AnimationGridSize * _zoom);
                             y >= 0;
                            y -= (SettingsWindow.AnimationGridSize * _zoom)
                            )
                        {
                            GUIHelper.DrawHorizontalLine(new Vector2(0, y), _editorRect.width, 1.0f, gridLineStyle);
                        }

                        // vertical lines, centered on the offset
                        for (float x = _dragOriginOffset.x + (SettingsWindow.AnimationGridSize * _zoom);
                             x <= _editorRect.width;
                            x += (SettingsWindow.AnimationGridSize * _zoom))
                        {
                            GUIHelper.DrawVerticalLine(new Vector2(x, 0), _editorRect.height, 1.0f, gridLineStyle);
                        }
                        for (float x = _dragOriginOffset.x - (SettingsWindow.AnimationGridSize * _zoom);
                             x >= 0;
                            x -= (SettingsWindow.AnimationGridSize * _zoom))
                        {
                            GUIHelper.DrawVerticalLine(new Vector2(x, 0), _editorRect.height, 1.0f, gridLineStyle);
                        }

                        GUIHelper.DrawHorizontalLine(new Vector2(0, _dragOriginOffset.y), _editorRect.width, 1.0f, Style.xAxisStyle);
                        GUIHelper.DrawVerticalLine(new Vector2(_dragOriginOffset.x, 0), _editorRect.height, 1.0f, Style.yAxisStyle);
                    }
                }

            GUI.EndGroup();
        }

        private void DrawBottomControls()
        {
            GUILayout.Space(_editorRect.height + _bottomSpace); // + (PADDING * 1.5f));

            GUILayout.BeginVertical(GUILayout.Height(BOTTOM_CONTROL_AREA_HEIGHT));

                GUILayout.BeginHorizontal();

                GUILayout.Space(PADDING);

                if (GUILayout.Button(new GUIContent(Resources.buttonCenter, "Center on Origin"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonCenter.width), GUILayout.Height(Resources.buttonCenter.height)))
                {
                    CenterDragOriginOffset();
                }

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonZoomOne, "Set Zoom to One"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonZoomOne.width), GUILayout.Height(Resources.buttonZoomOne.height)))
                {
                    _zoom = 1.0f;
                }

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonContrast, "Toggle Contrast"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonZoomOne.width), GUILayout.Height(Resources.buttonZoomOne.height)))
                {
                    SettingsWindow.AnimationContrastDark = !SettingsWindow.AnimationContrastDark;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent((ShowGrid ? Resources.buttonGridOn : Resources.buttonGridOff), "Toggle Grid"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonGridOff.width), GUILayout.Height(Resources.buttonGridOff.height)))
                {
                    ShowGrid = !ShowGrid;
                }

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent((ShowBounds ? Resources.buttonBoundsOn : Resources.buttonBoundsOff), "Toggle Bounds"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonBoundsOff.width), GUILayout.Height(Resources.buttonBoundsOff.height)))
                {
                    ShowBounds = !ShowBounds;
                }

                GUILayout.Space(PADDING);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(PADDING);

                GUILayout.Label("Size: X=" + _textureSize.x.ToString() + " Y=" + _textureSize.y.ToString(), Style.normalLabelStyle, GUILayout.Width(120.0f));

                GUILayout.Label("Zoom: ", Style.normalLabelStyle, GUILayout.Width(45.0f));
                _zoom = GUILayout.HorizontalSlider(_zoom, ZOOM_MIN, ZOOM_MAX);
                GUILayout.Label("x " + EditorHelper.RoundFloat(_zoom, 2).ToString(), Style.normalLabelStyle, GUILayout.Width(45.0f));

                GUILayout.Space(PADDING);

                GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void CenterDragOriginOffset()
        {
            _dragOriginOffset.x = (_editorRect.width * 0.5f);
            _dragOriginOffset.y = (_editorRect.height * 0.5f);
        }

        private bool GetInput(ref Vector2 pivotOffset, Event evt, bool usingDefault)
        {
            Vector2 areaMousePos;
            bool needsRepainted = false;

            if (_editorRect.Contains(_evt.mousePosition))
            {
                areaMousePos = _evt.mousePosition - GUIHelper.UpperLeftCorner(_editorRect);

                if (EditorHelper.LeftMouseButton(_evt) && !usingDefault)
                {
                    switch (_evt.type)
                    {
                        case EventType.MouseDown:
                        case EventType.MouseDrag:
                            pivotOffset = GetOffset(areaMousePos);
                            _evt.Use();
                            break;
                    }
                }
                else if (EditorHelper.MiddleMouseButton(_evt))
                {
                    switch (_evt.type)
                    {
                        case EventType.MouseDown:
                            _dragStartPosition = areaMousePos;
                            _dragStartOrigin = _dragOriginOffset;
                            _dragging = true;
                            evt.Use();
                            break;

                        case EventType.MouseDrag:
                            if (_dragging)
                            {
                                _dragOriginOffset = _dragStartOrigin + (areaMousePos - _dragStartPosition);
                                needsRepainted = true;
                                _evt.Use();
                            }
                            break;

                        case EventType.MouseUp:
                            _dragging = false;
                            evt.Use();
                            break;
                    }
                }
                
                if (evt.type == EventType.ScrollWheel)
                {
                    _zoom = Mathf.Clamp((_zoom - (evt.delta.y * SCROLL_WHEEL_SENSITIVITY)), ZOOM_MIN, ZOOM_MAX);
                    needsRepainted = true;
                    evt.Use();
                }
            }

            return needsRepainted;
        }

        private Vector2 GetOffset(Vector2 mousePos)
        {
            Vector2 offset;

            offset.x = ((mousePos.x - _dragOriginOffset.x) / _spriteSize.x);
            offset.y = ((_dragOriginOffset.y - mousePos.y) / _spriteSize.y);

            return offset;
        }

        private TextureAtlasEditorWindow GetAtlasEditor()
        {
            if (_atlasEditor == null)
                _atlasEditor = TextureAtlasEditorWindow.ShowEditor();

            return _atlasEditor;
        }
    }
}
