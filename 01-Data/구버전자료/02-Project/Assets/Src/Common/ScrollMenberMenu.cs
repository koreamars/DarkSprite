using UnityEngine;
using System.Collections;

public class ScrollMenberMenu : ScrollMenu {
	
	public override void Awake () {

		if(isTest) isMemberType = true;

		base.Awake();

		if(isTest) {

			byte count = 15;
			ListMemberModel[] testData = new ListMemberModel[count];
			for(byte i = 0; i < count; i++){
				testData[i] = new ListMemberModel();
				testData[i].id = i;
				testData[i].ClassId = 2;
				testData[i].scriptString = "test : " + i;
			}
			SetScrollData(testData, 1);
			SetScrollView();
		}
	}

	public override void SetScrollView() {
		base.SetScrollView();
		short index = 0;
		ListMemberModel listMemberModel;

		foreach(GameObject btnObj in _BtnObjList)
		{
			listMemberModel = _ListMenuData[index] as ListMemberModel;
			btnObj.GetComponent<ClassMark>().SetSortingNum(sortingNum);
			btnObj.GetComponent<ClassMark>().SetChangeClass(MemberData.getInstence().GetClassModelByClassId(listMemberModel.ClassId).Markid);
			btnObj.GetComponent<CommonBtn>().SetTextAnchor(TextAnchor.MiddleLeft);
			if(btnObj.transform.position.y < _ScrollViewLitY && btnObj.transform.position.y > _ScrollViewMaxY)
			{
				btnObj.GetComponent<ClassMark>().SetEnabled(true);
			}
			else
			{
				btnObj.GetComponent<ClassMark>().SetEnabled(false);
			}

			index ++;
		}
	}

	public override void SetScrollData (ListMenuModel[] dataList, short index)
	{
		ClassModel classModel;
		foreach(ListMemberModel model in dataList) {
			classModel = MemberData.getInstence().GetClassModelByClassId(model.ClassId);
			model.scriptString = "<size=16>[" + ScriptData.getInstence().GetGameScript(classModel.scriptId).script + "]</size> " + model.scriptString;
		}
		base.SetScrollData (dataList, index);
	}
}
