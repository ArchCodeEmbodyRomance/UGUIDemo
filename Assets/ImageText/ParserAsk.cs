using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PicText))]
public class ParserAsk : MonoBehaviour {
    [Multiline]
    public string Text;
    public List<Texture2D> Textures;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    [ContextMenu("SetPicText")]
    void SetPicText()
    {
        PicText pt = GetComponent<PicText>();
        pt.InputDate(Text, Textures);
    }
}
