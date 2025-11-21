using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    public Image fillImage;
    public float maxEnergy = 100f;
    public float currentEnergy;

    void Start()
    {
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy+ amount, 0, maxEnergy);
        UpdateUI();
    }

    public void UseEnergy(float amount){
        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);
        UpdateUI();

        if(currentEnergy <= 0)
        {
            GameManager.I.EndGame();
        }
    }

    void UpdateUI()
    {
        fillImage.fillAmount = currentEnergy / maxEnergy;
    }
}
