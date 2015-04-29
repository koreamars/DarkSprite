using UnityEngine;
using System.Collections;

public class NotificationManager : MonoBehaviour {

	private static NotificationManager _instence;

	private ELANNotification _ELANNotification = null;

	private ELANNotification _MemberCareNotification = null;
	private ELANNotification _TaxNotification = null;

	private bool _IsMemberData = false;
	private bool _IsTaxData = false;
	private bool _IsNormalNotiData = false;

	public static NotificationManager getInstence()	{

		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<NotificationManager>();
			//_MemberCareNotification = instence.AddComponent<ELANNotification>() as ELANNotification;
			//_MemberCareNotification.ID = 1;
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}

	public void AddNotification(string message, int time) {
		print("AddNotification");
		// 안드로이드 노티 기능.

		if(_ELANNotification == null) {
			_ELANNotification = new ELANNotification();
			_ELANNotification.ID = 3;
		}

		CancelNormalNoti();

		_ELANNotification.message = message;
		_ELANNotification.title = "DarkSprite";
		_ELANNotification.repetition = 0;		// 반복 횟수.
		_ELANNotification.delay = time;
		_ELANNotification.useSound = true;		// 소리.
		_ELANNotification.useVibration = false;	// 진동.
		_ELANNotification.send();

		_IsNormalNotiData = true;

	}

	public void CancelNormalNoti() {
		if(_IsNormalNotiData == false) return;

		_ELANNotification.cancel();

		_IsNormalNotiData = false;
	}

	// HP/MP 회복 노티.
	public void SetMemberCareNotification(int time) {
		//print("SetMemberCareNotification : " + time);
		UserData.getInstence().TestData = time;
		if(time < 60) return;

		if(_MemberCareNotification == null) {
			_MemberCareNotification = new ELANNotification();
			_MemberCareNotification.ID = 1;
		}

		CancelMemberCareNoti();

		_MemberCareNotification.message = ScriptData.getInstence().GetGameScript(220101).script;
		_MemberCareNotification.title = ScriptData.getInstence().GetGameScript(220204).script;
		_MemberCareNotification.repetition = 0;		// 반복 횟수.
		_MemberCareNotification.delay = time;
		_MemberCareNotification.useSound = true;		// 소리.
		_MemberCareNotification.useVibration = false;	// 진동.
		_MemberCareNotification.send();

		_IsMemberData = true;
	}

	// 세금 징수 관련 노티.
	public void SetTaxNotification(int time) {

		print("SetTaxNotification : " + time);
		if(_TaxNotification == null) {
			_TaxNotification = new ELANNotification();
			_TaxNotification.ID = 2;
		}

		CancelTaxNoti();

		_TaxNotification.message = ScriptData.getInstence().GetGameScript(220102).script;
		_TaxNotification.title = ScriptData.getInstence().GetGameScript(220203).script;
		_TaxNotification.repetition = 0;		// 반복 횟수.
		_TaxNotification.delay = time;
		_TaxNotification.useSound = true;		// 소리.
		_TaxNotification.useVibration = false;	// 진동.
		_TaxNotification.send();

		_IsTaxData = true;
	}

	// HP/MP 노티 취소.
	public void CancelMemberCareNoti() {

		if(_IsMemberData == false) return;

		_MemberCareNotification.cancel();

		_IsMemberData = false;
	}

	// 세금 징수 노티 취소.
	public void CancelTaxNoti() {

		if(_IsTaxData == false) return;

		_TaxNotification.cancel();

		_IsTaxData = false;
	}

	// 모든 노티 제거.
	public void CancelAllNoti() {
		CancelTaxNoti();
		CancelMemberCareNoti();
		CancelNormalNoti();
	}



}
