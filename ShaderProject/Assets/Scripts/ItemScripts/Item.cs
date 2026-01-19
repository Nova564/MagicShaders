using System;
using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour
{
    [SerializeField] int _itemID;
    public string Name{get;set;}
    public int ID { get { return _itemID;} set{ _itemID = value;}}
}
