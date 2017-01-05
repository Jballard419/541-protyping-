using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteRangeTestingHandler : MonoBehaviour {

    private string lowestText = "Lowest Note in Range: ";
    private Text textObject;
    private Slider lowestSlider;
    private VirtualInstrumentManager vmm = null;

	// Use this for initialization
	void Start () {
        vmm = GameObject.Find("Main Camera").GetComponent<VirtualInstrumentManager>();
        lowestSlider = gameObject.GetComponent<Slider>();
        textObject = gameObject.transform.GetChild( 3 ).GetComponent<Text>();
        lowestSlider.minValue = (int)Music.PITCH.B0;
        lowestSlider.maxValue = (int)Music.PITCH.CS6;
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void OnLowestSliderValueChanged( float aNewLowestValue )
    {
        lowestText = "Lowest Note In Range: " + Music.NoteToString( (int)aNewLowestValue );
        textObject.text = lowestText;
        vmm.ChangeNoteRange.Invoke( (Music.PITCH)lowestSlider.value );
    }
}
