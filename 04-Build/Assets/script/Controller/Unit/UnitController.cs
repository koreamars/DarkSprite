// ------------------------------------------------------------------------------
//  케릭터 스케레톤 본을 관리 및 생성한다.
// ------------------------------------------------------------------------------
using System;
using System.IO;
using DragonBones;
using DragonBones.Factorys;
using DragonBones.Animation;
using DragonBones.Objects;
using DragonBones.Display;
using DragonBones.Textures;
using System.Collections;
using System.Collections.Generic;
using Com.Viperstudio.Utils;
using UnityEngine;

public class UnitController : MonoBehaviour {

	private TextAsset _jsonReader;
	private TextReader _reader;
	private Dictionary<String, System.Object> _skeletonRawData;
	private SkeletonData _skeletonData;
	private Texture _textures;
	private Dictionary<String, System.Object> _atlasRawData;
	private AtlasData _atlasData;
	private TextureAtlas _textureAtlas;
	private UnityFactory _factory;
	private Renderer _textureRenderer;

	public Armature currentArmature;

	public IEnumerator Init () {
		// 기본 드레곤 본 설정.
		_jsonReader = (TextAsset)Resources.Load("skeleton.json", typeof(TextAsset));
		_reader = new StringReader (_jsonReader.text);
		_skeletonRawData = Json.Deserialize (_reader) as Dictionary<String, System.Object>;
		_skeletonData = ObjectDataParser.ParseSkeletonData (_skeletonRawData);

		// 팩토리 설정.
		_factory = new UnityFactory ();
		_factory.AddSkeletonData (_skeletonData, "BaseUnitAni");

		// 텍스쳐 정보 설정.
		_textures = Resources.Load<Texture>("texture");
		_jsonReader = (TextAsset)Resources.Load("texture.json", typeof(TextAsset));
		_reader = new StringReader (_jsonReader.text);
		_atlasRawData = Json.Deserialize (_reader) as Dictionary<String, System.Object>;
		_atlasData = AtlasDataParser.ParseAtlasData (_atlasRawData);
		_textureAtlas = new TextureAtlas (_textures, _atlasData);
		
		_factory.AddTextureAtlas (_textureAtlas, "BaseUnitAni");



		yield return new WaitForEndOfFrame ();
	}

	public IEnumerator showArmature () {			

		if(currentArmature != null) {
			GameObject delObject = ((currentArmature.Display as UnityArmatureDisplay).Display as GameObject);
			Destroy(delObject);
		}
		currentArmature = _factory.BuildArmature ("BaseUnitAni", null, "BaseUnitAni", "BaseUnitAni");
		currentArmature.AdvanceTime (0f);
		WorldClock.Clock.Add (currentArmature);
		currentArmature.Animation.GotoAndPlay ("idle", -1, -1, 0);

		// 위치 보정.
		GameObject newObject = ((currentArmature.Display as UnityArmatureDisplay).Display as GameObject);
		newObject.name = "Armature";
		newObject.transform.parent = this.transform;
		newObject.transform.position = new Vector3 (0, 0, 0);

		yield return new WaitForEndOfFrame ();
	}

	public IEnumerator updateArmature() {

		Texture2D texture1 = Resources.Load<Texture2D>("texture");
		//Texture2D texture2 = Resources.Load<Texture2D>("skirtTest");
		Texture2D texture2 = Resources.Load<Texture2D>("left-chest");
		Texture2D texture3 = Resources.Load<Texture2D>("right-chest");

		//TextureData data = _textureAtlas.AtlasData.GetTextureData ("skirt");
		TextureData data1 = _textureAtlas.AtlasData.GetTextureData ("left-chest");
		TextureData data2 = _textureAtlas.AtlasData.GetTextureData ("right-chest");

		//Vector2 vector = new Vector2 (0, 0);
		Texture2D newTexture = null;
		Vector2 vector1 = new Vector2 (data1.X, 2048 - data1.Y - (int)texture2.height);
		newTexture = TextureMarge.SetTextureMarge (texture1, texture2, vector1);
		Vector2 vector2 = new Vector2 (data2.X, 2048 - data2.Y - (int)texture3.height);
		newTexture = TextureMarge.SetTextureMarge (newTexture, texture3, vector2);

		_textures = newTexture as Texture;
		//_textures = Resources.Load<Texture>("texture2");
		_textureAtlas = new TextureAtlas (_textures, _atlasData);
		
		_factory.AddTextureAtlas (_textureAtlas, "BaseUnitAni");

		yield return new WaitForEndOfFrame ();
	}

	public void setPosition(Vector3 vector) {
		if (currentArmature == null) return;
		((currentArmature.Display as UnityArmatureDisplay).Display as GameObject).transform.position = vector;
	}
}
