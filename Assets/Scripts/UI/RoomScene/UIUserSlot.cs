using TMPro;
using UnityEngine;

public class UIUserSlot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _txtName;

    [SerializeField]
    private GameObject _objUser;

    [SerializeField]
    private GameObject _objReady;

    [SerializeField]
    private GameObject _objLeader;

    /// <summary>
    /// 이름 UI 갱신.
    /// </summary>
    public void UpdateNameUI(string name)
    {
        _txtName.text = name;
    }

    /// <summary>
    /// 레디 UI 갱신.
    /// </summary>
    public void UpdateReadyUI(bool isReady)
    {
        _objReady.SetActive(isReady);
    }

    /// <summary>
    /// 슬롯 UI 빈 칸으로 갱신.
    /// </summary>
    public void UpdateEmptyUI()
    {
        _txtName.text = string.Empty;
        _objUser.SetActive(false);
        _objReady.SetActive(false);
        _objLeader.SetActive(false);
    }

    /// <summary>
    /// 팀장 UI 갱신.
    /// </summary>
    public void UpdateLeaderUI(bool isLeader)
    {
        _objLeader.SetActive(isLeader);
    }

    /// <summary>
    /// 유저 프로필 UI 갱신.
    /// </summary>
    public void UpdateUserProfileUI()
    {
        _objUser.SetActive(true);
    }
}
