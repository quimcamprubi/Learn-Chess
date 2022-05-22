using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour {
    public string textValue;
    public Text textElement;
    
    public void Update() {
        textElement.text = textValue;
    }
}
