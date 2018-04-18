using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeightText : MonoBehaviour {
    Text text;
    private void Awake()
    {
        text = gameObject.GetComponent<Text>();
    }
    // Use this for initialization
    void Start ()
    {

        Debug.Log("changebefore: " + text.preferredHeight);
        text.text = "lkafjdslkjfalsdjflajdfladsjfa\\nfjalsdjflasdjfajd\\nfajdflajdsfjaf\\nfadslkafjdlfja\\nend!";
        Debug.Log("changeafter: " + text.preferredHeight);
        StartCoroutine(getheight());
	}
    IEnumerator getheight()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("After one second : " + text.preferredHeight);
    }

	
	// Update is called once per frame
	void Update () {
		
	}
}
