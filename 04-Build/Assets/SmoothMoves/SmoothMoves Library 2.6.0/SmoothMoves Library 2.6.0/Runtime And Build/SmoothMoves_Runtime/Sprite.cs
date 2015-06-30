using UnityEngine;

namespace SmoothMoves
{
    /// <summary>
    /// This is a simple class to show 4 vertex quads with dynamic batching among shared materials
    /// </summary>
    public class Sprite : MonoBehaviour
    {
        [HideInInspector]
        public MeshRenderer _meshRenderer;
        [HideInInspector]
        public MeshFilter _meshFilter;
        [HideInInspector]
        public Mesh _mesh;

        [HideInInspector]
        public Vector2 _halfSize;
        [HideInInspector]
        public GameObject _meshGameObject;
        [HideInInspector]
        public int _textureIndex;

        [HideInInspector]
        public Vector2 _topLeft;
        [HideInInspector]
        public Vector2 _topRight;
        [HideInInspector]
        public Vector2 _bottomLeft;
        [HideInInspector]
        public Vector2 _bottomRight;

        [HideInInspector]
        public Vector3[] _vertices;
        [HideInInspector]
        public Color[] _colors;
        [HideInInspector]
        public Vector2[] _uvs;
        [HideInInspector]
        public int[] _triangles;
        [HideInInspector]
        public Vector3[] _normals;

        /// <summary>
        /// Possible pivot points for the sprite's pivot offset
        /// </summary>
        public enum PIVOT_PRESET
        {
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        /// <summary>
        /// The possible ways the sprite can be resized
        /// </summary>
        public enum SIZE_MODE
        {
            /// <summary>
            /// Scaled relative to the original sprite dimensions
            /// </summary>
            RelativeScale = 0,

            /// <summary>
            /// Set in absolute pixels
            /// </summary>
            Absolute = 1
        }

        [HideInInspector]
        public TextureAtlas atlas;
        [HideInInspector]
        public string textureGUID;
        [HideInInspector]
        public SIZE_MODE sizeMode = SIZE_MODE.RelativeScale;
        [HideInInspector]
        public Vector2 size = Vector2.zero;
        [HideInInspector]
        public Vector2 scale = Vector2.one;
        [HideInInspector]
        public Vector2 pivotOffsetOverride;
        [HideInInspector]
        public Color color = Color.white;
        [HideInInspector]
        public bool useDefaultPivot;

        void Awake()
        {
            Initialize();
        }

        public void Dispose()
        {
            if (_mesh != null)
            {
                Clear();
                _mesh = null;
            }
        }

        public void Initialize()
        {
            if (_vertices == null)
            {
                _vertices = new Vector3[4];
            }
            else if (_vertices.Length == 0)
            {
                _vertices = new Vector3[4];
            }

            if (_colors == null)
            {
                _colors = new Color[4];
            }
            else if (_colors.Length == 0)
            {
                _colors = new Color[4];
            }

            if (_uvs == null)
            {
                _uvs = new Vector2[4];
            }
            else if (_uvs.Length == 0)
            {
                _uvs = new Vector2[4];
            }

            if (_triangles == null)
            {
                _triangles = new int[6];
            }
            else if (_triangles.Length == 0)
            {
                _triangles = new int[6];
            }

            if (_normals == null)
            {
                _normals = new Vector3[4];
                _normals[0] = Vector3.back;
                _normals[1] = Vector3.back;
                _normals[2] = Vector3.back;
                _normals[3] = Vector3.back;
            }
            else if (_normals.Length == 0)
            {
                _normals = new Vector3[4];
                _normals[0] = Vector3.back;
                _normals[1] = Vector3.back;
                _normals[2] = Vector3.back;
                _normals[3] = Vector3.back;
            }

            if (_mesh == null)
            {
                _meshGameObject = gameObject;

                if (_meshGameObject != null)
                {
                    _meshFilter = _meshGameObject.GetComponent<MeshFilter>();
                    if (_meshFilter == null)
                    {
                        _meshFilter = _meshGameObject.AddComponent<MeshFilter>();
                        _meshFilter.sharedMesh = new Mesh();
                    }
                    else if (_meshFilter.sharedMesh == null)
                    {
                        _meshFilter.sharedMesh = new Mesh();
                    }

                    _mesh = _meshFilter.sharedMesh;

                    if (_mesh.vertices.Length == 0)
                    {
                        _mesh.vertices = _vertices;
                        _mesh.uv = _uvs;
                        _mesh.colors = _colors;
                        _mesh.triangles = _triangles;
                        _mesh.normals = _normals;
                        _mesh.RecalculateBounds();
                        _mesh.RecalculateNormals();
                    }

                    _meshRenderer = _meshGameObject.GetComponent<MeshRenderer>();
                    if (_meshRenderer == null)
                    {
                        _meshRenderer = _meshGameObject.AddComponent<MeshRenderer>();
                    }
                }
            }

            if (_textureIndex == -1)
            {
                if (atlas != null)
                {
                    _textureIndex = atlas.GetTextureIndex(textureGUID);
                }
            }
        }

