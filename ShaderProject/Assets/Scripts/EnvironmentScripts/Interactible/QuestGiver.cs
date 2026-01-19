using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour,  Interactible
{
    [SerializeField] List<Quest> _ListQuests = new List<Quest>();
    public void Interact()
    {
        foreach (var quest in _ListQuests)
        {
            //FindFirstObjectByType<PlayerController>().QuestList.Add(quest);
            
        }
    }
}

[System.Serializable]
public class Quest
{
    public string QuestName;
    public string QuestLine;
    public float ItemAmountReq;
    public Item ItemReq;
    public float ItemActualAmount;
}
