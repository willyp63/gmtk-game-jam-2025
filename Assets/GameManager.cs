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
        // Handle ferris wheel rotation with arrow keys
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ferrisWheel.RotateWheel(false); // Counter-clockwise
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ferrisWheel.RotateWheel(true); // Clockwise
        }
    }
}
