using UnityEngine;
using System.Collections;

/*
 * 게임내의 사용자의 부대 정보를 갑니다.
 * */

public class UnitData : MonoBehaviour {


	private static UnitData _instence;

	private ArrayList _UserUnitData;

	public static UnitData getInstence()	{
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<UnitData>();
		}
		
		return _instence;
	}

	public UnitData () {

		_UserUnitData = new ArrayList();

	}

	public ArrayList GetUserUnitList() {
		return _UserUnitData;
	}


}
