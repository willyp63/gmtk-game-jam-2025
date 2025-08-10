using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Deck", menuName = "Ferris Wheel/Deck")]
public class DeckData : ScriptableObject
{
    public string deckName = "";

    public List<AnimalWithModifier> animals = new();
}
