using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATI_DemoSongButtonHandler : MonoBehaviour {

    VirtualInstrumentManager vim = null;

	// Use this for initialization
	void Start () {
        vim = GameObject.Find( "Main Camera" ).GetComponent<VirtualInstrumentManager>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClicked()
    {
        vim.PlaySong.Invoke( Music.GetDemoSong( 120 ) );
    }
}
