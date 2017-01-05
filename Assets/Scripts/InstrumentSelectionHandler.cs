using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstrumentSelectionHandler : MonoBehaviour {

    VirtualInstrumentManager vmm = null;

	// Use this for initialization
	void Start () {
        vmm = GameObject.Find( "Main Camera" ).GetComponent<VirtualInstrumentManager>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnInstrumentSelected( int aSelection )
    {
        vmm.ChangeInstrument.Invoke( (Music.INSTRUMENT_TYPE)aSelection );
    }
}
