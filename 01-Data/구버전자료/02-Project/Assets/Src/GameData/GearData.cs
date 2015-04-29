using UnityEngine;
using System;
using System.Xml;
using System.Collections;

public class GearData : MonoBehaviour {

	private static GearData _instence;

	private Gear[] _GearDataList;
	private Research[] _ResearchDataList;

	public static GearData getInstence()	{
		if(_instence == null) 
		{
			//_instence = new DataController();
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<GearData>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}
	
	void Awake() {

		// 기본 장비 데이터 초기화.
		TextAsset gearDataAsset = (TextAsset)Resources.Load("XMLData/DefaultGearData",typeof(TextAsset));
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(gearDataAsset.text);

		_GearDataList = new Gear[xmlDoc["DefaultGearData"].ChildNodes.Count];

		int xmlIndex = 0;
		foreach(XmlElement xmlElement in xmlDoc["DefaultGearData"]) {
			_GearDataList[xmlIndex] = new Gear();
			_GearDataList[xmlIndex].id = System.Convert.ToInt16(xmlElement["id"].InnerText);
			_GearDataList[xmlIndex].gearType = System.Convert.ToByte(xmlElement["gearType"].InnerText);
			_GearDataList[xmlIndex].scriptId = System.Convert.ToInt32(xmlElement["scriptId"].InnerText);
			_GearDataList[xmlIndex].resourceURI = xmlElement["resourceURI"].InnerText.ToString();
			_GearDataList[xmlIndex].thumbnailURI = xmlElement["thumbnailURI"].InnerText.ToString();
			_GearDataList[xmlIndex].minAP = System.Convert.ToInt16(xmlElement["minAP"].InnerText);
			_GearDataList[xmlIndex].maxAP = System.Convert.ToInt16(xmlElement["maxAP"].InnerText);
			_GearDataList[xmlIndex].ammo = System.Convert.ToInt16(xmlElement["ammo"].InnerText);
			_GearDataList[xmlIndex].minIAD = System.Convert.ToInt16(xmlElement["minIAD"].InnerText);
			_GearDataList[xmlIndex].maxIAD = System.Convert.ToInt16(xmlElement["maxIAD"].InnerText);
			_GearDataList[xmlIndex].addIA = System.Convert.ToInt16(xmlElement["addIA"].InnerText);
			_GearDataList[xmlIndex].spendIA = System.Convert.ToInt16(xmlElement["spendIA"].InnerText);
			_GearDataList[xmlIndex].addHP = System.Convert.ToInt16(xmlElement["addHP"].InnerText);
			_GearDataList[xmlIndex].spendMP = System.Convert.ToInt16(xmlElement["spendMP"].InnerText);
			_GearDataList[xmlIndex].addMP = System.Convert.ToInt16(xmlElement["addMP"].InnerText);
			_GearDataList[xmlIndex].makeTime = System.Convert.ToInt16(xmlElement["makeTime"].InnerText);
			_GearDataList[xmlIndex].makeCost = System.Convert.ToInt16(xmlElement["makeCost"].InnerText);
			_GearDataList[xmlIndex].makeResource = System.Convert.ToByte(xmlElement["makeResource"].InnerText);
			_GearDataList[xmlIndex].makeResourceType = System.Convert.ToByte(xmlElement["makeResourceType"].InnerText);
			_GearDataList[xmlIndex].upNextId = System.Convert.ToInt16(xmlElement["upNextId"].InnerText);

			xmlIndex ++;
		}

		// 기본 장비 데이터 초기화.
		TextAsset ResearchDataAsset = (TextAsset)Resources.Load("XMLData/ResearchData",typeof(TextAsset));
		XmlDocument researchXmlDoc = new XmlDocument();
		researchXmlDoc.LoadXml(ResearchDataAsset.text);
		
		_ResearchDataList = new Research[researchXmlDoc["ResearchData"].ChildNodes.Count];

		xmlIndex = 0;
		foreach(XmlElement xmlElement in researchXmlDoc["ResearchData"]) {
			_ResearchDataList[xmlIndex] = new Research();
			_ResearchDataList[xmlIndex].id = System.Convert.ToByte(xmlElement["id"].InnerText);
			_ResearchDataList[xmlIndex].gearId = System.Convert.ToByte(xmlElement["itemId"].InnerText);
			_ResearchDataList[xmlIndex].unlockLevel = System.Convert.ToByte(xmlElement["unlockLevel"].InnerText);
			_ResearchDataList[xmlIndex].unlockStep = System.Convert.ToByte(xmlElement["unlockStep"].InnerText);
			_ResearchDataList[xmlIndex].researchCost = System.Convert.ToInt32(xmlElement["researchCost"].InnerText);
			_ResearchDataList[xmlIndex].coreCost = System.Convert.ToInt32(xmlElement["coreCost"].InnerText);
			_ResearchDataList[xmlIndex].researchTime = System.Convert.ToInt16(xmlElement["researchTime"].InnerText);

			xmlIndex ++;
		}

	}

	/** ID에 해당하는 장비 정보를 반환함 */
	public Gear GetGearByID(short id) {
		foreach(Gear gear in _GearDataList) {
			if(gear.id == id) return gear;
		}
		return null;
	}

	/**
	 * ID에 해당하는 무기 아이템을 반환합니다. 
	 * */
	public Gear GetWeaponGearByID(short id) {
		foreach(Gear gear in _GearDataList) {
			if(gear.gearType == GearType.Weapon_Gun || gear.gearType == GearType.Weapon_Rocket 
			   || gear.gearType == GearType.Weapon_Missle || gear.gearType == GearType.GhostWeapon) {
				if(gear.id == id) return gear;
			}
		}
		return null;
	}

	/**
	 * ID에 해당하는 Research 데이터를 반환합니다.
	 * */
	public Research GetResearchByID(byte id) {
		foreach(Research research in _ResearchDataList) {
			if(research.id == id) return research;
		}
		return null;
	}

	/**
	 * GearID에 해당하는 Research 데이터를 반환합니다.
	 * */
	public Research GetResearchByGearID(short gearid) {
		foreach(Research research in _ResearchDataList) {
			if(research.gearId == gearid) return research;
		}
		return null;
	}


	/** Arraylist로 전달되는 Id외의 Research 정보 list를 반환함. */
	public ArrayList GetUnlockResearchBylockIds(byte[] lockIds) {
		ArrayList researchList = new ArrayList();

		int index;
		foreach(Research research in _ResearchDataList) {
			index = Array.IndexOf(lockIds, research.id);
			if(index < 0) researchList.Add(research.id);
		}
		return researchList;
	}

	/** Research 목록을 반환 */
	public Research[] GetResearchList() {
		return _ResearchDataList;
	}

	/** 해당 스텝의 기술의 수를 반환. */
	public byte GetStepCount(byte step) {
		byte stepCount = 0;
		foreach(Research research in _ResearchDataList) {
			if(research.unlockStep == step) stepCount ++;
		}
		return stepCount;
	}

	/** 해당 스텝에 해당하는 리서치 정보를 반환함 */
	public ArrayList GetResearchsByStep(byte step) {
		ArrayList researchList = new ArrayList();
		foreach(Research research in _ResearchDataList) {
			if(research.unlockStep == step) researchList.Add(research);
		}
		return researchList;
	}

	/** 대원의 스펙을 착용한 장비에 맞게 갱신함 */
	public void UpdateMemberGearSpec(Member member) {
		DefaultMember defaultMember = MemberData.getInstence().GetDefaultMemberByID(member.DefaultId);
		ClassModel classModel = MemberData.getInstence().GetClassModelByClassId(member.ClassId);
		Gear suitGear = GetGearByID(member.SuitGearId);
		Gear bodyGear = GetGearByID(member.BodyGearId);
		Gear engineGear = GetGearByID(member.EngineGearId);
		Gear weapon1Gear = GetGearByID(member.Weapon1GearId);
		Gear weapon2Gear = GetGearByID(member.Weapon2GearId);
		Gear weapon3Gear = GetGearByID(member.Weapon3GearId);

		member.MaxHP = classModel.HP + suitGear.addHP + bodyGear.addHP + engineGear.addHP;
		if(member.CurrentHP > member.MaxHP) member.CurrentHP = member.MaxHP;
		member.MaxMP = classModel.MP + suitGear.addMP + bodyGear.addMP + engineGear.addMP;
		if(member.CurrentMP > member.MaxMP) member.CurrentMP = member.MaxMP;
		member.TotalIA = classModel.IA + suitGear.addIA + bodyGear.addIA + engineGear.addIA;
		member.TotalIA -= (suitGear.spendIA + bodyGear.spendIA + engineGear.spendIA + weapon1Gear.spendIA);
		if(weapon2Gear != null) member.TotalIA -= weapon2Gear.spendIA;
		if(weapon3Gear != null) member.TotalIA -= weapon3Gear.spendIA;

		member.UMP = suitGear.spendMP + bodyGear.spendMP + engineGear.spendMP + weapon1Gear.spendMP;
		if(weapon2Gear != null) member.UMP += weapon2Gear.spendMP;
		if(weapon3Gear != null) member.UMP += weapon3Gear.spendMP;
	}

}
