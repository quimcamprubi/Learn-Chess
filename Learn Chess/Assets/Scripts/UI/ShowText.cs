using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
    public string textValue;
    public Text textElement;

    // Update is called once per frame
    void Update()
    {
        textElement.text = textValue;
    }
}
