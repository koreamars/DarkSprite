using UnityEngine;
using System.Collections;

/*
 * 사용자의 현재 마을 정보를 정의합니다.
 * */

public class UserTown {

	public byte id;
	/** 주민 수 */
	public short resident;
	public bool isInvasion;				// 침략 중인지.
	public long lastTaxTime;
	public long lastResidentPlusTime;	// 주민 증가 시간.
	public byte invasionGhostTownId;
	public short invasionGhostClose;
	public long lastInvasionEndTime;	// 공격까지 남은 시간.
}
	