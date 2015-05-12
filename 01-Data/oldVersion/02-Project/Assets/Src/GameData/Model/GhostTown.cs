using UnityEngine;
using System.Collections;

/*
 * 사용자의 현재 둥지 정보를 정의합니다.
 * 
 * */

public class GhostTown {

	public byte id;
	public short ghostClose;
	public long lastClosePlusTime;
	public long lastAttackTime;
	public bool isView;
}
