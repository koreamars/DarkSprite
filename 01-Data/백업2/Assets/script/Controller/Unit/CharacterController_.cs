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

public class CharacterController_ : MonoBehaviour {

	public delegate void ShowCharacterEnd (byte index);
	private ShowCharacterEnd _showCharacterEnd = null;

	private static CharacterController instance;  
	private static GameObject container; 

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

	private GameObject[] charObjList = new GameObject[6];
	private byte _maxObjIndex = 0;
	private byte _currentIndex = 0;
	private Vector3 _prevPos;

	public static CharacterController GetInstance()  
	{  
		if( !instance )  
		{  
			container = new GameObject();  
			container.name = "CharacterController";  
			instance = container.AddComponent(typeof(CharacterController)) as CharacterController;  
		}  
		return instance;  
	} 

	public IEnumerator Init() {
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

	/**
	 * index에 해당하는 객체 정보를 반환함.
	 * */
	public GameObject GetCharacterView(byte index) {
		GameObject characterObj = charObjList [index];
		return characterObj;
	}

	/**
	 * 캐릭터 하나를 반환함.
	 * */
	public GameObject ShowCharacterView(CharacterModel model, ShowCharacterEnd endcall) {
		_showCharacterEnd = endcall;
		GameObject characterObj = Instantiate(Resources.Load ("character/Unit") as GameObject);

		charObjList [_maxObjIndex] = characterObj;
		_maxObjIndex += 1;

		CharacterView view = characterObj.GetComponent<CharacterView> ();
		view.unityFactory = _factory;
		view.Show (ShowEndCall);

		return characterObj;
	}

	private void ShowEndCall() {
		print ("ShowEndCall");
		if (_showCharacterEnd != null) {
			byte returnValue = (byte)(_maxObjIndex - 1);
			_showCharacterEnd (returnValue);
		}
	}

	public void UpdateCharacterView(byte index, CharacterModel model, ShowCharacterEnd endcall) {

		Texture2D texture1 = Resources.Load<Texture2D>("texture");
		//Texture2D texture2 = Resources.Load<Texture2D>("skirtTest");
		Texture2D texture2 = Resources.Load<Texture2D>("character/unitbody/type1/left-chest");
		Texture2D texture3 = Resources.Load<Texture2D>("character/unitbody/type1/right-chest");
		//texture3.Resize ((int)(texture3.width * 1f), (int)(texture3.height * 1f));
		//texture3.Resize (texture3.width, texture3.height);
		//texture3.Apply ();

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

		_currentIndex = index;
		GameObject characterObj = charObjList [index];
		_prevPos = characterObj.transform.position;
		characterObj.transform.position = new Vector3 (0, 0, 0);
		CharacterView view = characterObj.GetComponent<CharacterView> ();
		view.Show (UpdateEndCall);
	}

	private void UpdateEndCall() {
		GameObject characterObj = charObjList [_currentIndex];
		characterObj.transform.position = _prevPos;
		print ("UpdateEndCall");
	}

	/**
	 * 보유한 캐릭터 정보를 모두 정리함.
	 * 
	 **/
	public void Dispose() {

	}

	void Update ()
	{
		WorldClock.Clock.AdvanceTime (Time.deltaTime);
	}
}
