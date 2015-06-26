using UnityEngine;
using System;
using System.Collections;
using GoogleMobileAds.Api;

/*
 * 게임내에 중요 시스템 정보를 가집니다.
 * 
 * */

public class SystemData : MonoBehaviour {

	// 빌드 버전 =============================================== 빌드 버전 ====================================================== 빌드 버전
	public const string BuildVersion = "1.1.136";
	// 빌드 버전 =============================================== 빌드 버전 ====================================================== 빌드 버전

	public static SystemData instance;

	public GameObject SystemMessage;

	public float screenTopY = 3f;
	public float screenLeftX = 0f;
	public float screenRightX = 0f;
	public float mapDefaultScale = 1f;

	public float PopupHeight = 0f;
	public float PopupY = 0f;

	/**
	 * 턴마다 소비되는  행동 포인트..
	 * */
	public long millisecondNum = 10000000;			// 밀리초 연산자.
	public short TurnActPoint = 2;

	public byte DarkgearSuccessValue = 20;			// 다크 기어 기본 성공 확률 - 전투후 획득 획득 확률과 별개임.

	/** 주민 1명당 세금. */
	public byte ResidentPerPay = 2;				
	/** 마을의 주민 증가 수. */
	public byte TownNextResidentCount = 5;			
	/** 마을 최대 인구수. */
	public short TownMaxResidentCount = 500;		
	/** 초기 마을 인구 수. */
	public short TownDefaultResident = 10;			
	/** 고스트에 의한 마을 공격 데미지. */
	public short TownDamage = 100;		

	public byte MemberMaxClass = 8;

	/** 둥지의 밀집도 증가 수. */
	public byte GhostTownNextCount	= 200;			
	/** 공격하는 둥지가 공격 가능한 밀집도 제한. */
	public short GhostAttachLimit = 200;			
	/** 둥지 밀집도 공격력. - 둥지 공격시 타격 받는 정도. */
	public byte GhostTownDamage = 100;				
	/** 초기 둥지 밀집도. */
	public short GhostDefaultClose = 10;
	/** 동시에 공격되는 최대 마을 수 */
	public byte MaxSameTownAttackCount = 5;
	/** 누적 공격 횟수 - 100이면 무조건 마을 조건 */
	public int AccumulateTownAttackCount = 0;

	// 각 정보의 딜레이 시간 정보.
	/*
	public int TownTaxCollectdelay = 120;			// 마을의 세금 징수 소요 시간.
	public short TownNextResidentTime = 150;		// 마을의 인구 증가 딜레이.
	public short GhostTownNextTime = 150;			// 고스트 둥지 성장 딜레이.
	public short GhostAttachDelay = 600;			// 고스트 침략 딜레이.
	public short TownInvasionDelay = 300;			// 마을 공격 소요 시간.
	*/

	public int TownTaxCollectdelay = 7200;			// 마을의 세금 징수 소요 시간.
	public short TownNextResidentTime = 3000;		// 마을의 인구 증가 시간.
	public short GhostTownNextTime = 1800;			// 고스트 둥지 성장 딜레이.
	public short GhostAttachDelay = 3600;			// 고스트 침략 딜레이. - 마을 성장 시간 보다 높아야 함.
	public short TownInvasionDelay = 300;			// 마을 공격 소요 시간.

	public int MemberHpPlusDelay = 60;				// 대원은 HP회복 시간 계산.
	public int MemberHpPlusCount = 2;				// 대원 HP 회복 량.
	public int MemberMpPlusDelay = 60;				// 대원은 MP회복 시간 계산.
	public int MemberMpPlusCount = 2;				// 대원 MP 회복 량.

	public int FightUnitRwardCount = 3;				// 한 전투에서 획득 가능한 리워드 수 제한.
	public int FightUnitExp = 13;					// 고스트 한마리당 획득 경험치.

	// 과금 정보. 
	public byte CoreMakeChipCount = 8;				// 코어 하나를 만드는 필요한 칩수.
	public byte MemberSearchCore = 10;				// 대원 수색에 필요한 코어 비용.
	public short NormalSearchMoney = 500;
	public byte MemberHealCore = 1;					// 대원이 회복에 필요한 코어 비용.
	public byte MemberHealMoney = 100;					// 대원이 회복에 필요한 캐쉬 비용.
	public byte MemberCareCore = 4;					// 대원 치료에 필요한 코어 비용.
	public byte MemberRescueCore = 10;					// 대원 구조에 필요한 코어 비용.
	public byte MemberClassCost = 15;				// 대원 계급별 급여.

	public byte TownAttackPoint = 8;				// 클래스당 둥지 공격력.

	public bool IsStoryCharge = false;				// 스토리 과금 유무.

	// 기지 개발 최대 치.
	public byte BaseUpgradeMax = 2;
	public byte ResearchUpgradeMax = 1;
	public byte FactoryUpgradeMax = 3;

	public byte MaxOwnItemCount = 10;	// 최대 장비 보유량.

	public int GearDeleteRewardValue = 5;	// 장비 제거시 자원 반환 비율. 1/n


	// 맴버 획득 확률 퍼센트.	(모든 수의 합이 1000이 되야 함).
	public short[] MemberChanceData = new short[5]{600, 300, 70, 25, 5};

