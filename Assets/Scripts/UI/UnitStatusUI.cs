using UnityEngine;
using Character;
using UnityEngine.UI;

public class UnitStatusUI : MonoBehaviour
{
    [HideInInspector]
    public static UnitStatusUI instance;

    public Text nameText;
    public Text atkPwrText;
    public Text defText;
    public Text atkRngText;
    public Text speedText;

    // Use this for initialization
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Unit unit)
    {
        nameText.text = unit.unitName;
        atkPwrText.text = unit.attackPower.ToString();
        defText.text = unit.defence.ToString();
        atkRngText.text = unit.attackRange.ToString();
        speedText.text = unit.speed.ToString();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
