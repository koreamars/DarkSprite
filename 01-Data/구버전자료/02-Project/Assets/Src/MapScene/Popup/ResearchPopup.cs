using UnityEngine;
using System.Collections;

public class ResearchPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;
	public GameObject SlotField;
	public GameObject ScrollField;
	public GameObject MainThumbnail;
	public GameObject ResearchBtn;
	public GameObject CoreMakeBtn;
	public GameObject CoreMergingBtn;
	public GameObject DataViewTxt1;
	public GameObject DataViewTxt2;
	public GameObject DataViewTxt3;
	public GameObject DataViewTxt4;
	public GameObject DataViewTxt5;
	public GameObject DataViewTxt6;
	public GameObject DataViewTxt7;

	public Color ResearchBtnOutLineColor;
	public Color TitleFontColor;
	public Color TitleFontLineColor;

	private GameObject DataTitleTxt;
	private GameObject _GearDataUIObj;
	private GearDataUI _GearDataUI;

	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private GearData _GearData;
	private UserData _UserData;
	private ScriptData _ScriptData;

	private float _PrevMoveY = 0;
	private float _CurrentMoveY = 0;

	private bool _isMove;
	private int _CurrentIndex = 1;
	private GameObject[] _SlotList;

	private float _MaxYPoint = -13f;

	private bool _IsResearch;
	private bool _OnTime;
	private float _Timer = 0;
	private GameObject _RewardViewer;
	private GameObject _ListViewer;

	private Camera _UICamera;
	private Camera _ResearchCamera;

	//private GameObject _ChipSlot;
	private OutLineFont DataTitleFont;
	private GameObject _DarkGearPopup;

	void Awake() {
		SystemData.GetInstance();
		_UserData = UserData.getInstence();
		_GearData = GearData.getInstence();
		_ScriptData = ScriptData.getInstence();

		if(isTest) {
			init();
		}
	}


	public void init() {

		Camera[] cameraList = Camera.allCameras;
		foreach(Camera cameraObj in cameraList) {
			if(cameraObj.name == "UICamera") _UICamera = cameraObj;
			if(cameraObj.name == "ResearchCamera") _ResearchCamera = cameraObj;
		}

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);
		
		/*
		_Slot = Instantiate(Resources.Load<GameObject>("Common/ResearchSlot"), new Vector3(0f, -13f, 0f), SlotField.transform.rotation) as GameObject;
		_Slot.transform.parent = SlotField.transform;
		*/
		//ResearchCamera.enabled = false;

		/*
		GameObject dataTitleTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		DataTitleFont = dataTitleTxt.GetComponent<OutLineFont>();
		DataTitleFont.SetString("");
		DataTitleFont.SetAlign(TextAnchor.UpperLeft);
		dataTitleTxt.transform.parent = this.gameObject.transform;
		dataTitleTxt.transform.position = new Vector2(-6.98f, 0.52f);
		DataTitleFont.SetFontSize(22);
		DataTitleFont.SetLineSize(2);
		ResearchBtnOutLineColor.a = 1f;
		DataTitleFont.SetLineColor(ResearchBtnOutLineColor);
		*/

		_RewardViewer = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
		_RewardViewer.transform.parent = this.gameObject.transform; 

		_GearDataUIObj = Instantiate(Resources.Load<GameObject>("Common/GearDataUI")) as GameObject;
		_GearDataUIObj.transform.parent = this.gameObject.transform; 
		_GearDataUIObj.transform.localScale = new Vector2(0.78f, 0.78f);
		_GearDataUIObj.transform.position = new Vector2(-5.98f, -1.11f);
		_GearDataUI = _GearDataUIObj.GetComponent<GearDataUI>();
		_GearDataUI.init();
		
		Research[] researchList = _GearData.GetResearchList();
		_SlotList = new GameObject[researchList.Length];
		GameObject slot;
		float yPoint = -13f;
		float xPoint = 1f;
		byte prevStep = 0;
		float slotFieldHeight = 0f;
		int index = 1;
		foreach(Research research in researchList) {
			
			//if(index > 5) continue;
			
			yPoint = -13f + ((research.unlockStep - 1) * 3);
			if(prevStep == research.unlockStep) {
				xPoint += 2.2f;
			} else {
				xPoint = 1f - (_GearData.GetStepCount(research.unlockStep) * 1.1f);
				slotFieldHeight += 3.3f;
			}
			prevStep = research.unlockStep;
			slot = Instantiate(Resources.Load<GameObject>("Common/ResearchSlot"), new Vector3(xPoint, yPoint, 0f), SlotField.transform.rotation) as GameObject;
			slot.GetComponent<ResearchSlot>().init(index, research, 2);
			slot.GetComponent<ResearchSlot>().SetClick(OnMouseUp);
			slot.GetComponent<ResearchSlot>().SetDragCallBack(OnMouseDrag);
			slot.transform.parent = SlotField.transform;
			_SlotList[index - 1] = slot;
				
			index ++;
		}

		Transform firstSlotObj = _SlotList[0].transform;
		Vector3 chipSlotPos = new Vector3(firstSlotObj.position.x + 2.5f, firstSlotObj.position.y, firstSlotObj.position.z);

		// 코어 버튼.
		CoreMakeBtn.GetComponent<CommonBtn>().Init(0, ScriptData.getInstence().GetGameScript(160152).script, 10, Color.white);
		CoreMakeBtn.GetComponent<CommonBtn>().SetClick(OnSceneChange);
		if(UserData.getInstence().StoryStepId < 18) CoreMakeBtn.GetComponent<CommonBtn>().SetEnabled(false);

		// 코어머징 버튼.
		if(_UserData.StoryStepId >= 34) {

			string chValue = "1";
			string checkValue =  "0";
			print("_UserData.SingleCountValues : " + _UserData.SingleCountValues);
			char[] values = _UserData.SingleCountValues.ToCharArray();
			if(values[0] == (char)(checkValue[0])) StoryData.getInstence().UpdateStoryStep(60003);
			values[0] = (char)(chValue[0]);
			string newValues = "";
			newValues = new string(values);
			_UserData.SingleCountValues = newValues;
			LocalData.getInstence().UserResourceSave();

			CoreMergingBtn.GetComponent<CommonBtn>().Init(0, ScriptData.getInstence().GetGameScript(160159).script, 10, Color.white);
			CoreMergingBtn.GetComponent<CommonBtn>().SetClick(OnCoreMerging);
			CoreMergingBtn.GetComponent<CommonBtn>().SetEnabled(true);
		} else {
			CoreMergingBtn.GetComponent<CommonBtn>().SetEnabled(false);
		}

		// 코어 칩 변환.
		/*
		Research chipResearch = new Research();
		chipResearch.id = 0;
		chipResearch.gearId = 0;

		_ChipSlot = Instantiate(Resources.Load<GameObject>("Common/ResearchSlot"), chipSlotPos, Quaternion.identity) as GameObject;
		_ChipSlot.GetComponent<ResearchSlot>().init(0, chipResearch, 2);
		_ChipSlot.GetComponent<ResearchSlot>().SetClick(OnMouseUp);
		_ChipSlot.GetComponent<ResearchSlot>().SetDragCallBack(OnMouseDrag);
		_ChipSlot.transform.parent = SlotField.transform;
		*/
		_MaxYPoint = (slotFieldHeight * -1) - 2f;
		
		BoxCollider2D collider = ScrollField.GetComponent<BoxCollider2D>();
		collider.size = new Vector2(4.7f, slotFieldHeight);
		collider.center = new Vector2(0, (slotFieldHeight / 2) - (slotFieldHeight / 5));
		
		CommonBtn commonBtn = ScrollField.GetComponent<CommonBtn>();
		commonBtn.btnId = -1;
		commonBtn.SetDragCallBack(OnMouseDrag);
		commonBtn.SetClick(OnMouseUp);
		
		ResearchBtn.GetComponent<CommonBtn>().SetClick(OnResearchBtn);
		ResearchBtnOutLineColor.a = 1;
		ResearchBtn.GetComponent<CommonBtn>().SetTxtColor(Color.white);
		ResearchBtn.GetComponent<CommonBtn>().FontOutLineColor = ResearchBtnOutLineColor;
		if(_UserData.GetIsResearching() > 0) _IsResearch = true;

		byte currentResearchId = _UserData.GetIsResearching();

		byte slotIndex = 1;
		float firstYpos = SlotField.transform.position.y;
		byte slotPrevStep = 0;
		foreach(GameObject slotObject in _SlotList) {
			Research checkResearch = slotObject.GetComponent<ResearchSlot>().GetResearch();
			if(slotPrevStep != checkResearch.unlockStep) firstYpos -= 2f;
			if(currentResearchId == 0) 
			{
				UserResearch userResearch = UserData.getInstence().GetUserResearchByReserch(checkResearch);
				if(userResearch == null || userResearch.isComplete == false) {
					slotIndex -= 1;
					break;
				}
			} else {
				if(checkResearch.id == currentResearchId) {
					break;
				}
			}

			slotIndex ++;
		}
		if(slotIndex == 0) slotIndex = 1;

		SelectSlot(slotIndex);

		SlotField.transform.position = new Vector3(0f, firstYpos, 0f);

		/*
		DataTitleTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;

		DataTitleTxt.name = "titleTxt";
		DataTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(110002).script);
		DataTitleTxt.GetComponent<OutLineFont>().SetFontSize(24);
		DataTitleTxt.GetComponent<OutLineFont>().SetLineSize(1.5f);
		DataTitleTxt.GetComponent<OutLineFont>().SetAlign(TextAnchor.MiddleLeft);
		DataTitleTxt.transform.parent = this.gameObject.transform;
		DataTitleTxt.transform.position = new Vector2(-0.48f, 4.19f);
		TitleFontColor.a = 1;
		DataTitleTxt.GetComponent<OutLineFont>().SetFontColor(TitleFontColor);
		TitleFontLineColor.a = 1;
		DataTitleTxt.GetComponent<OutLineFont>().SetLineColor(TitleFontLineColor);
		*/
	}

	/** 씬을 번경함 */
	private void OnSceneChange(int id) {
		if(id == 0) {
			if(_CurrentIndex != 0) GuideArrowManager.getInstence().ShowArrow(SceneType.ResearchPopup_CoreMake);
		} else {
			if(_CurrentIndex == 0) GuideArrowManager.getInstence().ShowArrow(SceneType.ResearchPopup);
		}

		SelectSlot(id);
	}

	/** 코어머징을 시작 합니다. */
	private void OnCoreMerging(int id) {

		if(_UICamera != null) _UICamera.enabled = false;
		if(_ResearchCamera != null) _ResearchCamera.enabled = false;

		_UserData.GetMakeGearList();

		_ListViewer = Instantiate(Resources.Load<GameObject>("Common/ListViewer")) as GameObject;
		_ListViewer.GetComponent<ListViewer>().Init(ListViewer.UserGearType, OnDarkGearSelect, OnCloseDarkListViewer);

	}

	/** 다크 기어 창 닫기. */
	private void OnCloseDarkListViewer() {
		print("OnCloseDarkListViewer");
		_ListViewer.GetComponent<ListViewer>().DestoryObject();
		Destroy(_ListViewer);

		if(_UICamera != null) _UICamera.enabled = true;
		if(_ResearchCamera != null) _ResearchCamera.enabled = true;

	}

	/** 다크 기어 선택 */
	private void OnDarkGearSelect(int id) {
		/*
		Gear gear = _GearData.GetGearByID((short)(id));

		BillModel billmodel = new BillModel();
		billmodel.core = gear.makeResource;
		billmodel.corechip = 2;
		billmodel.money = gear.makeCost;
		_UserData.SetConfirmPop(ScriptData.getInstence().GetGameScript(150156).script, OnDarkGearPopConfirm, billmodel);
		*/
		OnCloseDarkListViewer();
		_DarkGearPopup = Instantiate(Resources.Load<GameObject>("Common/DarkGearPopup")) as GameObject;
		_DarkGearPopup.name = "DarkGearPopup";
		_DarkGearPopup.GetComponent<DarkGearPop>().init((short)(id), 2, DarkGearPop.BaseType);
	}

	void Update() {
		if(_IsResearch) {

			_Timer += Time.deltaTime;
			if(_Timer > 1) {
				if(_UserData.GetIsResearching() == 0) {
					_IsResearch = false;
					UpdateAllSlot();
				}
				SelectSlot(_CurrentIndex);
				_Timer = 0;
			}
		}

		ResearchSlot slotObjCS;

		foreach(GameObject slotObj in _SlotList) {
			slotObjCS = slotObj.GetComponent<ResearchSlot>();
			ArrayList prevSlotList = GetPrevStepSlotList((byte)(slotObjCS.GetResearch().unlockStep - 1));
			if(prevSlotList.Count > 0) {
				LineRenderer slotLineRenderer = slotObj.GetComponent<LineRenderer>();
				slotLineRenderer.SetVertexCount(prevSlotList.Count * 2);
				slotLineRenderer.SetWidth(0.1f, 0.1f);
				GameObject prevObj = null;
				int lineCount = 0;
				for(int lineIndex = 0; lineIndex < prevSlotList.Count; lineIndex ++) {
					prevObj = prevSlotList[lineIndex] as GameObject;
					if(slotObjCS.GetResearch().unlockLevel <= _UserData.ResearchLevel) {
						slotLineRenderer.SetColors(Color.white, Color.white);
					} else {
						slotLineRenderer.SetColors(Color.gray, Color.gray);
					}
					slotLineRenderer.SetPosition(lineCount, slotObj.transform.position);
					lineCount ++;
					slotLineRenderer.SetPosition(lineCount, prevObj.transform.position);
					lineCount ++;
				}
			}

		}
	}

	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}

	private ArrayList GetPrevStepSlotList(byte prevStep) {
		ArrayList prevStepList = new ArrayList();

		foreach(GameObject slotObj in _SlotList) {
			if(slotObj.GetComponent<ResearchSlot>().GetResearch().unlockStep == prevStep) prevStepList.Add(slotObj);
		}

		return prevStepList;
	}

	private void OnMouseDrag(int id, Vector2 moveVector) {
		if(moveVector.y > 2) _isMove = true;
		if(_PrevMoveY == 0) {
			_PrevMoveY = SlotField.transform.position.y; 
		}
		_CurrentMoveY = moveVector.y / 60;
		float thisMoveY = _PrevMoveY - _CurrentMoveY;
		if(thisMoveY > -9f) {
			thisMoveY = -9f; 
			return;
		}
		if(thisMoveY < _MaxYPoint) {
			thisMoveY = _MaxYPoint; 
			return;
		}
		SlotField.transform.position = new Vector2(0, thisMoveY);
	}

	private void OnMouseUp(int id) {

		_PrevMoveY = 0;
		_CurrentMoveY = 0;
		if(id >= 0 && _isMove == false) {
			OnSceneChange(id);
		}

		_isMove = false;
	}
		
	private void OnPopupClose() {
		print("OnPopupClose");
		if(_CloseCallback != null) _CloseCallback();
	}

	private void SelectSlot(int index) {

		//DataTitleFont.SetString("");
		DataViewTxt1.transform.position = new Vector2(-4.87f, -0.23f);
		DataViewTxt1.GetComponent<TextMesh>().text = "";
		DataViewTxt2.GetComponent<TextMesh>().text = "";
		DataViewTxt3.GetComponent<TextMesh>().text = "";
		DataViewTxt4.GetComponent<TextMesh>().text = "";
		DataViewTxt5.GetComponent<TextMesh>().text = "";
		DataViewTxt6.GetComponent<TextMesh>().text = "";
		DataViewTxt7.GetComponent<TextMesh>().text = "";

		if(_CurrentIndex > 0 && _SlotList.Length >= _CurrentIndex) {
			GameObject prevObj = _SlotList[_CurrentIndex - 1];
			prevObj.GetComponent<ResearchSlot>().SelectSlot(false);
		}
		MainThumbnail.renderer.enabled = false;
		ResearchBtn.transform.position = new Vector3(-4.05f, -20f, 0);

		//_ChipSlot.GetComponent<ResearchSlot>().SelectSlot(false);

		Color btnColor = Color.white;

		_CurrentIndex = index;

		Transform CurrentChipViewerT = this.gameObject.transform.FindChild("CurrentChipViewer") as Transform;
		if(CurrentChipViewerT) Destroy(CurrentChipViewerT.gameObject);

		BillModel billModel = new BillModel();
		if(_CurrentIndex > 0) {
			_GearDataUI.GearUpdate(0);
			if(_SlotList.Length < index) return;
			GameObject slotObj = _SlotList[index - 1];
			Research currentReserch = slotObj.GetComponent<ResearchSlot>().GetResearch();
			if(_UserData.ResearchLevel < currentReserch.unlockLevel) {
				_RewardViewer.GetComponent<RewardViewer>().init(new BillModel(), 10, false);
				DataViewTxt1.transform.position = new Vector2(-6.84f, -0.24f);
				DataViewTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(150150).script;
				return;
			}

			MainThumbnail.renderer.enabled = true;
			slotObj.GetComponent<ResearchSlot>().SelectSlot(true);

			Gear currentGear = _GearData.GetGearByID(currentReserch.gearId);
			//DataTitleFont.SetString(ScriptData.getInstence().GetGameScript(currentGear.scriptId).script);
			
			SpriteRenderer renderer = (SpriteRenderer)MainThumbnail.GetComponent ("SpriteRenderer");
			//renderer.sprite = Resources.Load<Sprite>("UnitResource/Thumbnail/" + currentGear.thumbnailURI);
			renderer.sprite = Resources.Load<Sprite>("UnitResource/Thumbnail/");

			long time = (currentReserch.researchTime * SystemData.GetInstance().millisecondNum);

			_GearDataUI.GearUpdate(currentGear.id);

			/*
			switch(currentGear.gearType) {
			case GearType.Body:
				//DataViewTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160116).script + " : " + currentGear.addIA;
				//DataViewTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160118).script + " : " + currentGear.addHP;
				//DataViewTxt3.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160119).script + " : " + currentGear.addMP;
				break;
			case GearType.Suit:
				//DataViewTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160117).script + " : " + currentGear.spendIA;
				//DataViewTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160118).script + " : " + currentGear.addHP;
				//DataViewTxt3.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160119).script + " : " + currentGear.addMP;
				break;
			case GearType.Engine:
				//DataViewTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160117).script + " : " + currentGear.spendIA;
				//DataViewTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160120).script + " : " + currentGear.spendMP;
				break;
			case GearType.Weapon_Gun:
			case GearType.Weapon_Missle:
			case GearType.Weapon_Rocket:
				//DataViewTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160121).script + " : " + currentGear.minAP + "~" + currentGear.maxAP;
				//DataViewTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160122).script + " : " + currentGear.minIAD + "~" + currentGear.maxIAD;
				string ammostr;
				if(currentGear.ammo < 0) {
					ammostr = _ScriptData.GetGameScript(160124).script;
				} else {
					ammostr = currentGear.ammo + "";
				}
				//DataViewTxt3.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160125).script + " : " + ammostr;
				//DataViewTxt4.GetComponent<TextMesh>().text = "개발비용 : " + currentReserch.researchCost;
				//DataViewTxt5.GetComponent<TextMesh>().text = "소요시간 : " + SystemData.GetInstance().GetTimeStrByTime(time);
				
				break;
			}
			*/
			UserResearch userResearch = _UserData.GetUserResearchByReserch(currentReserch);
			if(_IsResearch) {	// 개발 중.
				if(userResearch != null) {
					if(!userResearch.isComplete) {
						long lastTime = (userResearch.startTime + (currentReserch.researchTime * SystemData.GetInstance().millisecondNum)) - SystemData.GetInstance().getCurrentTime();
						DataViewTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160109).script + " : " + SystemData.GetInstance().GetTimeStrByTime(lastTime);
					} else {
						DataViewTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(150107).script;
					}
					
				} else {
					DataViewTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160129).script + " : " + SystemData.GetInstance().GetTimeStrByTime(time);
				}
				ResearchBtn.transform.position = new Vector3(-4.15f, -20f, 0);
				
			} else {	// 개발 대기.
				if(userResearch == null) {
					DataViewTxt4.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160130).script + " : ";
					DataViewTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160129).script + " : " + SystemData.GetInstance().GetTimeStrByTime(time);
					ResearchBtn.transform.position = new Vector3(-4.05f, -4.15f, 0);
					ResearchBtnOutLineColor.a = 1f;
					ResearchBtn.GetComponent<CommonBtn>().FontOutLineColor = ResearchBtnOutLineColor;
					ResearchBtn.GetComponent<CommonBtn>().Init(currentReserch.id, _ScriptData.GetGameScript(130300).script, 1, btnColor);
					billModel.money = currentReserch.researchCost;
					billModel.moneyPlus = false;
					billModel.core = currentReserch.coreCost;
					billModel.corePlus = false;
				} else {
					DataViewTxt4.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(150107).script;
					//DataViewTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(150107).script;
					ResearchBtn.transform.position = new Vector3(-4.15f, -20f, 0);

				}
				
			}
			_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
			_RewardViewer.GetComponent<RewardViewer>().init(billModel, 10, false);
			_RewardViewer.transform.position = new Vector3(-4.56f, -2.6f, 0f);
			_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 1f);


		} else {

			_GearDataUI.GearUpdate(0);

			MainThumbnail.renderer.enabled = true;

			SpriteRenderer renderer = (SpriteRenderer)MainThumbnail.GetComponent ("SpriteRenderer");
			renderer.sprite = Resources.Load<Sprite>("MainScene/Research/Research-CoreMake");

			//DataTitleFont.SetString(ScriptData.getInstence().GetGameScript(160152).script);
			DataViewTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(150124).script;
			DataViewTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160130).script + " : ";
			DataViewTxt3.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160128).script + " : ";
			//DataViewTxt3.GetComponent<TextMesh>().text = "";

			billModel.corechip = SystemData.GetInstance().CoreMakeChipCount;
			billModel.corechipPlus = false;

			ResearchBtn.transform.position = new Vector3(-4.05f, -4.15f, 0);
			ResearchBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(160152).script, 1, btnColor);
			//160128
			//_ChipSlot.GetComponent<ResearchSlot>().SelectSlot(true);

			_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
			_RewardViewer.GetComponent<RewardViewer>().init(billModel, 10, false);
			_RewardViewer.transform.position = new Vector3(-2.99f, -0.83f, 0f);
			_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

			BillModel totalBill = new BillModel();
			totalBill.corechip = _UserData.UserChips;
			totalBill.corechipPlus = true;

			GameObject CurrentChipViewer = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
			CurrentChipViewer.transform.parent = this.gameObject.transform;
			CurrentChipViewer.name = "CurrentChipViewer";
			CurrentChipViewer.transform.localScale = new Vector3(1f, 1f, 1f);
			CurrentChipViewer.GetComponent<RewardViewer>().init(totalBill, 10, false);
			CurrentChipViewer.transform.position = new Vector3(-3.48f, -1.18f, 0f);
			CurrentChipViewer.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

		}

	}

	private void UpdateAllSlot() {
		foreach(GameObject obj in _SlotList) {
			obj.GetComponent<ResearchSlot>().UpdateSlot();

		}
	}

	private void OnResearchBtn(int id) {

		BillModel billModel;

		if(id == 0) {	// 코어 제작.

			bool state = _UserData.AddCoreByCoreChip();
			if(state == false) {
				_UserData.SetAlert(_ScriptData.GetGameScript(150109).script, new BillModel());
				return;
			} else {
				billModel = new BillModel();
				billModel.core = 1;
				billModel.corePlus = true;
				_UserData.UpdatePayData(billModel, new Vector2(-3.64f, -3.7f));
				SelectSlot(_CurrentIndex);
			}

			MissionData.getInstence().AddMissionGoal(MissionGoalType.Research_CoreChange_Start, 1);

			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("MakeCore", _UserData.UserCores + "-" + _UserData.UserChips);


		} else {
			// 개발 진행.
			if(_IsResearch) {	// 개발이 진행중.
				_UserData.SetAlert(_ScriptData.GetGameScript(150119).script, new BillModel());
				return;
			}
			Research research = _GearData.GetResearchByID((byte)(id));
			
			//research.unlockStep
			ArrayList prevStepResearchList = _GearData.GetResearchsByStep((byte)(research.unlockStep - 1));
			foreach(Research prevResearch in prevStepResearchList) {
				if(_UserData.GetUserResearchByReserch(prevResearch) == null) {
					_UserData.SetAlert(_ScriptData.GetGameScript(150120).script, new BillModel());
					return;
				}
			}
			
			billModel = _UserData.SetResearch(research);
			if(billModel.money > 0) {
				_UserData.UpdatePayData(billModel, new Vector2(-3.64f, -3.7f));

			} else {
				return;
			}
			
			int slotIndex = 1;
			foreach(GameObject obj in _SlotList) {
				if(obj.GetComponent<ResearchSlot>().GetResearch().id == id) {
					SelectSlot(slotIndex);
					break;
				}
				slotIndex ++;
			}

			if(id == 9) StoryData.getInstence().UpdateStoryStep(60002);	// 최초 로켓 개발 안내.

			_IsResearch = true;
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Research_Research_Start, 1);
			LocalData.getInstence().UserResearchDataSave();

			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("ResearchGear", research.gearId.ToString());
		}

		GuideArrowManager.getInstence().UpdateArrow();
	}
	
}

