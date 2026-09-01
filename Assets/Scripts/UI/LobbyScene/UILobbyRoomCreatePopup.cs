using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class UILobbyRoomCreatePopup : MonoBehaviourPunCallbacks
{
    // 방 제목 텍스트.
    [SerializeField]
    private TMP_InputField _txtTitle;

    // 방 인원 수 텍스트.
    [SerializeField]
    private TextMeshProUGUI _txtCount;

    [SerializeField]
    private Button _btnCreate;

    [SerializeField]
    private Button _btnCancel;

    private const int kMIN_COUNT = 4;       // 방 최소 인원.
    private const int kMAX_COUNT = 10;      // 방 최대 인원.

    // 현재 방 인원 수.
    private int _currentCount = kMIN_COUNT;

    void Start()
    {
        _txtCount.text = _currentCount.ToString();
    }

    /// <summary>
    /// 방 생성 버튼.
    /// </summary>
    public void OnClickRoomCreate()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        if (string.IsNullOrWhiteSpace(_txtTitle.text) == true)
        {
            Debug.Log("방 제목을 입력해주세요");
            return;
        }

        // 룸 커스텀 프로퍼티.
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = _currentCount,

            // 방 옵션.
            CustomRoomProperties = new Hashtable()
            {
                {"ROOM_TITLE", _txtTitle.text},
                {"IS_START", false}
            },
            // 로비에서도 보여줄 옵션 (OnRoomListUpdate 콜백 매개변수로 넘길 옵션임).
            CustomRoomPropertiesForLobby = new string[]
            {
                "ROOM_TITLE",
                "IS_START"
            }
        };

        string roomName = string.Format($"{_txtTitle.text}_{System.Guid.NewGuid().ToString("N")}");

        _btnCreate.interactable = false;
        _btnCancel.interactable = false;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    /// <summary>
    /// 방 생성에 실패했을 경우 콜백.
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패 : {returnCode} / {message}");

        _btnCreate.interactable = true;
        _btnCancel.interactable = true;
    }

    /// <summary>
    /// 방 생성 취소 버튼.
    /// </summary>
    public void OnClickCancel()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 인원 수 이전 버튼.
    /// </summary>
    public void OnClickPrev()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        _currentCount--;

        if (_currentCount < kMIN_COUNT)
        {
            _currentCount = kMIN_COUNT;
        }

        _txtCount.text = _currentCount.ToString();
    }

    /// <summary>
    /// 인원 수 다음 버튼.
    /// </summary>
    public void OnClickNext()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        _currentCount++;

        if (_currentCount > kMAX_COUNT)
        {
            _currentCount = kMAX_COUNT;
        }

        _txtCount.text = _currentCount.ToString();
    }
}
