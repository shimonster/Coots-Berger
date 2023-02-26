using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICallbackManager : MonoBehaviour
{
    public void GameManagerOnUITransition()
    {
        FindObjectOfType<GameManager>().OnUILevelTransitionClosed();
    }
}
