using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

namespace SmoothMoves
{
    static public class GUIHelper
    {
        static private bool clippingEnabled;
        static private Rect clippingBounds;
        static private Material lineMaterial;

        /* @ Credit: "http://cs-people.bu.edu/jalon/cs480/Oct11Lab/clip.c" */
        static private bool clip_test(float p, float q, ref float u1, ref float u2)
        {
            float r;
            bool retval = true;
            if (p < 0.0)
            {
                r = q / p;
                if (r > u2)
                    retval = false;
                else if (r > u1)
                    u1 = r;
            }
            else if (p > 0.0)
            {
                r = q / p;
                if (r < u1)
                    retval = false;
                else if (r < u2)
                    u2 = r;
            }
            else
                if (q < 0.0)
                    retval = false;

            return retval;
        }

        static private bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
        {
            float u1 = 0.0f, u2 = 1.0f, dx = p2.x - p1.x, dy;
            if (clip_test(-dx, p1.x - bounds.xMin, ref u1, ref u2))
                if (clip_test(dx, bounds.xMax - p1.x, ref u1, ref u2))
                {
                    dy = p2.y - p1.y;
                    if (clip_test(-dy, p1.y - bounds.yMin, ref u1, ref u2))
                        if (clip_test(dy, bounds.yMax - p1.y, ref u1, ref u2))
                        {
                            if (u2 < 1.0)
                            {
                                p2.x = p1.x + u2 * dx;
                                p2.y = p1.y + u2 * dy;
                            }
                            if (u1 > 0.0)
                            {
                                p1.x += u1 * dx;
                                p1.y += u1 * dy;
                            }
                            return true;
                        }
                }
            return false;
        }

        static public void BeginGroup(Rect position)
        {
            clippingEnabled = true;
            clippingBounds = new Rect(0, 0, position.width, position.height);
            GUI.BeginGroup(position);
        }

        static public void EndGroup()
        {
            GUI.EndGroup();
            clippingBounds = new Rect(0, 0, Screen.width, Screen.height);
            clippingEnabled = false;
        }

        static public void DrawLine(Vector2 pointA, Vector2 pointB, GUIStyle style)
        {
            DrawLine(pointA, pointB, Style.GetStyleBackgroundColor(style));
        }

        static public void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
        {
            SetLineMaterial();
            lineMaterial.SetPass(0);

            if (clippingEnabled)
                if (!segment_rect_intersection(clippingBounds, ref pointA, ref pointB))
                    return;

            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(pointA.x, pointA.y, 0);
            GL.Vertex3(pointB.x, pointB.y, 0);
            GL.End();
        }

        static public void DrawHorizontalLine(float y, float x1, float x2, Color color)
        {
            SetLineMaterial();
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(x1, y, 0);
            GL.Vertex3(x2, y, 0);
            GL.End();
        }

        static public void DrawVerticalLine(float x, float y1, float y2, Color color)
        {
            SetLineMaterial();
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(x, y1, 0);
            GL.Vertex3(x, y2, 0);
            GL.End();
        }

        static public void DrawGrid(float xStart, float yStart, float xInterval, float yInterval, int xCount, int yCount, Color color)
        {
            SetLineMaterial();
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            float x, y;
            for (int xCounter = 0; xCounter < xCount; xCounter++)
            {
                x = xStart + (xCounter * xInterval);
                y = yStart + (yCount * yInterval);

                GL.Vertex3(x, yStart, 0);
                GL.Vertex3(x, y, 0);
            }
            for (int yCounter = 0; yCounter < yCount; yCounter++)
            {
                x = xStart + (xCount * xInterval);
                y = yStart + (yCounter * yInterval);

                GL.Vertex3(xStart, y, 0);
                GL.Vertex3(x, y, 0);
            }

            GL.End();
        }

        static public void DrawGridFromOrigin(Vector2 origin, Rect bounds, float xInterval, float yInterval, Color color)
        {
            float x, y;
            int leftXCount;
            int rightXCount;
            int topYCount;
            int bottomYCount;

            leftXCount = Mathf.CeilToInt((origin.x - bounds.xMin) / xInterval);
            rightXCount = Mathf.CeilToInt((bounds.xMax - origin.x) / xInterval);
            topYCount = Mathf.CeilToInt((origin.y - bounds.yMin) / yInterval);
            bottomYCount = Mathf.CeilToInt((bounds.yMax - origin.y) / yInterval);

            SetLineMaterial();
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < leftXCount; i++)
            {
                x = origin.x - (i * xInterval);

                if (x > bounds.xMin && x < bounds.xMax)
                {
                    GL.Vertex3(x, bounds.yMin, 0);
                    GL.Vertex3(x, bounds.yMax, 0);
                }
            }
            for (int i = 0; i < rightXCount; i++)
            {
                x = origin.x + (i * xInterval);

                if (x > bounds.xMin && x < bounds.xMax)
                {
                    GL.Vertex3(x, bounds.yMin, 0);
                    GL.Vertex3(x, bounds.yMax, 0);
                }
            }

