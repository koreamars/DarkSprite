using UnityEngine;
using System.Collections;

public class MissionViewer : MonoBehaviour {

	public Color MissionTextColor;

	private float _PosLeft;

	private ArrayList MissionObjList;
	private ArrayList RewardObjList;

	// Use this for initialization
	void Start () {

		_PosLeft = SystemData.GetInstance().screenLeftX + 0.8f;

		MissionObjList = new ArrayList();
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		MissionObjList.Add(null);
		RewardObjList = new ArrayList();
		RewardObjList.Add(null);
		RewardObjList.Add(null);
		RewardObjList.Add(null);
		RewardObjList.Add(null);
		RewardObjList.Add(null);
		RewardObjList.Add(null);
		RewardObjList.Add(null);
		RewardObjList.Add(null);

		//MissionUpdate();
		MissionData.getInstence().SetUpdateMissionView(MissionUpdate);

	}

	private void MissionUpdate() {

		if(SystemData.GetInstance().CurrentSceneType != SceneType.MainScene) return;

		_PosLeft = SystemData.GetInstance().screenLeftX + 0.8f + this.gameObject.transform.position.x;

		UserData.getInstence().UserMissionList.Sort(new MissionSort());

		UserMission userMission;
		for(int mIndex = 0; mIndex < MissionObjList.Count; mIndex ++) {
			userMission = null;
			if(UserData.getInstence().UserMissionList.Count > mIndex) userMission = UserData.getInstence().UserMissionList[mIndex] as UserMission;

			if(userMission != null) {
				DefaultMission defaultMission = MissionData.getInstence().GetDefaultMission(userMission.defaultMissionId);
				if(defaultMission.Show > 0) {
					ShowMission(mIndex, userMission.defaultMissionId, userMission.currentGoal1, userMission.currentGoal2);
				} else {
					if(MissionObjList[mIndex] != null) Destroy(MissionObjList[mIndex] as GameObject);
					MissionObjList[mIndex] = null;
				}
			} else {
				if(MissionObjList[mIndex] != null) Destroy(MissionObjList[mIndex] as GameObject);
				MissionObjList[mIndex] = null;
			}

		}

	}

	private void ShowMission(int index, short missionId, byte count1, byte count2) {

		GameObject field = MissionObjList[index] as GameObject;
		GameObject textObj;
		GameObject symbolObj = null;
		GameObject rewardViewer = null;
		BillModel rewardBill = null;

		if(field == null) {
			field = new GameObject();
			symbolObj = new GameObject();
			symbolObj.transform.parent = field.transform;
			symbolObj.AddComponent<SpriteRenderer>();
			textObj = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
			textObj.name = "textobj";
			textObj.GetComponent<OutLineFont>().SetFontSize(16);
			textObj.GetComponent<OutLineFont>().SetAlign(TextAnchor.MiddleLeft);
			textObj.GetComponent<OutLineFont>().SetFontColor(MissionTextColor);
			textObj.transform.parent = field.transform;
			field.transform.position = new Vector2(_PosLeft, this.gameObject.transform.position.y + 3.4f - (0.55f * index));
			symbolObj.transform.position = new Vector2(field.transform.position.x - 0.3f, field.transform.position.y);
			textObj.transform.position = new Vector2(field.transform.position.x, field.transform.position.y);
			field.transform.parent = this.gameObject.transform;
			MissionObjList[index] = field;

			rewardViewer = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
			rewardBill = GetRewardBillByMission(missionId);
			if(rewardBill != null) {
				rewardViewer.GetComponent<RewardViewer>().init(rewardBill, 10, false);
				rewardViewer.transform.localScale = new Vector2(0.7f, 0.7f);
				rewardViewer.transform.parent = field.transform;
				RewardObjList[index] = rewardViewer;
			}
		} else {
			textObj = field.transform.FindChild("textobj").transform.gameObject;
			rewardViewer = RewardObjList[index] as GameObject;
		}

		textObj.GetComponent<OutLineFont>().SetString("null");

		DefaultMission defaultMission = MissionData.getInstence().GetDefaultMission(missionId);
		MissionScriptModel model = MissionData.getInstence().GetmissionScriptByMissionId(defaultMission.MissionGoal2Type);

		byte currentCount = 0;
		//if(count1 > 0) currentCount += count1;
		if(count2 > 0) currentCount += count2;

		byte targetCount = 0;
		//if(defaultMission.MissionGoal1Count > 0) targetCount += defaultMission.MissionGoal1Count;
		if(defaultMission.MissionGoal2Count > 0) targetCount += defaultMission.MissionGoal2Count;
		string missionTitle = "";
		if(model != null) {
			if(defaultMission.MissionGoal1Type == MissionGoalType.Target_GhostTown_Unlock || defaultMission.MissionGoal2Type == MissionGoalType.Target_GhostTown_Unlock) {
				Town town = WarzoneData.getInstence().GetDefaultTownData(defaultMission.MissionGoal2Count);
				string townName = ScriptData.getInstence().GetGameScript(town.townNameId).script;
				missionTitle = model.missionScript + " (" + townName + ")";
				textObj.GetComponent<OutLineFont>().SetString(missionTitle);
			} else {
				missionTitle = model.missionScript + " (" + currentCount + "/" + targetCount + ")";
				textObj.GetComponent<OutLineFont>().SetString(missionTitle);
			}

		} else {
			textObj.GetComponent<OutLineFont>().SetString("null / " + missionId);
		}
		if(rewardViewer != null) {
			float textWidth = textObj.GetComponent<OutLineFont>().GetTextWidth() + 0.5f;
			rewardViewer.transform.position = new Vector2(field.transform.position.x + textWidth, field.transform.position.y);
		}

		if(symbolObj != null) {
			SpriteRenderer renderer = symbolObj.GetComponent<SpriteRenderer>();
			if(rewardBill == null) {
				renderer.sprite = Resources.Load<Sprite>("Common/MissionSymbol/symbol0");
			} else {
				renderer.sprite = Resources.Load<Sprite>("Common/MissionSymbol/symbol" + model.symbolId);
				textObj.GetComponent<OutLineFont>().SetFontColor(Color.white);
			}
		}

	}

	public BillModel GetRewardBillByMission(short missionId) {
		BillModel returnBill = new BillModel();

		DefaultMission defaultMission = MissionData.getInstence().GetDefaultMission(missionId);
		Reward reward;
		foreach(short rewardId in defaultMission.RewardIds) {
			reward = MissionData.getInstence().GetReward(rewardId);
			if(reward != null) {
				switch(reward.Type) {
				case RewardType.Cash:
					returnBill.money = reward.Count;
					break;
				case RewardType.Core:
					returnBill.core = reward.Count;
					break;
				case RewardType.CoreChip:
					returnBill.corechip = reward.Count;
					break;
				}
			}

		}
		if(returnBill.money <= 0 && returnBill.core <= 0 && returnBill.corechip <= 0) {
			returnBill = null;
		} 
		return returnBill;
	}

	public class MissionSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((UserMission)x).defaultMissionId;
			int g2 = ((UserMission)y).defaultMissionId;
			
			if (g1 < g2)
				return -1;
			else
				return 0;
		}
		
	}
}
