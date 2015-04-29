using UnityEngine;
using System.Xml;
using System.Collections;

public class MissionData : MonoBehaviour {

	private static MissionData _instence;

	private DefaultMission[] _DefaultMissionData;
	private Reward[] _RewardData;
	private MissionScriptModel[] _MissionScriptData;

	private ArrayList _AddMissionList;

	public delegate void UpdateMissionView();
	private UpdateMissionView _UpdateMissionView;

	public static MissionData getInstence()	{
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<MissionData>();
			DontDestroyOnLoad(_instence);

		}
		
		return _instence;
	}

	// IntroScene에서 한번 초기화 함.
	public IEnumerator init() {
		yield return new WaitForEndOfFrame();
		// 기본 미션 초기화.
		TextAsset missiontextAsset = (TextAsset)Resources.Load("XMLData/DefaultMissionData",typeof(TextAsset));
		XmlDocument missionXmlDoc = new XmlDocument();
		missionXmlDoc.LoadXml(missiontextAsset.text);

		int xmlIndex = 0;
		
		_DefaultMissionData = new DefaultMission[missionXmlDoc["DefaultMissionData"].ChildNodes.Count];
		
		foreach(XmlElement xmlElement in missionXmlDoc["DefaultMissionData"]) {
			DefaultMission defaultMission = new DefaultMission();
			defaultMission.id = System.Convert.ToInt16(xmlElement["id"].InnerText);
			defaultMission.Type = System.Convert.ToByte(xmlElement["type"].InnerText);
			defaultMission.Show = System.Convert.ToByte(xmlElement["show"].InnerText);
			defaultMission.StartStoryStep = System.Convert.ToInt32(xmlElement["StartStoryStep"].InnerText);
			defaultMission.EndStoryStep = System.Convert.ToInt32(xmlElement["EndStoryStep"].InnerText);
			defaultMission.MissionGoal1Type = System.Convert.ToByte(xmlElement["MissionGoal1Id"].InnerText);
			defaultMission.MissionGoal1Count = System.Convert.ToByte(xmlElement["MissionGoal1Count"].InnerText);
			defaultMission.MissionGoal2Type = System.Convert.ToByte(xmlElement["MissionGoal2Id"].InnerText);
			defaultMission.MissionGoal2Count = System.Convert.ToByte(xmlElement["MissionGoal2Count"].InnerText);
			defaultMission.RewardIds = new short[7];

			byte rewardIndex = 0;
			string rewardIds = xmlElement["RewardIds"].InnerText;
			string rewardIdStr = "";
			foreach(char strN in rewardIds.ToCharArray()) {
				if(strN != System.Convert.ToChar(",")) {
					rewardIdStr += strN;

				} else {
					defaultMission.RewardIds[rewardIndex] = System.Convert.ToInt16(rewardIdStr);
					//print("rewardIdStr : " + rewardIdStr);
					rewardIdStr = "";
					rewardIndex ++;
				}
			}
			rewardIndex ++;
			short rewardId = System.Convert.ToInt16(rewardIdStr);
			if(rewardId > 0) defaultMission.RewardIds[rewardIndex] = rewardId;


			_DefaultMissionData[xmlIndex] = defaultMission;
			
			xmlIndex ++;
		}

		// 리워드 정보 초기화.
		TextAsset rewardtextAsset = (TextAsset)Resources.Load("XMLData/RewardData",typeof(TextAsset));
		XmlDocument rewardXmlDoc = new XmlDocument();
		rewardXmlDoc.LoadXml(rewardtextAsset.text);

		xmlIndex = 0;

		_RewardData = new Reward[rewardXmlDoc["RewardData"].ChildNodes.Count];

		foreach(XmlElement xmlElement in rewardXmlDoc["RewardData"]) {
			Reward reward = new Reward();
			reward.id = System.Convert.ToInt16(xmlElement["id"].InnerText);
			reward.Type = System.Convert.ToByte(xmlElement["type"].InnerText);
			reward.Count = System.Convert.ToInt16(xmlElement["count"].InnerText);

			_RewardData[xmlIndex] = reward;

			xmlIndex ++;
		}

		// 미션 스크립트.
		TextAsset scriptTextAsset = (TextAsset)Resources.Load("XMLData/DefaultMissionScriptData",typeof(TextAsset));
		XmlDocument scriptXmlDoc = new XmlDocument();
		scriptXmlDoc.LoadXml(scriptTextAsset.text);
		
		xmlIndex = 0;
		
		_MissionScriptData = new MissionScriptModel[scriptXmlDoc["DefaultMissionScriptData"].ChildNodes.Count];
		
		foreach(XmlElement xmlElement in scriptXmlDoc["DefaultMissionScriptData"]) {
			MissionScriptModel scriptModel = new MissionScriptModel();
			scriptModel.missionId = System.Convert.ToInt16(xmlElement["id"].InnerText);
			scriptModel.symbolId = System.Convert.ToByte(xmlElement["symbolId"].InnerText);
			scriptModel.missionScript = xmlElement["scriptData"].InnerText;
			
			_MissionScriptData[xmlIndex] = scriptModel;
			
			xmlIndex ++;
		}

		yield return 0;

	}

	public void SetUpdateMissionView(UpdateMissionView OnUpdateMissionView) {
		_UpdateMissionView = new UpdateMissionView(OnUpdateMissionView);
	}

	public void ShowUpdateMissionView() {
		if(_UpdateMissionView != null) _UpdateMissionView();
	}

	/** 기본 미션 정보를 반환 함. */
	public DefaultMission GetDefaultMission(short missionId) {
		foreach(DefaultMission defaultMission in _DefaultMissionData) {
			if(missionId == defaultMission.id) return defaultMission;
		}
		return null;
	}

	/** 기본 리워드 정보를 반환 함. */
	public Reward GetReward(short rewardId) {
		//print("_RewardData : " + _RewardData.Length);
		foreach(Reward reward in _RewardData) {
			if(rewardId == reward.id) return reward;
		}
		return null;
	}

	/** 미션 스크립트 정보를 반환 함. */
	public MissionScriptModel GetmissionScriptByMissionId(short missionGoalTypeId) {
		foreach(MissionScriptModel model in _MissionScriptData) {
			if(missionGoalTypeId == model.missionId) return model;
		}
		return null;
	}

	/** 해당 골에 관련된 미션을 갱신합니다. */
	public void AddMissionGoal(byte type, byte count) {
		DefaultMission defaultMission;

		bool update = false;
		ArrayList deleteMissionList = new ArrayList();
		foreach(UserMission userMission in UserData.getInstence().UserMissionList) {

			defaultMission = GetDefaultMission(userMission.defaultMissionId);

			if(type == MissionGoalType.Target_GhostTown_Unlock) {	// 동일 값일 경우.
				// 1번 미션 골 체크.
				if(defaultMission.MissionGoal1Count == count && defaultMission.MissionGoal1Type == type) {
					userMission.currentGoal1 = count;
					update = true;
				}

				// 2번 미션 골 체크.
				if(defaultMission.MissionGoal2Count == count && defaultMission.MissionGoal2Type == type) {
					userMission.currentGoal2 = count;
					update = true;
				}
			} else if (type == MissionGoalType.Member_TotalCount) {
				// 1번 미션 골 체크.
				if(defaultMission.MissionGoal1Type == type) {
					userMission.currentGoal1 = count;
					update = true;
				}
				
				// 2번 미션 골 체크.
				if(defaultMission.MissionGoal2Type == type) {
					userMission.currentGoal2 = count;
					update = true;
				}
			} else {
				// 1번 미션 골 체크.
				if(defaultMission.MissionGoal1Count > userMission.currentGoal1 && defaultMission.MissionGoal1Type == type) {
					userMission.currentGoal1 += count;
					update = true;
				}

				// 2번 미션 골 체크.
				if(defaultMission.MissionGoal2Count > userMission.currentGoal2 && defaultMission.MissionGoal2Type == type) {
					userMission.currentGoal2 += count;
					update = true;
				}
			}


			// 미션 완료 체크.  ////////////////////////////////////
			if(defaultMission.MissionGoal1Count <= userMission.currentGoal1 && defaultMission.MissionGoal2Count <= userMission.currentGoal2) {
				//미션 완료 시.
				update = true;
				// 리워드 지급.
				BillModel billModel = SetReward(defaultMission.RewardIds);

				// 스토리 스텝을 저장합니다.
				if(defaultMission.EndStoryStep > 0) {
					if(defaultMission.EndStoryStep < 100000) UserData.getInstence().StoryStepId = defaultMission.EndStoryStep;
					LocalData.getInstence().UserStoryStepSave();
					StoryData.getInstence().UpdateStoryStep(defaultMission.EndStoryStep);

					// GuideArrow 갱신.
					GuideArrowManager.getInstence().UpdateArrow();
				}

				if(defaultMission.id >= 1000) {
					MissionScriptModel model = MissionData.getInstence().GetmissionScriptByMissionId(defaultMission.MissionGoal2Type);
					string missionTitle = "";
					if(model != null) {
						if(defaultMission.MissionGoal1Type == MissionGoalType.Target_GhostTown_Unlock || defaultMission.MissionGoal2Type == MissionGoalType.Target_GhostTown_Unlock) {
							Town town = WarzoneData.getInstence().GetDefaultTownData(defaultMission.MissionGoal2Count);
							string townName = ScriptData.getInstence().GetGameScript(town.townNameId).script;
							missionTitle = model.missionScript + " (" + townName + ")";
						} else {
							missionTitle = model.missionScript;
						}
						UserData.getInstence().SetAlert(missionTitle + "<p>" + ScriptData.getInstence().GetGameScript(150139).script, billModel);
					}

				}

				// 유저 정보에서 해당 미션 제거.
				deleteMissionList.Add(userMission.defaultMissionId);

			}
		}
		foreach(short deleteMissionId in deleteMissionList) {
			DeleteMission(deleteMissionId);
		}

		if(_AddMissionList != null) {
			foreach(short addMissionId in _AddMissionList) {
				AddMission(addMissionId);
			}
			_AddMissionList.Clear();
		}

		if(update) {
			UpdateMissionData();
			if(_UpdateMissionView != null) _UpdateMissionView();
			LocalData.getInstence().UserMissionDataSave();
		}
	}

	/** 미션 갱신. */
	public void UpdateMissionData() {

		DefaultMission defaultMission;
		UserMission mainUserMission;
		ArrayList updateIds = new ArrayList();
		foreach(UserMission userMission in UserData.getInstence().UserMissionList) {
			updateIds.Add(userMission.defaultMissionId);
		}

		if(updateIds.Count == 0) return;

		foreach(short missionId in updateIds) {
			defaultMission = GetDefaultMission(missionId);
			mainUserMission = UserData.getInstence().GetUserMission(missionId);

			if(mainUserMission == null) continue;

			if(defaultMission.MissionGoal2Count <= mainUserMission.currentGoal2) {
				AddMissionGoal(defaultMission.MissionGoal2Type, mainUserMission.currentGoal2);
			} else {
				AddMissionGoal(defaultMission.MissionGoal1Type, mainUserMission.currentGoal1);
			}

		}
	}

	/** 해당 리워드를 보상으로 지급합니다. */
	private BillModel SetReward(short[] rewardIdList) {

		BillModel returnBillModel = new BillModel();
		returnBillModel.core = 0;
		returnBillModel.corechip = 0;
		returnBillModel.money = 0;

		Reward reward;
		foreach(short rewardId in rewardIdList) {

			if(rewardId == 0) continue;

			reward = GetReward(rewardId);

			switch(reward.Type) {
			case RewardType.Cash:
				UserData.getInstence().UserMoney += reward.Count;
				returnBillModel.money += reward.Count;
				break;
			case RewardType.Core:
				UserData.getInstence().UserCores += reward.Count;
				returnBillModel.core += reward.Count;
				break;
			case RewardType.CoreChip:
				UserData.getInstence().UserChips += reward.Count;
				returnBillModel.corechip += reward.Count;
				break;
			case RewardType.Gear:
				Gear gear = GearData.getInstence().GetGearByID((byte)(reward.Count));
				OwnGear ownGear = UserData.getInstence().GetOwnGearByGearId(gear.id);
				if(ownGear == null) {
					ownGear = new OwnGear();
					ownGear.gearId = gear.id;
					ownGear.ownCount = 1;
					UserData.getInstence().UserOwnGearList.Add(ownGear);
				} else {
					ownGear.ownCount += 1;
				}
				break;
			case RewardType.Mission:
				if(_AddMissionList == null) _AddMissionList = new ArrayList();
				_AddMissionList.Add(reward.Count);
				break;
			case RewardType.TownOpen:
				GhostTown ghostTown = WarzoneData.getInstence().GetGhostTownByTownId((byte)(reward.Count));
				if(ghostTown != null) {
					ghostTown.isView = true;
					if(DarkSprite.getInstence().MainMapData != null) DarkSprite.getInstence().MainMapData.UpdateTownSymbols();
					LocalData.getInstence().GhostTownDataSave();
				}
				break;
			case RewardType.TownAttack:
				SystemData systemData = SystemData.GetInstance();
				UserTown userTown = WarzoneData.getInstence().GetUserTownByID((byte)(reward.Count));
				userTown.isInvasion = true;
				//userTown.lastInvasionEndTime += (systemData.GhostAttachDelay * systemData.millisecondNum) + systemData.getCurrentTime();
				userTown.lastInvasionEndTime = systemData.getCurrentTime();
				long invasionTime = (systemData.TownInvasionDelay * systemData.millisecondNum);
				userTown.lastInvasionEndTime += invasionTime;
				GhostTown attackGhostTown = WarzoneData.getInstence().GetGhostTownByRootId(userTown.id, false);
				userTown.invasionGhostClose = 100;
				userTown.invasionGhostTownId = attackGhostTown.id;
				if(DarkSprite.getInstence().MainMapData != null) DarkSprite.getInstence().MainMapData.UpdateTownSymbols();
				break;
			}
		}

		return returnBillModel;
	}

	/** 해당 하는 미션을 새로 지급합니다. */
	public void AddMission(short missionId) {
		bool sumMission = false;
		foreach(UserMission userMission in UserData.getInstence().UserMissionList) {
			if(userMission.defaultMissionId == missionId) {
				print("동일 미션이 존재함.");
				sumMission = true;
				break;
			}
		}
		if(sumMission) return;

		DefaultMission defaultMission = GetDefaultMission(missionId);
		if(defaultMission.StartStoryStep > 0) {
			if(defaultMission.StartStoryStep < 100000) UserData.getInstence().StoryStepId = defaultMission.StartStoryStep;
			LocalData.getInstence().UserStoryStepSave();
			StoryData.getInstence().UpdateStoryStep(defaultMission.StartStoryStep);
			//print("defaultMission.StartStoryStep : " + defaultMission.StartStoryStep);

			// GuideArrow 갱신.
			GuideArrowManager.getInstence().UpdateArrow();
		}
		UserMission newUserMission = new UserMission();
		newUserMission.defaultMissionId = missionId;
		newUserMission.currentGoal1 = 0;
		newUserMission.currentGoal2 = 0;
		UserData.getInstence().UserMissionList.Add(newUserMission);

		// 맴버 수 골 체크.
		if(defaultMission.MissionGoal1Type == MissionGoalType.Member_TotalCount) {
			newUserMission.currentGoal1 = (byte)(UserData.getInstence().UserMemberList.Count);
		} 

		if(defaultMission.MissionGoal2Type == MissionGoalType.Member_TotalCount) {
			newUserMission.currentGoal2 = (byte)(UserData.getInstence().UserMemberList.Count);
		}

		// 타운 클리어 체크.
		GhostTown ghostTown = null;
		if(defaultMission.MissionGoal1Type == MissionGoalType.Target_GhostTown_Unlock) {
			ghostTown = WarzoneData.getInstence().GetGhostTownByTownId(defaultMission.MissionGoal1Count);
			if(ghostTown == null) newUserMission.currentGoal1 = defaultMission.MissionGoal1Count;
		}

		ghostTown = null;
		if(defaultMission.MissionGoal2Type == MissionGoalType.Target_GhostTown_Unlock) {
			ghostTown = WarzoneData.getInstence().GetGhostTownByTownId(defaultMission.MissionGoal2Count);
			if(ghostTown == null) newUserMission.currentGoal2 = defaultMission.MissionGoal2Count;
		}
	}

	/** 해당 미션을 유저 정보에서 제거 합니다. */
	private void DeleteMission(short missionId) {
		short delIndex = 0;
		foreach(UserMission userMission in UserData.getInstence().UserMissionList) {
			if(userMission.defaultMissionId == missionId) break;
			delIndex ++;
		}
		UserData.getInstence().UserMissionList.RemoveAt(delIndex);
	}

	/** 지난 미션 (튜토리얼)을 다시 보여줌. */
	public void MainStoryCheck() {

		DefaultMission defaultMission = null;

		foreach(UserMission userMission in UserData.getInstence().UserMissionList) {
			defaultMission = GetDefaultMission(userMission.defaultMissionId);
			if(defaultMission.Type == 0) {
				if(userMission.currentGoal1 == 0 && userMission.currentGoal2 == 0) {
					StoryData.getInstence().UpdateStoryStep(UserData.getInstence().StoryStepId);
				}

			}
		}

	}

}
