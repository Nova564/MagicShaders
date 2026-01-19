using System;
using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour
{
    [SerializeField] int _itemID;
    [SerializeField] string _name;
    public string Name{ get { return _name;} set{ _name = value;}}
    public int ID { get { return _itemID;} set{ _itemID = value;}}
}
