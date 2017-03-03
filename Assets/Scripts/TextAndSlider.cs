using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextAndSlider : MonoBehaviour, IEndDragHandler
{
	private Music.PITCH mValue = Music.PITCH.C3;
	private Text mText = null;
	private Slider mSlider = null;
	private VirtualInstrumentManager mVIM = null;

	private void Start()
	{
		// Get the slider and add its listener.
		mSlider = gameObject.GetComponent<Slider>();
		mSlider.onValueChanged.AddListener( OnSliderValueChanged );
		Assert.IsNotNull( mSlider, "Text/Slider handler could not find the slider!" );

		mVIM = GameObject.Find("VirtualInstrumentManager").GetComponent<VirtualInstrumentManager>();
		Assert.IsNotNull( mVIM, "Text/Slider handler could not find the Virtual Instrument Manager!" );
	}

	public Music.PITCH GetValue()
	{
		return mValue;
	}

	public void OnSliderValueChanged( float aNewValue )
	{
		mValue = (Music.PITCH)aNewValue;
		mText.text = Music.NoteToString(mValue);
	}

	public void OnEndDrag( PointerEventData aPointerData )
	{
		mValue = (Music.PITCH)mSlider.value;
		mText.text = Music.NoteToString(mValue);
		mVIM.ChangeNoteRange.Invoke (mValue);
	}

}
