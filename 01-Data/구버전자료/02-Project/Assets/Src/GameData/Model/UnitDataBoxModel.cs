using UnityEngine;
using System.Collections;

public class UnitDataBoxModel {

	public byte id;
	public short modelId;
	public byte type;		// 0:member 1:enemy
	public string imgName;
	public int currentHP;
	public int maxHP;
	public int currentMP;
	public int maxMP;
	public bool isSelect;
	public bool isTarget;
	public bool isOut;
	public int ActNum;
	public int MaxActNum;
	public byte turnIndex = 0;
	public int Ammo1 = -1;
	public int Ammo2 = -1;
	public int Ammo3 = -1;
}
