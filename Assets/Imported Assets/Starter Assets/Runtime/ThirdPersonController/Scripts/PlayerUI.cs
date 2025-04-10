using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Image hpBar;
    public Image spBar;

    public PlayerStatus playerStatus;

    public void UpdateUI()
    {
        Debug.Log("UpdateUI 호출됨");
        if (playerStatus != null)
        {
            if (hpBar != null)
            {
                hpBar.fillAmount = playerStatus.currentHp / playerStatus.maxHp;
            }
            
            if (spBar != null)
            {
                Debug.Log("SP 바 업데이트: " + playerStatus.currentSp + " / " + playerStatus.maxSp);
                spBar.fillAmount = playerStatus.currentSp / playerStatus.maxSp;
            }
        }
    }
}
