using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterReactionUI : MonoBehaviour
{
    private Character character;
    private Slider characterSubReactionBar;
    private float characterSubReactions;

    private Slider characterReactionPip1;
    private Slider characterReactionPip2;

    private int characterReactions;

    private Vector3 originalScale;
    private bool isInvisible = true;

    public Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        camTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        gameObject.GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        character = gameObject.transform.parent.gameObject.GetComponentInParent<Character>();
        characterSubReactionBar = gameObject.transform.GetChild(0).gameObject.GetComponent<Slider>();
        characterReactionPip1 = gameObject.transform.GetChild(1).gameObject.GetComponent<Slider>(); // This is the higher pip.
        characterReactionPip2 = gameObject.transform.GetChild(2).gameObject.GetComponent<Slider>(); 


        characterSubReactionBar.value = 0;
        characterSubReactions = 0;

        characterReactions = 0;
        characterReactionPip1.value = 0;
        characterReactionPip2.value = 0;

        originalScale = transform.localScale;  // Make the UI invisible until an character is hit.
        transform.localScale = new Vector3(0, 0, 0);
        isInvisible = true;

        if (character.reactionResistance >= 3)
        {
            characterReactionPip1.gameObject.SetActive(true);
            characterReactionPip2.gameObject.SetActive(true);
        }
        else if (character.reactionResistance >= 2)
        {
            characterReactionPip1.gameObject.SetActive(true);
            characterReactionPip2.gameObject.SetActive(false);
        }
        else
        {
            characterReactionPip1.gameObject.SetActive(false);
            characterReactionPip2.gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + camTransform.forward);

        if (characterSubReactions != character.freezeSubReactionsTriggered)
        {
            characterSubReactions = character.freezeSubReactionsTriggered;
            characterSubReactionBar.value = characterSubReactions;

            if (isInvisible == true)
            {
                isInvisible = false;
                transform.localScale = originalScale;
            }
            else if (characterReactions == 0 && characterSubReactions == 0)
            {
                originalScale = transform.localScale;  // Make the UI invisible until an character is hit.
                transform.localScale = new Vector3(0, 0, 0);
                isInvisible = true;
            }
        }

        if (characterReactions != character.freezeReactionsTriggered && character.reactionResistance > 1)
        {
            characterReactions = character.freezeReactionsTriggered;

            if (characterReactions >= 2)
            {
                characterReactionPip1.value = 1;
                characterReactionPip2.value = 1;
            }
            else if (characterReactions == 1)
            {
                characterReactionPip1.value = 1;
                characterReactionPip2.value = 0;
            }
            else
            {
                characterReactionPip1.value = 0;
                characterReactionPip2.value = 0;
            }

            if (isInvisible == true)
            {
                isInvisible = false;
                transform.localScale = originalScale;
            }
            else if (characterReactions == 0 && characterSubReactions == 0)
            {
                originalScale = transform.localScale;  // Make the UI invisible until an character is hit.
                transform.localScale = new Vector3(0, 0, 0);
                isInvisible = true;
            }
        }
        
        

    }
}
