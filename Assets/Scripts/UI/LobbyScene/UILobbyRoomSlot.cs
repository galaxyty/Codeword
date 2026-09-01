using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UILobbyRoomSlot : MonoBehaviour
{
    // 방 제목 텍스트.
    [SerializeField]
    private TextMeshProUGUI _txtTitle;

    // 방 인원 수 텍스트.
    [SerializeField]
    private TextMeshProUGUI _txtCount;

    [SerializeField]
    private GameObject _objWait;

    [SerializeField]
    private GameObject _objStart;

    // 로딩 팝업.
    private GameObject _loadingPopup;

    // 룸 객체.
    private RoomInfo _roomInfo;

    // 현재 인원 수.
    private int _currentCount;

    // 최대 인원 수.
    private int _maxCount;

    public void SetLoadingPopup(GameObject popup)
    {
        _loadingPopup = popup;
    }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
    }

    public void SetName(string name)
    {
        _txtTitle.text = name;
    }

    public void SetCount(int current, int max)
    {
        _currentCount = current;
        _maxCount = max;

        _txtCount.text = $"{_currentCount}/{_maxCount}";
    }

    /// <summary>
    /// 상태 UI 변경.
    /// </summary>
    public void UpdateStateUI(bool isStart)
    {
        _objWait.SetActive(!isStart);
        _objStart.SetActive(isStart);
    }

    /// <summary>
    /// 방 입장 버튼.
    /// </summary>
    public void OnClickRoom()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        if (_roomInfo.IsOpen == false)
        {
            return;
        }

        _loadingPopup.SetActive(true);

        PhotonNetwork.JoinRoom(_roomInfo.Name);
    }
}