        public void DisconnectInstance()
        {
            _meshGameObject = gameObject;

            if (_meshGameObject != null)
            {
                _meshFilter = _meshGameObject.GetComponent<MeshFilter>();
                if (_meshFilter == null)
                {
                    _meshFilter = _meshGameObject.AddComponent<MeshFilter>();
                    _meshFilter.sharedMesh = new Mesh();
                }

                _meshFilter.sharedMesh = new Mesh();
                _mesh = _meshFilter.sharedMesh;

                _mesh.vertices = _vertices;
                _mesh.uv = _uvs;
                _mesh.colors = _colors;
                _mesh.triangles = _triangles;
                _mesh.normals = _normals;
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
            }
        }

        public void Copy(Sprite otherSprite, bool updateMesh)
        {
            _halfSize = otherSprite._halfSize;
            _textureIndex = otherSprite._textureIndex;

            _topLeft = otherSprite._topLeft;
            _topRight = otherSprite._topRight;
            _bottomLeft = otherSprite._bottomLeft;
            _bottomRight = otherSprite._bottomRight;

            _vertices = otherSprite._vertices;
            _colors = otherSprite._colors;
            _uvs = otherSprite._uvs;
            _triangles = otherSprite._triangles;
            _normals = otherSprite._normals;

            atlas = otherSprite.atlas;
            textureGUID = otherSprite.textureGUID;
            sizeMode = otherSprite.sizeMode;
            size = otherSprite.size;
            scale = otherSprite.scale;
            pivotOffsetOverride = otherSprite.pivotOffsetOverride;
            useDefaultPivot = otherSprite.useDefaultPivot;
            color = otherSprite.color;

            if (updateMesh)
            {
                _meshFilter = _meshGameObject.GetComponent<MeshFilter>();
                if (_meshFilter == null)
                {
                    _meshFilter = _meshGameObject.AddComponent<MeshFilter>();
                    _meshFilter.sharedMesh = new Mesh();
                }
                else if (_meshFilter.sharedMesh == null)
                {
                    _meshFilter.sharedMesh = new Mesh();
                }

                _mesh = _meshFilter.sharedMesh;

                _mesh.vertices = _vertices;
                _mesh.uv = _uvs;
                _mesh.colors = _colors;
                _mesh.triangles = _triangles;
                _mesh.normals = _normals;
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
            }
        }

        /// <summary>
        /// Sets the texure atlas of the sprite
        /// </summary>
        /// <param name="newAtlas">The reference to the texture atlas</param>
        public void SetAtlas(TextureAtlas newAtlas)
        {
            //Initialize();

            atlas = newAtlas;

            CalculateTextureIndex(textureGUID);

            SetDefaultSize();

            UpdateArrays();

            if (atlas != null)
                _meshRenderer.material = atlas.material;
            else
                _meshRenderer.material = null;
        }

        private void CalculateTextureIndex(string guid)
        {
            if (atlas == null)
            {
                _textureIndex = -1;
            }
            else
            {
                _textureIndex = atlas.GetTextureIndex(guid);
            }

            if (_textureIndex == -1)
            {
                textureGUID = "";
            }
        }

        private void SetDefaultSize()
        {
            if (atlas != null)
            {
                if (_textureIndex > -1 && _textureIndex < (atlas.textureSizes.Count))
                {
                    switch (sizeMode)
                    {
                        case SIZE_MODE.RelativeScale:
                            SetScale(scale.x, scale.y);
                            break;
                    }
                }
            }
        }

        public void SetTextureGUID(string guid)
        {
            textureGUID = guid;

            CalculateTextureIndex(textureGUID);

            SetDefaultSize();

            UpdateArrays();
        }

        /// <summary>
        /// Sets the texture name of the sprite
        /// </summary>
        /// <param name="textureName">The name of the texture</param>
        public void SetTextureName(string textureName)
        {
            if (atlas == null)
                return;

            SetTextureGUID(atlas.GetTextureGUIDFromName(textureName));
        }

        public string GetTextureName()
        {
            if (atlas == null)
                return "";

            return (atlas.GetNameFromTextureGUID(textureGUID));
        }

