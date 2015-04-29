using UnityEngine;
using System.Collections;

/*
 * 게임의 초기 마을 정보를 정의 합니다.
 * */

public class Town {
	public byte id;
	public byte soldierCount;	// 획득 가능한 대원의 수.
	public Vector2 position;		// 맵에서의 마을의 위치 정보.
	public int townNameId;		// 마을의 이름 스크립트 ID
	public bool isView;			// 사용자에게 보여질에 대한 정보.
	public byte state;			// 마을 혹은 둥지 상태 정보.
	public short resident;		// 주민 수.
	public short ghostClose;	// 고스트 밀집도.
	public short maxClose;		// 고스트 최대 밀집도.
	public byte root;			// 둥지시 고스트 공격이 타겟이 되는 마을의 ID
}
