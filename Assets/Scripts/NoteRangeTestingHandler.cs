using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteRangeTestingHandler : MonoBehaviour {

    private string lowestText = "Lowest Note in Range: ";
    private Text textObject;
    private Slider lowestSlider;
    private Music.PITCH lowestSupported = Music.PITCH.B0;
    private Music.PITCH highestSupported = Music.PITCH.C8;
    private VirtualInstrumentManager vmm = null;

	// Use this for initialization
	void Start () {
        vmm = GameObject.Find("Main Camera").GetComponent<VirtualInstrumentManager>();
        lowestSlider = gameObject.GetComponent<Slider>();
        textObject = gameObject.transform.GetChild( 3 ).GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        lowestSlider.minValue = (int)lowestSupported;
        lowestSlider.maxValue = (int)highestSupported;
	}

    public void OnLowestSliderValueChanged( float aNewLowestValue )
    {
        lowestText = "Lowest Note In Range: " + Music.NoteToString( (int)aNewLowestValue );
        textObject.text = lowestText;
        vmm.ChangeNoteRange.Invoke( (Music.PITCH)lowestSlider.value );
    }
}