        /// <summary>
        /// Sets the way the sprite is resized, either with absolute pixel
        /// values, or by scale relative to the original dimensions
        /// </summary>
        /// <param name="newSizeMode">The new way the sprite is to be resized</param>
        public void SetSizeMode(SIZE_MODE newSizeMode)
        {
            sizeMode = newSizeMode;

            switch (sizeMode)
            {
                case SIZE_MODE.RelativeScale:
                    SetScale(scale);
                    break;

                case SIZE_MODE.Absolute:
                    SetSize(size);
                    break;
            }
        }

        /// <summary>
        /// Sets the size of the sprite
        /// </summary>
        /// <param name="newSize">2 Dimensional vector storing the width and height</param>
        public void SetSize(Vector2 newSize)
        {
            SetSize(newSize.x, newSize.y);
        }

        /// <summary>
        /// Sets the size of the sprite
        /// </summary>
        /// <param name="width">Width of the sprite</param>
        /// <param name="height">Height of the sprite</param>
        public void SetSize(float width, float height)
        {
            sizeMode = SIZE_MODE.Absolute;

            size.x = width;
            size.y = height;

            if (atlas == null)
            {
                scale.x = 1.0f;
                scale.y = 1.0f;
            }
            else if (atlas.textureGUIDs.Count == 0 || _textureIndex == -1 || _textureIndex > (atlas.textureSizes.Count - 1))
            {
                scale.x = 1.0f;
                scale.y = 1.0f;
            }
            else if (atlas.textureSizes[_textureIndex].x == 0 || atlas.textureSizes[_textureIndex].y == 0)
            {
                scale.x = 1.0f;
                scale.y = 1.0f;
            }
            else
            {
                scale.x = size.x / atlas.textureSizes[_textureIndex].x;
                scale.y = size.y / atlas.textureSizes[_textureIndex].y;
            }

            UpdateVertices();
        }

        /// <summary>
        /// Sets the scale of the sprite, relative to its original dimensions
        /// </summary>
        /// <param name="newScale">2 Dimensional scale for the width and height</param>
        public void SetScale(Vector2 newScale)
        {
            SetScale(newScale.x, newScale.y);
        }

        /// <summary>
        /// Sets the scale of the sprite, relative to its original dimensions
        /// </summary>
        /// <param name="x">The x scale of the sprite</param>
        /// <param name="y">The y scale of the sprite</param>
        public void SetScale(float x, float y)
        {
            sizeMode = SIZE_MODE.RelativeScale;

            scale.x = x;
            scale.y = y;

            if (atlas == null)
            {
                size.x = 1.0f;
                size.y = 1.0f;
            }
            else if (atlas.textureGUIDs.Count == 0 || _textureIndex == -1 || _textureIndex > (atlas.textureSizes.Count - 1))
            {
                size.x = 1.0f;
                size.y = 1.0f;
            }
            else if (atlas.textureSizes[_textureIndex].x == 0 || atlas.textureSizes[_textureIndex].y == 0)
            {
                size.x = 1.0f;
                size.y = 1.0f;
            }
            else
            {
                size.x = scale.x * atlas.textureSizes[_textureIndex].x;
                size.y = scale.y * atlas.textureSizes[_textureIndex].y;
            }

            UpdateVertices();
        }

        /// <summary>
        /// Sets the pivot point for the sprite to scale and rotate around, relative to the sprite's location
        /// </summary>
        /// <param name="newPivotOffset">Offset of the pivot point</param>
        /// <param name="useDefault">Use the default set in the texture atlas</param>
        public void SetPivotOffset(Vector2 newPivotOffset, bool useDefault)
        {
            if (!useDefaultPivot)
            {
                pivotOffsetOverride = newPivotOffset;
            }
            useDefaultPivot = useDefault;

            UpdateVertices();
        }

        /// <summary>
        /// Sets the color of the sprite
        /// </summary>
        /// <param name="newColor">New sprite color</param>
        public void SetColor(Color newColor)
        {
            color = newColor;

            UpdateColor();
        }

