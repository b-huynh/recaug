using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
  Manages Anki-style content for language learning app
  Front side card: Foreign language word
  Back side card: Foreign word AND translation. 3 buttons (confidene levels)
*/
public class Anki : MonoBehaviour
{
    public bool showingAnswer = false; // If true, back side content is shown

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowAnswer()
    {
        // TODO: Change content
        showingAnswer = true;
    }

}
