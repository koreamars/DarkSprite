using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SmoothMoves
{
    static public class Style
    {
        // Pallette
        static public Color LightPurple;
        static public Color DarkPurple;
        static public Color DeepPurple;
        static public Color Yellow;
        static public Color Salmon;
        static public Color DarkOrange;
        static public Color DarkOrangeFaint;
        static public Color LightOrange;
        static public Color Red;
        static public Color LightRed;
        static public Color White;
        static public Color Black;
        static public Color LightGreen;
        static public Color LightGreenFaint;
        static public Color LightPurpleFaint;
        static public Color RedFaint;
        static public Color GreenFaint;
        static public Color BlueFaint;
        static public Color Grey1;
        static public Color Grey2;
        static public Color Grey3;
        static public Color Grey4;
        static public Color Grey5;
        static public Color Grey6;
        static public Color Grey7;
        static public Color Grey8;
        static public Color Grey9;
        static public Color Grey3Faint;
        static public Color Blank;
        static public Color Green;

        // textures
        static public Texture2D LightPurpleTexture;
        static public Texture2D DarkPurpleTexture;
        static public Texture2D YellowTexture;
        static public Texture2D SalmonTexture;
        static public Texture2D DarkOrangeTexture;
        static public Texture2D DarkOrangeFaintTexture;
        static public Texture2D LightOrangeTexture;
        static public Texture2D RedTexture;
        static public Texture2D LightRedTexture;
        static public Texture2D WhiteTexture;
        static public Texture2D BlackTexture;
        static public Texture2D LightGreenTexture;
        static public Texture2D LightGreenFaintTexture;
        static public Texture2D LightPurpleFaintTexture;
        static public Texture2D RedFaintTexture;
        static public Texture2D GreenFaintTexture;
        static public Texture2D BlueFaintTexture;
        static public Texture2D Grey1Texture;
        static public Texture2D Grey2Texture;
        static public Texture2D Grey3Texture;
        static public Texture2D Grey4Texture;
        static public Texture2D Grey5Texture;
        static public Texture2D Grey6Texture;
        static public Texture2D Grey7Texture;
        static public Texture2D Grey8Texture;
        static public Texture2D Grey9Texture;
        static public Texture2D Grey3FaintTexture;
        static public Texture2D BlankTexture;
        static public Texture2D BoneColorNoneTexture;

        // styles
        static public GUIStyle selectionDoneStyle;
        static public GUIStyle selectionWorkingStyle;
        static public GUIStyle warningStyle;
        static public GUIStyle errorStyle;
        static public GUIStyle windowRectBackgroundStyle;
        static public GUIStyle windowRectDarkBackgroundStyle;
        static public GUIStyle selectedInformationStyle;
        static public GUIStyle selectedInformationFieldStyle;
        static public GUIStyle selectedInformationValueStyle;
        static public GUIStyle emptyKeyframeWarningStyle;
        static public GUIStyle selectedEmptyKeyframeWarningStyle;
        static public GUIStyle fieldStyle;
        static public GUIStyle unSelectedInformationStyle;
        static public GUIStyle targetStyle;
        static public GUIStyle setValueStyle;
        static public GUIStyle setValueSelectedStyle;
        static public GUIStyle setValueFaintStyle;
        static public GUIStyle boundsStyle;
        static public GUIStyle boundsDarkStyle;
        static public GUIStyle xAxisStyle;
        static public GUIStyle yAxisStyle;
        static public GUIStyle zAxisStyle;
        static public GUIStyle centeredTextStyle;
        static public GUIStyle centeredGreyTextStyle;
        static public GUIStyle timelineLabelStyle;
        static public GUIStyle smallLeftTextStyle;
        static public GUIStyle gridLineStyle;
        static public GUIStyle gridBackgroundStyle;
        static public GUIStyle gridFrameStyle;
        static public GUIStyle gridFrameMarkerStyle;
        static public GUIStyle whiteStyle;
        static public GUIStyle blankStyle;
        static public GUIStyle selectedTextureStyle;
        static public GUIStyle unSelectedTextureStyle;
        static public GUIStyle antiSelectionStyle;
        static public GUIStyle popupWindowStyle;
        static public GUIStyle blackStyle;
		static public GUIStyle animationLabelStyle;
        static public GUIStyle propertiesGroupStyle;
        static public GUIStyle wordWrapStyle;
		static public GUIStyle emptyStyle;
        static public GUIStyle colliderStyle;
        static public GUIStyle currentFrameMarkerStyle;
        static public GUIStyle highlightLabelStyle;
        static public GUIStyle maskStyle;
        static public GUIStyle selectTextureLabelOffStyle;
        static public GUIStyle selectTextureLabelOnStyle;
        static public GUIStyle normalLabelStyle;
        static public GUIStyle normalToggleStyle;
        static public GUIStyle lightGridLineStyle;
        static public GUIStyle disabledStyle;
        static public GUIStyle inactiveStyle;
        static public GUIStyle lightStyle;
        static public GUIStyle keyframeAlreadySetStyle;

        static public GUIStyle noBorderButtonStyle;
        static public GUIStyle boneColorButtonStyle;
        static public GUIStyle boneNoColorButtonStyle;

        static private Stack<Color> _colorStack;
        static private Stack<Color> _backgroundColorStack;

        static public void Reset()
        {
			CreateStacks();
			
            LightPurple = new Color(0.92f, 0.77f, 1.00f);
            DarkPurple = new Color(0.83f, 0.55f, 0.99f);
            Yellow = new Color(0.98f, 1.0f, 0.61f);
            Salmon = new Color(1.0f, 0.38f, 0.38f);
            DarkOrange = new Color(1.0f, 0.74f, 0.29f);
            DarkOrangeFaint = new Color(DarkOrange.r, DarkOrange.g, DarkOrange.b, 0.5f);
            LightOrange = new Color(0.99f, 0.87f, 0.64f);
            Red = new Color(0.79f, 0.22f, 0.22f);
            LightRed = new Color(0.91f, 0.48f, 0.48f);
            White = new Color(1.0f, 1.0f, 1.0f);
            Black = new Color(0, 0, 0);
            LightGreen = new Color(0.44f, 0.77f, 0.55f);
            LightGreenFaint = new Color(0.44f, 0.77f, 0.55f, 0.6f);
            LightPurpleFaint = new Color(LightPurple.r, LightPurple.g, LightPurple.b, 0.5f);
            RedFaint = new Color(1.0f, 0, 0, 0.3f);
            GreenFaint = new Color(0, 1.0f, 0, 0.3f);
            BlueFaint = new Color(0, 0, 1.0f, 0.3f);
            Grey1 = new Color(0.1f, 0.1f, 0.1f);
            Grey2 = new Color(0.2f, 0.2f, 0.2f);
            Grey3 = new Color(0.3f, 0.3f, 0.3f);
            Grey4 = new Color(0.4f, 0.4f, 0.4f);
            Grey5 = new Color(0.5f, 0.5f, 0.5f);
            Grey6 = new Color(0.6f, 0.6f, 0.6f);
            Grey7 = new Color(0.7f, 0.7f, 0.7f);
            Grey8 = new Color(0.8f, 0.8f, 0.8f);
            Grey9 = new Color(0.9f, 0.9f, 0.9f);
            Grey3Faint = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            Blank = new Color(0, 0, 0, 0);
            Green = new Color(0, 1.0f, 0);

            SetTexture(ref DarkPurpleTexture, DarkPurple);
            SetTexture(ref LightPurpleTexture, LightPurple);
            SetTexture(ref YellowTexture, Yellow);
            SetTexture(ref SalmonTexture, Salmon);
            SetTexture(ref DarkOrangeTexture, DarkOrange);
            SetTexture(ref DarkOrangeFaintTexture, DarkOrangeFaint);
            SetTexture(ref LightOrangeTexture, LightOrange);
            SetTexture(ref RedTexture, Red);
            SetTexture(ref LightRedTexture, LightRed);
            SetTexture(ref WhiteTexture, White);
            SetTexture(ref BlackTexture, Black);
            SetTexture(ref LightGreenTexture, LightGreen);
            SetTexture(ref LightGreenFaintTexture, LightGreenFaint);
            SetTexture(ref LightPurpleFaintTexture, LightPurpleFaint);
            SetTexture(ref RedFaintTexture, RedFaint);
            SetTexture(ref GreenFaintTexture, GreenFaint);
            SetTexture(ref BlueFaintTexture, BlueFaint);
            SetTexture(ref Grey1Texture, Grey1);
            SetTexture(ref Grey2Texture, Grey2);
            SetTexture(ref Grey3Texture, Grey3);
            SetTexture(ref Grey4Texture, Grey4);
            SetTexture(ref Grey5Texture, Grey5);
            SetTexture(ref Grey6Texture, Grey6);
            SetTexture(ref Grey7Texture, Grey7);
            SetTexture(ref Grey8Texture, Grey8);
            SetTexture(ref Grey9Texture, Grey9);
            SetTexture(ref Grey3FaintTexture, Grey3Faint);
            SetTexture(ref BlankTexture, Blank);

            GUIHelper.LoadTexture(ref BoneColorNoneTexture, "BoneColorNone.png", 8, 20);

            SetStyle(ref selectionDoneStyle, ref DarkPurpleTexture, White);
            SetStyle(ref selectionWorkingStyle, ref LightPurpleTexture, White);
            SetStyle(ref warningStyle, ref YellowTexture, Black);
            SetStyle(ref errorStyle, ref RedFaintTexture, White);
            SetStyle(ref windowRectBackgroundStyle, ref Grey2Texture, White);
            SetStyle(ref windowRectDarkBackgroundStyle, ref Grey1Texture, White);
            SetStyle(ref disabledStyle, ref Grey3Texture, White);
            SetStyle(ref inactiveStyle, ref Grey1Texture, White);
            SetStyle(ref selectedInformationStyle, ref DarkPurpleTexture, White);
            SetStyle(ref selectedInformationFieldStyle, ref BlankTexture, Grey3, 11, TextAnchor.MiddleLeft);
            SetStyle(ref selectedInformationValueStyle, ref BlankTexture, White, 11, TextAnchor.MiddleLeft);
            SetStyle(ref emptyKeyframeWarningStyle, ref RedTexture, White);
            SetStyle(ref selectedEmptyKeyframeWarningStyle, ref LightRedTexture, White);
            SetStyle(ref fieldStyle, ref BlankTexture, Black, 11, TextAnchor.MiddleLeft);
            SetStyle(ref unSelectedInformationStyle, ref Grey5Texture, White);
            SetStyle(ref targetStyle, ref LightGreenTexture, White);
            SetStyle(ref setValueStyle, ref DarkOrangeTexture, White);
            SetStyle(ref setValueSelectedStyle, ref LightOrangeTexture, White);
            SetStyle(ref setValueFaintStyle, ref DarkOrangeFaintTexture, White);
            SetStyle(ref boundsStyle, ref LightPurpleFaintTexture, White);
            SetStyle(ref boundsDarkStyle, ref Grey2Texture, White);
            SetStyle(ref lightStyle, ref WhiteTexture, Black);
            SetStyle(ref xAxisStyle, ref RedFaintTexture, White);
            SetStyle(ref yAxisStyle, ref GreenFaintTexture, White);
            SetStyle(ref zAxisStyle, ref BlueFaintTexture, White);
            SetStyle(ref centeredTextStyle, ref BlankTexture, Grey7);
            SetStyle(ref centeredGreyTextStyle, ref BlankTexture, Grey5);
            SetStyle(ref timelineLabelStyle, ref BlankTexture, Black, 7, TextAnchor.MiddleCenter);
            SetStyle(ref gridLineStyle, ref Grey3FaintTexture, White);
            SetStyle(ref lightGridLineStyle, ref Grey9Texture, Black);
            SetStyle(ref gridBackgroundStyle, ref Grey7Texture, White);
            SetStyle(ref gridFrameStyle, ref Grey3Texture, White);
            SetStyle(ref gridFrameMarkerStyle, ref Grey4Texture, White);
            SetStyle(ref whiteStyle, ref WhiteTexture, Black);
            SetStyle(ref blankStyle, ref BlankTexture, White, 11, TextAnchor.MiddleCenter, false, 0);
            SetStyle(ref selectedTextureStyle, ref DarkPurpleTexture, White);
            SetStyle(ref unSelectedTextureStyle, ref BlankTexture, White);
            SetStyle(ref antiSelectionStyle, ref Grey5Texture, Black);
            SetStyle(ref popupWindowStyle, ref Grey8Texture, Black);
            SetStyle(ref blackStyle, ref BlackTexture, Black);
			SetStyle(ref animationLabelStyle, ref BlankTexture, LightPurple, 11, TextAnchor.MiddleLeft);
            SetStyle(ref propertiesGroupStyle, ref Grey3Texture, Black, 11, TextAnchor.MiddleLeft);
            SetStyle(ref wordWrapStyle, ref BlankTexture, Grey7, 11, TextAnchor.UpperLeft, true, 2);
            SetStyle(ref colliderStyle, ref LightGreenTexture, White);
            SetStyle(ref currentFrameMarkerStyle, ref LightGreenFaintTexture, White);
            SetStyle(ref highlightLabelStyle, ref BlankTexture, LightGreen);
            SetStyle(ref maskStyle, ref Grey2Texture, White);
            SetStyle(ref selectTextureLabelOffStyle, ref BlankTexture, Grey7, 8, TextAnchor.UpperCenter);
            SetStyle(ref selectTextureLabelOnStyle, ref DarkPurpleTexture, White, 8, TextAnchor.UpperCenter);
            SetStyle(ref normalLabelStyle, ref BlankTexture, Grey7, 11, TextAnchor.UpperLeft);
            //SetStyle(ref normalToggleStyle, ref BlankTexture, Grey7, 11, TextAnchor.UpperLeft, false, GUI.skin.toggle);

            if (noBorderButtonStyle == null)
            {
                noBorderButtonStyle = new GUIStyle();
            }
            noBorderButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            noBorderButtonStyle.padding = new RectOffset(0, 0, 0, 0);

            if (boneColorButtonStyle == null)
            {
                boneColorButtonStyle = new GUIStyle();
            }
            boneColorButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            boneColorButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            boneColorButtonStyle.normal.background = WhiteTexture;

            if (boneNoColorButtonStyle == null)
            {
                boneNoColorButtonStyle = new GUIStyle();
            }
            boneNoColorButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            boneNoColorButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            boneNoColorButtonStyle.normal.background = BoneColorNoneTexture;

            keyframeAlreadySetStyle = new GUIStyle();
            keyframeAlreadySetStyle.normal.background = Resources.warning;

			if (emptyStyle == null)
			{
				emptyStyle = new GUIStyle();
			}
        }

        static public void OnGUI()
        {
            if (normalToggleStyle == null)
            {
                normalToggleStyle = new GUIStyle(GUI.skin.toggle);
                normalToggleStyle.normal.textColor = Grey7;
                normalToggleStyle.onNormal.textColor = Grey7;
                normalToggleStyle.active.textColor = Grey7;
                normalToggleStyle.focused.textColor = Grey7;
                normalToggleStyle.hover.textColor = Grey7;
            }
        }
		
		static private void CreateStacks()
		{
            if (_colorStack == null)
                _colorStack = new Stack<Color>();

            if (_backgroundColorStack == null)
                _backgroundColorStack = new Stack<Color>();
		}
		
        static public void SetStyle(ref GUIStyle style, ref Texture2D texture, Color foregroundColor)
        {
            SetStyle(ref style, ref texture, foregroundColor, 11, TextAnchor.MiddleCenter, false, 2);
        }

        static public void SetStyle(ref GUIStyle style, ref Texture2D texture, Color foregroundColor, int fontSize, TextAnchor textAnchor)
        {
            SetStyle(ref style, ref texture, foregroundColor, fontSize, textAnchor, false, 2);
        }

        static public void SetStyle(ref GUIStyle style, ref Texture2D texture, Color foregroundColor, int fontSize, TextAnchor textAnchor, bool wordWrap, int yOffset)
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

        static public void SetTexture(ref Texture2D texture, Color backgroundColor)
        {
            if (texture == null)
            {
                texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            }

            texture.SetPixel(0, 1, backgroundColor);
            texture.Apply();
        }

        static public Color GetStyleBackgroundColor(GUIStyle style)
        {
			if (style == null)
				return Color.black;

			if (style.normal.background == null)
				return Color.black;
			
            return style.normal.background.GetPixel(1, 1);
        }

        static public void PushBackgroundColor(GUIStyle style)
        {
            PushBackgroundColor(GetStyleBackgroundColor(style));
        }

        static public void PushBackgroundColor(Color c)
        {
			CreateStacks();
            _backgroundColorStack.Push(GUI.color);
            GUI.backgroundColor = c;
        }

        static public void PopBackgroundColor()
        {
            GUI.backgroundColor = _backgroundColorStack.Pop();
        }

        static public void PushColor(GUIStyle style)
        {
            PushColor(GetStyleBackgroundColor(style), 1.0f);
        }
		
		static public void PushColor(GUIStyle style, float brightnessFactor)
		{
			PushColor(GetStyleBackgroundColor(style), brightnessFactor);
		}

        static public void PushColor(Color c)
        {
			PushColor(c, 1.0f);
        }
		
		static public void PushColor(Color c, float brightnessFactor)
		{
			CreateStacks();
			_colorStack.Push(GUI.color);
			Color c2 = new Color(c.r * brightnessFactor, c.g * brightnessFactor, c.b * brightnessFactor, c.a);
            GUI.color = c2;
		}

        static public void PopColor()
        {
            GUI.color = _colorStack.Pop();
        }
    }
}
