using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private FerrisWheelQueue ferrisWheelQueue;

    [SerializeField]
    private FerrisWheel ferrisWheel;

    private void Start()
    {
        // Initialize the deck manager
        DeckManager.Instance.InitializeDeck();
        DeckManager.Instance.RegenerateQueue();

        // Initialize the ferris wheel queue
        ferrisWheelQueue.GenerateQueue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ferrisWheel.RotateWheel(false, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ferrisWheel.RotateWheel(false, 2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ferrisWheel.RotateWheel(false, 3);
        }
    }
}