        private void UpdateVertices()
        {
            Vector2 pivotOffset;

            if (useDefaultPivot)
            {
                if (atlas != null)
                {
                    pivotOffset = atlas.LookupDefaultPivotOffset(_textureIndex);
                }
                else
                {
                    pivotOffset = Vector2.zero;
                }
            }
            else
            {
                pivotOffset = pivotOffsetOverride;
            }

            if (atlas == null)
            {
                _topLeft = Vector2.zero;
                _bottomLeft = Vector2.zero;
                _bottomRight = Vector2.zero;
                _topRight = Vector2.zero;
            }
            else if (atlas.textureSizes == null)
            {
                _topLeft = Vector2.zero;
                _bottomLeft = Vector2.zero;
                _bottomRight = Vector2.zero;
                _topRight = Vector2.zero;
            }
            else if (_textureIndex > (atlas.textureSizes.Count - 1) || _textureIndex == -1)
            {
                _topLeft = Vector2.zero;
                _bottomLeft = Vector2.zero;
                _bottomRight = Vector2.zero;
                _topRight = Vector2.zero;
            }
            else
            {
                CalculateHalfSize();

                _topLeft.x = (-pivotOffset.x * size.x) - _halfSize.x;
                _topLeft.y = (-pivotOffset.y * size.y) + _halfSize.y;

                _topRight.x = (-pivotOffset.x * size.x) + _halfSize.x;
                _topRight.y = _topLeft.y;

                _bottomLeft.x = _topLeft.x;
                _bottomLeft.y = (-pivotOffset.y * size.y) - _halfSize.y;

                _bottomRight.x = _topRight.x;
                _bottomRight.y = _bottomLeft.y;
            }

            _vertices[0].x = _topLeft.x;
            _vertices[0].y = _topLeft.y;
            _vertices[0].z = 0;

            _vertices[1].x = _bottomLeft.x;
            _vertices[1].y = _bottomLeft.y;
            _vertices[1].z = 0;

            _vertices[2].x = _bottomRight.x;
            _vertices[2].y = _bottomRight.y;
            _vertices[2].z = 0;

            _vertices[3].x = _topRight.x;
            _vertices[3].y = _topRight.y;
            _vertices[3].z = 0;

            _triangles[0] = 0;
            _triangles[1] = 3;
            _triangles[2] = 1;

            _triangles[3] = 3;
            _triangles[4] = 2;
            _triangles[5] = 1;

            if (_mesh != null)
            {
                _mesh.vertices = _vertices;
                _mesh.triangles = _triangles;
                _mesh.RecalculateNormals();
                _mesh.RecalculateBounds();
            }
        }

        private void CalculateHalfSize()
        {
            switch (sizeMode)
            {
                case SIZE_MODE.RelativeScale:
                    if (atlas == null)
                    {
                        _halfSize.x = size.x * 0.5f; ;
                        _halfSize.y = size.y * 0.5f;
                    }
                    else if (atlas.textureSizes == null)
                    {
                        _halfSize.x = size.x * 0.5f; ;
                        _halfSize.y = size.y * 0.5f;
                    }
                    else if (atlas.textureGUIDs.Count == 0 || _textureIndex == -1 || _textureIndex > (atlas.textureSizes.Count - 1))
                    {
                        _halfSize.x = size.x * 0.5f; ;
                        _halfSize.y = size.y * 0.5f;
                    }
                    else
                    {
                        _halfSize.x = atlas.textureSizes[_textureIndex].x * scale.x * 0.5f;
                        _halfSize.y = atlas.textureSizes[_textureIndex].y * scale.y * 0.5f;
                    }
                    break;

                case SIZE_MODE.Absolute:
                    _halfSize.x = size.x * 0.5f;
                    _halfSize.y = size.y * 0.5f;
                    break;
            }
        }

        private void UpdateUVs()
        {
            if (atlas == null)
            {
                _uvs[0] = Vector2.zero;
                _uvs[1] = Vector2.zero;
                _uvs[2] = Vector2.zero;
                _uvs[3] = Vector2.zero;
            }
            else if (atlas.textureGUIDs.Count == 0 || _textureIndex == -1 || _textureIndex > (atlas.uvs.Count - 1))
            {
                _uvs[0] = Vector2.zero;
                _uvs[1] = Vector2.zero;
                _uvs[2] = Vector2.zero;
                _uvs[3] = Vector2.zero;
            }
            else
            {
                _uvs[0].x = atlas.uvs[_textureIndex].x;
                _uvs[0].y = atlas.uvs[_textureIndex].y + atlas.uvs[_textureIndex].height;

                _uvs[1].x = atlas.uvs[_textureIndex].x;
                _uvs[1].y = atlas.uvs[_textureIndex].y;

                _uvs[2].x = atlas.uvs[_textureIndex].x + atlas.uvs[_textureIndex].width;
                _uvs[2].y = atlas.uvs[_textureIndex].y;

                _uvs[3].x = atlas.uvs[_textureIndex].x + atlas.uvs[_textureIndex].width;
                _uvs[3].y = atlas.uvs[_textureIndex].y + atlas.uvs[_textureIndex].height;
            }

            if (_mesh != null)
            {
                _mesh.uv = _uvs;
            }
        }

        private void UpdateColor()
        {
            _colors[0] = color;
            _colors[1] = color;
            _colors[2] = color;
            _colors[3] = color;

            if (_mesh != null)
            {
                _mesh.colors = _colors;
            }
        }

        public void UpdateArrays()
        {
            Clear();

            UpdateVertices();
            UpdateUVs();
            UpdateColor();
        }

        private void Clear()
        {
            if (_mesh != null)
                _mesh.Clear();
        }
    }
}