            for (int i = 1; i < topYCount; i++)
            {
                y = origin.y - (i * yInterval);

                if (y > bounds.yMin && y < bounds.yMax)
                {
                    GL.Vertex3(bounds.xMin, y, 0);
                    GL.Vertex3(bounds.xMax, y, 0);
                }
            }
            for (int i = 1; i < bottomYCount; i++)
            {
                y = origin.y + (i * yInterval);

                if (y > bounds.yMin && y < bounds.yMax)
                {
                    GL.Vertex3(bounds.xMin, y, 0);
                    GL.Vertex3(bounds.xMax, y, 0);
                }
            }

            GL.End();
        }

        static public void Draw2DLines(Vector2[] points, Color color)
        {
            SetLineMaterial();
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int p = 0; p < (points.Length - 1); p++)
            {
                GL.Vertex3(points[p].x, points[p].y, 0);
                GL.Vertex3(points[p + 1].x, points[p + 1].y, 0);
            }

            GL.End();
        }

        static public void DrawBox(Rect r, GUIStyle style)
        {
            DrawBox(r, Style.GetStyleBackgroundColor(style));
        }

        static public void DrawBox(Rect r, Color color)
        {
            SetLineMaterial();
            lineMaterial.SetPass(0);

            Vector2 pointA;
            Vector2 pointB;
            Vector2 pointC;
            Vector2 pointD;

            pointA = new Vector2(r.x, r.y);
            pointB = new Vector2(r.x, r.yMax);
            pointC = new Vector2(r.xMax, r.yMax);
            pointD = new Vector2(r.xMax, r.y);

            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(pointA.x, pointA.y, 0);
            GL.Vertex3(pointB.x, pointB.y, 0);
            GL.Vertex3(pointB.x, pointB.y, 0);
            GL.Vertex3(pointC.x, pointC.y, 0);
            GL.Vertex3(pointC.x, pointC.y, 0);
            GL.Vertex3(pointD.x, pointD.y, 0);
            GL.Vertex3(pointD.x, pointD.y, 0);
            GL.Vertex3(pointA.x, pointA.y, 0);
            GL.End();
        }

        static public void DrawCircle(Vector2 center, float radius, int pointCount, GUIStyle style)
        {
            DrawCircle(center, radius, pointCount, Style.GetStyleBackgroundColor(style));
        }

        static public void DrawCircle(Vector2 center, float radius, int pointCount, Color color)
        {
            if (pointCount <= 0)
                return;

            SetLineMaterial();
            lineMaterial.SetPass(0);

            Vector2 [] points = new Vector2[pointCount];
            float angle = 0;

            for (int p=0; p<points.Length; p++)
            {
                angle = (360.0f / points.Length) * p * Mathf.Deg2Rad;

                points[p] = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }

            GL.Begin(GL.LINES);
            GL.Color(color);
            for (int p=0; p<(points.Length-1); p++)
            {
                GL.Vertex3(points[p].x, points[p].y, 0);
                GL.Vertex3(points[p+1].x, points[p+1].y, 0);
            }
            GL.Vertex3(points[points.Length - 1].x, points[points.Length - 1].y, 0);
            GL.Vertex3(points[0].x, points[0].y, 0);
            GL.End();
        }

        static private void SetLineMaterial()
        {
            if (!lineMaterial)
            {
                /* Credit:  */
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
               "SubShader { Pass {" +
               "   BindChannels { Bind \"Color\",color }" +
               "   Blend SrcAlpha OneMinusSrcAlpha" +
               "   ZWrite Off Cull Off Fog { Mode Off }" +
               "} } }");
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        static public void DrawBox(Rect rect, GUIStyle style, bool border)
        {
            DrawBox(rect, style, GUIContent.none, border);
        }

        static public void DrawBox(Rect rect, GUIStyle style, GUIContent content, bool border)
        {
            if (border)
            {
                GUI.Box(rect, content, style);

                //Style.PushBackgroundColor(style);
                //GUI.Box(rect, content);
                //Style.PopBackgroundColor();
            }
            else
            {
                GUI.Box(rect, content, style);
            }
        }

        static public Vector2 UpperLeftCorner(Rect rect)
        {
            return new Vector2(rect.x, rect.y);
        }

        static public void DrawVerticalLine(Vector2 point, float length, float width, GUIStyle style)
        {
            DrawBox(new Rect(point.x, point.y, width, length), style, false);
        }

        static public void DrawHorizontalLine(Vector2 point, float length, float width, GUIStyle style)
        {
            DrawBox(new Rect(point.x, point.y, length, width), style, false);
        }


        static public void LoadTexture(ref Texture2D texture, string resourceName, int width, int height)
        {
            if (texture == null)
            {
                texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

                Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SmoothMoves_Editor.EmbeddedResources." + resourceName);

                if (myStream != null)
                {
                    texture.LoadImage(GUIHelper.ReadToEnd(myStream));
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
    }
}
