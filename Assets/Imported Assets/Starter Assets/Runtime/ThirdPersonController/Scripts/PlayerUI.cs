using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Image hpBar;
    public Image spBar;

    public PlayerStatus playerStatus;

    public void UpdateUI()
    {
        if (playerStatus != null)
        {
            if (hpBar != null)
            {
                hpBar.fillAmount = playerStatus.currentHp / playerStatus.maxHp;
            }
            
            if (spBar != null)
            {
                spBar.fillAmount = playerStatus.currentSp / playerStatus.maxSp;
            }
        }
    }
}