	public Color UpColor_Blue = Color.blue;
	public Color UpColor_Green = Color.green;
	public Color DownColor_Red = Color.red;
	public Color WeaponColor_Red = Color.red;
	public Color WeaponColor_Yellow = Color.yellow;
	public Color NonColor_Glay = Color.gray;

	public bool firstStoryShow = false;

	public byte CurrentSceneType = 0;

	public bool IsMainBtnClick = false;

	// 베너 관련 정보.
	private BannerView _bannerView;
	public int UserDefaultBannerCount = 30;
	public int UserMaxBannerCount = 1000000;	// 베너 카운트 감소 차단 수치.

	public byte GameServiceType = 0;		// LIVE / BETA / ALPHA
	private string currentWWWStr = "";
	public bool IsGetFightCore = false;		// 코어 강제 지급.
	public bool IsShopPurchase = true;		// 결제를 검증 하는가?
	public bool IsMemberXpPlus = false;		// 강제로 맴버에서 추가 경험치를 부여함.

	public string TestGoogleId = "UA-44211594-5";

	public static SystemData GetInstance () {

		if(instance == null)
		{
			GameObject dataObj = new GameObject();
			instance = dataObj.AddComponent<SystemData>() as SystemData;
			DontDestroyOnLoad(instance);
		}

		return instance;
	}

	void Awake() {
		UpColor_Blue.r = 0.5f;
		UpColor_Blue.g = 0.5f;
		UpColor_Blue.b = 1f;
		
		UpColor_Green.r = 0.5f;
		UpColor_Green.g = 1f;
		UpColor_Green.b = 0.5f;
		
		screenTopY = Camera.main.ScreenToWorldPoint(new Vector3(0,Screen.height,0)).y;
		screenLeftX = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x;
		screenRightX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)).x;
		
		if(Screen.height > 520f) {
			PopupHeight = 500f;
			PopupY = (Screen.height - 500f) / 2;
		} else {
			PopupHeight = Screen.height - 20f;
			PopupY = 10f;
		}

	}

	void Start() {

	}

	public GameObject GetLoadingMark() {
		GameObject loadingMark = new GameObject();
		loadingMark.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Common/LoadingMark");
		loadingMark.name = "LoadingMark";
		loadingMark.GetComponent<Renderer>().sortingOrder = 101;
		loadingMark.layer = LayerMask.NameToLayer("Alert");

		return loadingMark;
	}

	public long getCurrentTime() {
		return System.DateTime.UtcNow.ToFileTime();
	}

	/**
	 * 해당 시간을 00:00:00 스트링 형태로 반환.
	 * */
	public string GetTimeStrByTime(long time) {
		TimeSpan timespan = new TimeSpan(time);
		return string.Format("{0:D2}:{1:D2}:{2:D2}", timespan.Hours, timespan.Minutes, timespan.Seconds);;
	}

	/** 시스템 메세지 노출 */
	public void AddMessage(string str, Color color) {
		if(SystemMessage != null) {
			SystemMessage.GetComponent<SystemMessage>().AddMessage(str, color);
		}
	}

	
	public void ShowBanner() {
		if(_bannerView == null) {
			_bannerView = new BannerView("ca-app-pub-4325367879357929/7581160494", AdSize.Banner, AdPosition.Top);
			// Create an empty ad request.
			AdRequest request = new AdRequest.Builder().Build();
			// Load the banner with the request.
			_bannerView.LoadAd(request);
			_bannerView.Hide();
		}

		if(_bannerView != null) _bannerView.Show();

	}
	
	public void HideBanner() {
		if(_bannerView != null) _bannerView.Hide();
	}

	public void SetSystemLoadData(string wwwstr) {
		currentWWWStr = wwwstr;
		string liveVer = "";
		string BetaVer = "";
		string AlphaVer = "";

		// 라이브 버전 체크.
		liveVer = GetServerDataToForm(currentWWWStr);
		if(SystemData.BuildVersion == liveVer) GameServiceType = ServiceType.LIVE;

		if(currentWWWStr.Length <= 0) return;

		// 베터 버전 체크.
		BetaVer = GetServerDataToForm(currentWWWStr);
		if(SystemData.BuildVersion == BetaVer) GameServiceType = ServiceType.BETA;

		if(currentWWWStr.Length <= 0) return;

		// 알파 버전 체크.
		AlphaVer = GetServerDataToForm(currentWWWStr);
		if(SystemData.BuildVersion == AlphaVer) GameServiceType = ServiceType.ALPHA;

		if(currentWWWStr.Length <= 0) return;

		if(currentWWWStr.Length <= 0) return;
	}

	/** 쉐어용 팝업 */
	public void SetSharePopup(byte sortingNum, string comment, string thumbUrl, string nameStr) {
		GameObject alertPopup = Instantiate(Resources.Load<GameObject>("Common/SharePopup")) as GameObject;
		alertPopup.GetComponent<ShareAlertPopup>().init(sortingNum, comment, thumbUrl, nameStr);
	}

	private string GetServerDataToForm(string wwwstr) {
		string returnStr = "";
		int space1Count = wwwstr.IndexOf("/");
		if(space1Count >= 0) {
			returnStr = wwwstr.Substring(0, space1Count);
			currentWWWStr = wwwstr.Substring(space1Count + 1);
		} else {
			returnStr = wwwstr;
			currentWWWStr = "";
		}
		
		return returnStr;
	}
}
