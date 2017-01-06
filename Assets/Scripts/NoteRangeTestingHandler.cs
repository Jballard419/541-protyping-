using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteRangeTestingHandler : MonoBehaviour {

#if DEBUG && DEBUG_MUSICAL_TYPING
    private string lowestText = "Lowest Note in Range: C4";
    private Text textObject;
    private Slider lowestSlider;
    private VirtualInstrumentManager vmm = null;
#endif

    // Use this for initialization
    void Start () {

#if DEBUG && DEBUG_MUSICAL_TYPING
        vmm = GameObject.Find("Main Camera").GetComponent<VirtualInstrumentManager>();
        lowestSlider = gameObject.GetComponent<Slider>();
        textObject = gameObject.transform.GetChild( 3 ).GetComponent<Text>();
        lowestSlider.minValue = (int)Music.PITCH.B0;
        lowestSlider.maxValue = (int)Music.PITCH.CS6;
#endif

    }
	
	// Update is called once per frame
	void Update () {

	}

#if DEBUG && DEBUG_MUSICAL_TYPING
    public void OnLowestSliderValueChanged( float aNewLowestValue )
    {
        lowestText = "Range: " + Music.NoteToString( (int)aNewLowestValue ) + " to " + Music.NoteToString( (int)aNewLowestValue + 18 );
        textObject.text = lowestText;
        vmm.ChangeNoteRange.Invoke( (Music.PITCH)lowestSlider.value );
    }
#endif

}
