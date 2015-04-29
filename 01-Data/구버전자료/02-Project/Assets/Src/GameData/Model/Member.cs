using UnityEngine;
using System.Collections;

public class Member {

	public short id;
	public short DefaultId;
	public byte ClassId;
	public int NameId;			// 0;
	public short BodyGearId;
	public short EngineGearId;
	public short SuitGearId;
	public short Weapon1GearId;
	public short Weapon2GearId;
	public short Weapon3GearId;
	public string Thumbnail;
	public int CurrentHP;
	public int MaxHP;
	public int CurrentMP;
	public int MaxMP;
	public int TotalIA;
	public int UMP;
	public byte UnitId;
	public int Exp;
	public long lastHPUpdateTime;
	public long lastMPUpdateTime;
	public byte state;
}
