using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    Entity PlayerStats = null;
    PlayerInput _playerinput = null;
    PlayerController _playerController = null;
    [SerializeField] Image _healthBar;
    [SerializeField] Image _XpBar;
    [SerializeField] TextMeshProUGUI _LvLText;
    [SerializeField] List<Image> _imgList;
    [SerializeField] List<Sprite> _Ps4SpriteList;
    [SerializeField] List<Sprite> _PCSpriteList;
    [SerializeField] List<Image> _CoolDownBarList;
    private List<AtkCoolDown>  _atkCoolDown = new List<AtkCoolDown>();
    [SerializeField] private TextMeshProUGUI _QuestTitle;
    [SerializeField] private TextMeshProUGUI _QuestDetails;
    
    
    
    // Update is called once per frame
    void Update()
    {
        if(PlayerStats == null && FindFirstObjectByType<PlayerMove>() != null)
        {
            PlayerStats = FindFirstObjectByType<PlayerMove>().gameObject.GetComponent<Entity>();
            _playerController = FindFirstObjectByType<PlayerMove>().gameObject.GetComponent<PlayerController>();
            for (int i = 0; i < 4; i++)
            {
                _atkCoolDown.Add(new AtkCoolDown());
            }
            _playerController.OnCoolDownAtk += SetCoolDownAtk;
        }

        if(_playerinput == null && FindFirstObjectByType<PlayerMove>() != null)
        {
            _playerinput = FindFirstObjectByType<PlayerMove>().gameObject.GetComponent<PlayerInput>();
        }
        
        if (PlayerStats != null)
        {
            UpdateHB();
            UpdateXPB();
            UpdateAtkCoolDown();
            
        }

        if (_playerController != null)
        {
            UpdateUIControllerButtons();
            UpdateQuestUI();
        }
    }

    private void OnDestroy()
    {
        _playerController.OnCoolDownAtk -= SetCoolDownAtk;
    }

    void UpdateHB()
    {
        float healthPercent = PlayerStats.Health / PlayerStats.MaxHealth;
        _healthBar.fillAmount = healthPercent;
    }
    void UpdateXPB()
    {
        float XpPercent = PlayerStats.XP / PlayerStats.XPToLVLUP;
        _XpBar.fillAmount = XpPercent;
        _LvLText.text = "LVL " + PlayerStats.LVL.ToString();
    }

    void UpdateUIControllerButtons()
    {
        for (int i = 0; i < _imgList.Count; i++)
        {
            var scheme = _playerinput.currentControlScheme;
            string display = _playerinput.actions["Attack" + (i + 1).ToString()]
                .GetBindingDisplayString(bindingMask: InputBinding.MaskByGroup(scheme));
            if (scheme == "Gamepad")
            {
                switch (display)
                {
                    case "Square":
                        _imgList[i].sprite = _Ps4SpriteList[0];
                        continue;
                    case "Circle":
                        _imgList[i].sprite = _Ps4SpriteList[2];
                        continue;
                    case "Triangle":
                        _imgList[i].sprite = _Ps4SpriteList[1];
                        continue;
                    case "R1":
                        _imgList[i].sprite = _Ps4SpriteList[4];
                        continue;
                }
            }
            else if (scheme == "Keyboard&Mouse")
            {
                switch (display)
                {
                    case "LMB":
                        _imgList[i].sprite = _PCSpriteList[0];
                        continue;
                    case "RMB":
                        _imgList[i].sprite = _PCSpriteList[1];
                        continue;
                    case "&":
                        _imgList[i].sprite = _PCSpriteList[2];
                        continue;
                    case "É":
                        _imgList[i].sprite = _PCSpriteList[3];
                        continue;
                }
            }
        }
    }

    void SetCoolDownAtk(float coolDown, int index)
    {
        _atkCoolDown[index].TimeZero = Time.time;
        _atkCoolDown[index].TimeCooldown = coolDown;
        _atkCoolDown[index].IsInCooldown = true;
    }

    void UpdateQuestUI()
    {
        foreach (Quest quest in _playerController.QuestList)
        {
            _QuestTitle.text = quest.QuestName;
            _QuestDetails.text = quest.QuestLine+ "\n " + quest.ItemReq.name + ": " + quest.ItemActualAmount + "/" + quest.ItemAmountReq;
        }

        if (_playerController.QuestList.Count <= 0)
        {
            _QuestTitle.text = null;
            _QuestDetails.text = null;
        }
        
    }
    
    void UpdateAtkCoolDown()
    {
        for (int i = 0; i < _atkCoolDown.Count; i++)
        {
            if (_atkCoolDown[i].IsInCooldown)
            {
                float CDPercent = (Time.time - _atkCoolDown[i].TimeZero) / _atkCoolDown[i].TimeCooldown;
                _CoolDownBarList[i].fillAmount = CDPercent;
            }
        }
        
    }
}

[System.Serializable]
class AtkCoolDown
{
    public float TimeZero;
    public float TimeCooldown;
    public bool IsInCooldown;
}