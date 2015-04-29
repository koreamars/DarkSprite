using UnityEngine;
using System.Collections.Generic;
using Prime31;


public class IABUIManager : MonoBehaviourGUI
{
#if UNITY_ANDROID

	void OnGUI()
	{
		beginColumn();

		if( GUILayout.Button( "Initialize IAB" ) )
		{
			//var key = "your public key from the Android developer portal here";
			var key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg/xyXZxAl+U2OS3t3uXeXdylwEfLHBMYELCG+h4U8ispNqnh9sw20UudIoE5EkYKhw//N9qUtB/Zz+eDgXQUZoMiHZbR0Dor45X/m29yAWlwiWMGXPnXnwr/yrog5G+vBoD74e2KVRMn53VXIU2FLEfBJot8J0LeQNVDG3A/J1q502iKzovBAoza4fykH9VxC9JqUL3dSztk/WmWqVaDJr/eXLwa9i6PWecNlcB+kWxf81lgy9tuGj9iZAYQiUiumJkWJ5FvV4UufD1zL6w1AGZWE9EMW7E+WOBal19lngPW/PeehjqaYPas4XpMi8y/r4sxAj+dnxJYtnr/z43QdwIDAQAB";
			GoogleIAB.init( key );
		}


		if( GUILayout.Button( "Query Inventory" ) )
		{
			// enter all the available skus from the Play Developer Console in this array so that item information can be fetched for them
			//var skus = new string[] { "com.prime31.testproduct", "android.test.purchased", "com.prime31.managedproduct", "com.prime31.testsubscription" };
			var skus = new string[] { "com.ClanaSoft.DarkSprite.managed.core100", 
				"com.ClanaSoft.DarkSprite.purchased.core100", 
				"com.ClanaSoft.DarkSprite.core100", 
				"core100", 
				"com.ClanaSoft.DarkSprite.purchased.core1100", 
				"android.test.purchased" };
				
			GoogleIAB.queryInventory( skus );
		}


		if( GUILayout.Button( "Are subscriptions supported?" ) )
		{
			Debug.Log( "subscriptions supported: " + GoogleIAB.areSubscriptionsSupported() );
		}


		if( GUILayout.Button( "Purchase Test Product" ) )
		{
			//GoogleIAB.purchaseProduct( "android.test.purchased" );
			GoogleIAB.purchaseProduct( "core100");
		}


		if( GUILayout.Button( "Consume Test Purchase" ) )
		{
			//GoogleIAB.consumeProduct( "android.test.purchased" );
			GoogleIAB.consumeProduct( "com.ClanaSoft.DarkSprite.purchased.core100" );
		}


		if( GUILayout.Button( "Test Unavailable Item" ) )
		{
			GoogleIAB.purchaseProduct( "android.test.item_unavailable" );
		}


		endColumn( true );


		if( GUILayout.Button( "Purchase Real Product" ) )
		{
			GoogleIAB.purchaseProduct( "com.prime31.testproduct", "payload that gets stored and returned" );
		}


		if( GUILayout.Button( "Purchase Real Subscription" ) )
		{
			GoogleIAB.purchaseProduct( "com.prime31.testsubscription", "subscription payload" );
		}


		if( GUILayout.Button( "Consume Real Purchase" ) )
		{
			GoogleIAB.consumeProduct( "com.prime31.testproduct" );
		}


		if( GUILayout.Button( "Enable High Details Logs" ) )
		{
			GoogleIAB.enableLogging( true );
		}


		if( GUILayout.Button( "Consume Multiple Purchases" ) )
		{
			var skus = new string[] { "com.prime31.testproduct", "android.test.purchased" };
			GoogleIAB.consumeProducts( skus );
		}

		endColumn();
	}
#endif
}
