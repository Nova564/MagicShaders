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
    [SerializeField] List<Image> _CoolDownBarList;
    [SerializeField] List<Image> _TelegraphIndicatorList;
    [SerializeField] TextMeshProUGUI _goldText;
    private List<AtkCoolDown> _atkCoolDown = new List<AtkCoolDown>();

    [Header("Telegraph Visual Feedback")]
    [SerializeField] private Color _telegraphColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color _normalColor = Color.white;

    void Start()
    {
        PlayerController.OnCoolDownAtk += SetCoolDownAtk;
        PlayerController.OnSpellStateChanged += OnSpellStateChanged;
    }

    void Update()
    {
        if (PlayerStats == null && FindFirstObjectByType<PlayerMove>() != null)
        {
            PlayerStats = FindFirstObjectByType<PlayerMove>().gameObject.GetComponent<Entity>();
            _playerController = FindFirstObjectByType<PlayerMove>().gameObject.GetComponent<PlayerController>();

            for (int i = 0; i < _CoolDownBarList.Count; i++)
            {
                _atkCoolDown.Add(new AtkCoolDown());
            }
        }

        if (_playerinput == null && FindFirstObjectByType<PlayerMove>() != null)
        {
            _playerinput = FindFirstObjectByType<PlayerMove>().gameObject.GetComponent<PlayerInput>();
        }

        if (PlayerStats != null)
        {
            UpdateHB();
            UpdateAtkCoolDown();
            UpdateGold();
        }
    }

    void OnDestroy()
    {
        PlayerController.OnCoolDownAtk -= SetCoolDownAtk;
        PlayerController.OnSpellStateChanged -= OnSpellStateChanged;
    }

    void UpdateHB()
    {
        float healthPercent = PlayerStats.Health / PlayerStats.MaxHealth;
        _healthBar.fillAmount = healthPercent;
    }

    void UpdateGold()
    {
        if (PlayerStats.GOLD <= 1)
        {
            _goldText.text = PlayerStats.GOLD.ToString() + " GOLD";
        }
        else
        {
            _goldText.text = PlayerStats.GOLD.ToString() + " GOLDS";
        }
    }

    void SetCoolDownAtk(float coolDown, int index)
    {
        if (index < 0 || index >= _atkCoolDown.Count) return;

        _atkCoolDown[index].TimeZero = Time.time;
        _atkCoolDown[index].TimeCooldown = coolDown;
        _atkCoolDown[index].IsInCooldown = true;
    }

    void OnSpellStateChanged(int spellIndex, bool isTelegraph)
    {
        if (spellIndex < 0 || spellIndex >= _TelegraphIndicatorList.Count) return;

        if (isTelegraph)
        {
            _TelegraphIndicatorList[spellIndex].color = _telegraphColor;
        }
        else
        {
            _TelegraphIndicatorList[spellIndex].color = _normalColor;
        }
    }

    void UpdateAtkCoolDown()
    {
        for (int i = 0; i < _atkCoolDown.Count; i++)
        {
            if (_atkCoolDown[i].IsInCooldown)
            {
                float CDPercent = (Time.time - _atkCoolDown[i].TimeZero) / _atkCoolDown[i].TimeCooldown;

                if (CDPercent >= 1f)
                {
                    _atkCoolDown[i].IsInCooldown = false;
                    _CoolDownBarList[i].fillAmount = 1f;
                }
                else
                {
                    _CoolDownBarList[i].fillAmount = CDPercent;
                }
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