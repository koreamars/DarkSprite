using UnityEngine;
using System.Collections;
using Spine;

public class TextureChangeTest : MonoBehaviour {

    public Sprite sprite;

    [SpineSlot]
    public string slot;

    [SpineSkin]
    public string skin;

	// Use this for initialization
	void Start () {
        
        SkeletonRenderer skeletonRenderer = GetComponent<SkeletonRenderer>();

        Attachment prevAttachment = skeletonRenderer.skeleton.GetAttachment(slot, "eye/type_00/eye_c");
        
        Attachment attachment = skeletonRenderer.skeleton.Data.AddUnitySprite(slot, sprite, skin);

        skeletonRenderer.skeleton.SetAttachment(slot, sprite.name);
       
        
       // SkeletonAnimation skeletonAnimation = GetComponent<SkeletonAnimation>();
        //skeletonAnimation.skeleton.SetAttachment("eye/type_00/eye_c", "eye/type_01/eye_c");
        //skeletonAnimation.skeleton.SetAttachment("eye/type_00/eye_c_b", "eye/type_01/eye_c_b");
        
        print("skin");
        //skeletonAnimation.skeleton.SetAttachment("Base/Head/Back_Hair", "hair/type_02/Back_Hair");
        //skeletonAnimation.skeleton.SetAttachment("Base/Head/Front_Hair", "hair/type_02/Front_Hair");
        
        /*
        var type = AttachmentType.region;

        if (type == AttachmentType.region) {
            //RegionAttachment prevregionAttachment = (RegionAttachment)prevAttachment;
            RegionAttachment regionAttachment = (RegionAttachment)attachment;
           // regionAttachment.x = prevregionAttachment.x;
           // regionAttachment.y = prevregionAttachment.y;
            //regionAttachment.scaleX = prevregionAttachment.scaleX;
           // regionAttachment.scaleY = prevregionAttachment.scaleY;
            regionAttachment.rotation = 270;
            //regionAttachment.width = prevregionAttachment.width;
            //regionAttachment.height = prevregionAttachment.height;
            regionAttachment.UpdateOffset();
        }
         */
	}
	
}
