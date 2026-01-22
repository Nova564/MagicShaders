using System;
using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour
{
    [SerializeField] int _itemID;
    [SerializeField] string _name;
    [SerializeField] bool _IsGold;
    [SerializeField] int _goldAmount;
    public string Name{ get { return _name;} set{ _name = value;}}
    public int ID { get { return _itemID;} set{ _itemID = value;}}
    public int GoldAmount { get { return _goldAmount;} set{ _goldAmount = value;}}
    public bool IsGold { get { return _IsGold;} set{ _IsGold = value;}}
}
