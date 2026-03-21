using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpEntryUI : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI rechargeText;
    public Button equipButton;
    
    [Header("Data")]
    public PowerUpData powerUpData;
    private PowerUpType powerUpType;
    
    public void Setup(PowerUpData data)
    {
        powerUpData = data;
        powerUpType = data.powerUpType;
        
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (powerUpData == null) return;
        
        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = powerUpData.icon;
        }
        
        // Name
        if (nameText != null)
        {
            nameText.text = powerUpData.powerUpName;
        }
        
        // Level
        if (levelText != null && PlayerInventory.Instance != null)
        {
            int level = PlayerInventory.Instance.GetPowerUpLevel(powerUpType);
            levelText.text = $"Lv.{level}";
        }
        
        // Recharge timer
        if (rechargeText != null && PlayerInventory.Instance != null)
        {
            bool isReady = PlayerInventory.Instance.IsPowerUpReady(powerUpType);
            
            if (isReady)
            {
                rechargeText.text = "Ready";
                rechargeText.color = Color.green;
            }
            else
            {
                var progress = PlayerInventory.Instance.GetPowerUpProgress(powerUpType);
                if (progress != null)
                {
                    rechargeText.text = $"{progress.rechargeTimer:F0}s";
                    rechargeText.color = Color.yellow;
                }
            }
        }
        
        // Equip button
        if (equipButton != null)
        {
            bool isReady = PlayerInventory.Instance != null && PlayerInventory.Instance.IsPowerUpReady(powerUpType);
            equipButton.interactable = isReady;
            
            TextMeshProUGUI buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isReady ? "EQUIP" : "WAIT";
            }
        }
    }
    
    public void OnEquipButtonClicked()
    {
        // Activate power-up in game
        if (PowerUpManager.Instance != null && powerUpData != null)
        {
            PowerUpManager.Instance.TryActivatePowerUp(powerUpData);
        }
    }
    
    private void Update()
    {
        // Update recharge timer in real-time
        if (rechargeText != null && PlayerInventory.Instance != null)
        {
            UpdateDisplay();
        }
    }
}