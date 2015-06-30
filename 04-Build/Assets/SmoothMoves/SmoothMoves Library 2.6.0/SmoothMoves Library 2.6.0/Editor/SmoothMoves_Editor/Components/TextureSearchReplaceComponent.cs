using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    public delegate void SearchAtlasChangedDelegate(TextureSelectComponent source, TextureAtlas searchAtlas);
    public delegate void SearchTextureChangedDelegate(TextureSelectComponent source, string searchTextureGUID);

    public delegate void ReplaceAtlasChangedDelegate(TextureSelectComponent source, TextureAtlas replaceAtlas);
    public delegate void ReplaceTextureChangedDelegate(TextureSelectComponent source, string replaceTextureGUID);
    public delegate void ReplacePivotOffsetChangedDelegate(TextureSelectComponent source, Vector2 replacePivotOffset, bool replaceUseDefaultPivot);

	public class TextureSearchReplaceComponent
	{
		private TextureSelectComponent _searchComponent;
		private TextureSelectComponent _replaceComponent;
		
		private SearchAtlasChangedDelegate _searchAtlasChangedDelegate;
		private SearchTextureChangedDelegate _searchTextureChangedDelegate;
		private ReplaceAtlasChangedDelegate _replaceAtlasChangedDelegate;
		private ReplaceTextureChangedDelegate _replaceTextureChangedDelegate;
		private ReplacePivotOffsetChangedDelegate _replacePivotOffsetChangedDelegate;

		public int index;
		
		public TextureSearchReplaceComponent(int idx, GameObject baseObject)
		{
			index = idx;
		
            _searchComponent = new TextureSelectComponent(index, false);
			_searchComponent.SetAtlasChangedDelegate(baseObject, SetSearchAtlas);
			_searchComponent.SetTextureChangedDelegate(SetSearchTextureGUID);
			
            _replaceComponent = new TextureSelectComponent(index, true);
			_replaceComponent.SetAtlasChangedDelegate(baseObject, SetReplaceAtlas);
			_replaceComponent.SetTextureChangedDelegate(SetReplaceTextureGUID);
			_replaceComponent.SetPivotOffsetChangedDelegate(SetReplacePivotOffset);
		}

        //public void OnEnable()
        //{
        //    Resources.OnEnable();
        //}		
		
		public void OnGUI()
		{
			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal(Style.antiSelectionStyle);
			
			GUILayout.BeginVertical();
			GUILayout.Label("Search For:");
			_searchComponent.OnGUI();
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			GUILayout.Space(50.0f);
			GUILayout.Label(Resources.arrow);
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			GUILayout.Label("Replace With:");
			_replaceComponent.OnGUI();
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5.0f);
			
			GUILayout.EndVertical();
		}
		
		public void SetSearchAtlasChangedDelegate(SearchAtlasChangedDelegate searchAtlasChangedDelegate)
		{
			_searchAtlasChangedDelegate = searchAtlasChangedDelegate;
		}
		
		public void SetSearchTextureChangedDelegate(SearchTextureChangedDelegate searchTextureChangedDelegate)
		{
			_searchTextureChangedDelegate = searchTextureChangedDelegate;
		}
		
		public void SetReplaceAtlasChangedDelegate(ReplaceAtlasChangedDelegate replaceAtlasChangedDelegate)
		{
			_replaceAtlasChangedDelegate = replaceAtlasChangedDelegate;
		}
		
		public void SetReplaceTextureChangedDelegate(ReplaceTextureChangedDelegate replaceTextureChangedDelegate)
		{
			_replaceTextureChangedDelegate = replaceTextureChangedDelegate;
		}
		
		public void SetReplacePivotOffsetChangedDelegate(ReplacePivotOffsetChangedDelegate replacePivotOffsetChangedDelegate)
		{
			_replacePivotOffsetChangedDelegate = replacePivotOffsetChangedDelegate;
		}
		
		
		
	    public void SetSearchAtlas(TextureAtlas atlas)
	    {
			_searchComponent.SetAtlas(atlas);
		}
		
	    public void SetSearchAtlas(TextureSelectComponent textureSelectComponent, TextureAtlas atlas)
	    {
			if (_searchAtlasChangedDelegate != null)
			{
				_searchAtlasChangedDelegate(textureSelectComponent, atlas);
			}
	    }
	
	    public void SetSearchTextureGUID(string textureGUID)
	    {
			_searchComponent.SetTextureGUID(textureGUID);
		}
		
	    public void SetSearchTextureGUID(TextureSelectComponent textureSelectComponent, string textureGUID)
	    {
			if (_searchTextureChangedDelegate != null)
			{
				_searchTextureChangedDelegate(textureSelectComponent, textureGUID);
			}
	    }
	
	    public void SetReplaceAtlas(TextureAtlas atlas)
	    {
			_replaceComponent.SetAtlas(atlas);
		}
		
	    public void SetReplaceAtlas(TextureSelectComponent textureSelectComponent, TextureAtlas atlas)
	    {
			if (_replaceAtlasChangedDelegate != null)
			{
				_replaceAtlasChangedDelegate(textureSelectComponent, atlas);
			}
	    }
	
	    public void SetReplaceTextureGUID(string textureGUID)
	    {
			_replaceComponent.SetTextureGUID(textureGUID);
		}
		
	    public void SetReplaceTextureGUID(TextureSelectComponent textureSelectComponent, string textureGUID)
	    {
			if (_replaceTextureChangedDelegate != null)
			{
				_replaceTextureChangedDelegate(textureSelectComponent, textureGUID);
			}
	    }

		public void SetReplacePivotOffset(Vector2 pivotOffset, bool useDefaultPivot)
	    {
            _replaceComponent.SetPivotOffset(pivotOffset, useDefaultPivot);
		}
		
		public void SetReplacePivotOffset(TextureSelectComponent textureSelectComponent, Vector2 pivotOffset, bool useDefaultPivot)
	    {
			if (_replacePivotOffsetChangedDelegate != null)
			{
				_replacePivotOffsetChangedDelegate(textureSelectComponent, pivotOffset, useDefaultPivot);
			}
	    }		
	}
}
