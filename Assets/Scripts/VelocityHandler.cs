using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VelocityHandler : MonoBehaviour {

#if DEBUG && DEBUG_MUSICAL_TYPING

    private VirtualInstrumentManager vmm = null;
    private Slider[] mSliders = null;
    private Text[] mText = null;
    private string[] keys =
        { "a: ", "w: ", "s: ", "e: ", "d: ", "f: ", "t: ", "g: ", "y: ", "h: ", "u: ", "j: ", "k: ", "o: ", "l: ", "p: ", ";: ", "': ", "]: " };

    public class VelocitySlider : MonoBehaviour
    {
        public VelocityHandler vh = null;
        public Slider slider = null;

        private void Start()
        {
            slider = gameObject.GetComponent<Slider>();
            slider.onValueChanged.AddListener( OnValueChange );
            vh = gameObject.transform.parent.GetComponent<VelocityHandler>();
        }

        private void Update()
        {

        }

        public void OnValueChange( float aValue )
        {
            vh.InvokeChangeKeyVelocityEvent( aValue, slider );
        }
    }

#endif
    // Use this for initialization
    void Start () {

#if DEBUG && DEBUG_MUSICAL_TYPING
        vmm = GameObject.Find( "Main Camera" ).GetComponent<VirtualInstrumentManager>();
        mSliders = new Slider[19];
        mText = new Text[19];
        for( int i = 0; i < 19; i++ )
        {
            mSliders[i] = gameObject.transform.GetChild( i ).GetComponent<Slider>();
            mSliders[i].gameObject.AddComponent<VelocitySlider>();
            mText[i] = gameObject.transform.GetChild( i ).GetChild( 3 ).GetComponent<Text>();
            mText[i].text = keys[i] + "100";
        }

    }
#endif

    // Update is called once per frame
    void Update () {
		
	}

#if DEBUG && DEBUG_MUSICAL_TYPING
    public void InvokeChangeKeyVelocityEvent( float aValue, Slider aSlider )
    {
        for( int i = 0; i < 19; i++ )
        {
            if( aSlider == mSliders[i] )
            {
                mText[i].text = keys[i] + aValue.ToString();
                vmm.DEBUG_ChangeKeyVelocity.Invoke( i, (int)aValue );
            }
        }
    }
#endif

}
