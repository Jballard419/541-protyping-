using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VelocityTextHandler : MonoBehaviour {

    private float velocity = 100f;
    private Text textComponent;
    private string textValue = "Current Velocity: ";

	// Use this for initialization
	void Start () {
        textComponent = GetComponent<Text>();
        textValue = "Current Velocity: " + velocity.ToString();
        textComponent.text = textValue;
	}
	
    public void setSliderTextValue(float aNewValue)
    {
        velocity = aNewValue;
        textValue = "Current Velocity: " + velocity.ToString();
        textComponent.text = textValue;
    }
}
