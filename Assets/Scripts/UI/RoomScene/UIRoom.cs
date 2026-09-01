using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRoom : MonoBehaviourPunCallbacks
{
    // 홍팀 슬롯.
    [SerializeField]
    private UIUserSlot[] _redSlots;

    // 청팀 슬롯.
    [SerializeField]
    private UIUserSlot[] _blueSlots;

    // 준비 버튼 (일반 유저용).
    [SerializeField]
    private Button _btnReady;

    [SerializeField]
    private TextMeshProUGUI _txtReady;

    // 시작 버튼 (방장용).
    [SerializeField]
    private Button _btnStart;

    // 팀 버튼.
    [SerializeField]
    private Button _btnRed;

    [SerializeField]
    private Button _btnBlue;

    // 룸 매니저.
    [SerializeField]
    private RoomManager _roomManager;

    // 게임 매니저.
    [SerializeField]
    private GameManager _gameManager;

    private const string _kREADY_FONT = "준비";

    private const string _kREADY_UNLOCK_FONT = "해제";

    void Start()
    {
        _btnStart.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        _btnReady.gameObject.SetActive(!PhotonNetwork.IsMasterClient);

        UpdateAllSlotClearUI();
        
        if (PhotonNetwork.IsMasterClient == true)
        {
            _roomManager.InitializationHost();
        }
        else
        {
            // 이후 들어온 유저들이 방의 모든 유저 UI 갱신.
            foreach (var player in PhotonNetwork.PlayerList)
            {
                UpdateProfileUI(player, player.CustomProperties);
            }
        }
    }

    // 해당 유저 슬롯 번호에 맞는 위치 UI 갱신.
    private void UpdateProfileUI(Player player, Hashtable data)
    {
        // 해당 인덱스 유저 이름 UI 갱신.
        if (data.TryGetValue("INDEX", out int index) == true)
        {
            UpdateNameUI(index, player.NickName);
            UpdateUserProfileUI(index);
        }

        // 이전 인덱스가 오면 슬롯에서 제거.
        if (data.TryGetValue("PREV_INDEX", out int prevIndex) == true)
        {
            UpdateSlotClearUI(prevIndex);
        }

        // 레디 버튼.
        if (data.TryGetValue("IS_READY", out bool isReady) == true)
        {
            UpdateReadyUI((int)player.CustomProperties["INDEX"], isReady);

            if (player == PhotonNetwork.LocalPlayer)
            {
                if (isReady == true)
                {
                    _txtReady.text = _kREADY_UNLOCK_FONT;
                }
                else
                {
                    _txtReady.text = _kREADY_FONT;
                }
            }
        }

        // 팀장 UI.
        if (data.TryGetValue("IS_LEADER", out bool isLeader))
        {
            UpdateLeaderUI((int)player.CustomProperties["INDEX"], isLeader);
        }
    }    

    // 모든 팀 슬롯 내용 비움.
    private void UpdateAllSlotClearUI()
    {
        // 이름 전부 빈 공백.
        for (int i = 0; i < 10; i++)
        {
            UpdateSlotClearUI(i);
        }
    }

    // 해당 유저 닉네임 UI 갱신.
    private void UpdateNameUI(int index, string nickName)
    {
        // 홍팀, 청팀인지에 따른 UI 분배.
        if (SlotUtility.GetIndexTeam(index, out int slotIndex) == ETeam.Red)
        {
            // 짝수일 시 홍팀.
            _redSlots[slotIndex].UpdateNameUI(nickName);
        }
        else
        {
            // 홀수일 시 청팀.
            _blueSlots[slotIndex].UpdateNameUI(nickName);
        }
    }

    // 해당 유저 프로필 갱신.
    private void UpdateUserProfileUI(int index)
    {
        // 홍팀, 청팀인지에 따른 UI 분배.
        if (SlotUtility.GetIndexTeam(index, out int slotIndex) == ETeam.Red)
        {
            // 짝수일 시 홍팀.
            _redSlots[slotIndex].UpdateUserProfileUI();
        }
        else
        {
            // 홀수일 시 청팀.
            _blueSlots[slotIndex].UpdateUserProfileUI();
        }
    }

    // 해당 유저 슬롯 내용 비움.
    private void UpdateSlotClearUI(int index)
    {
        // 홍팀, 청팀인지에 따른 UI 분배.
        if (SlotUtility.GetIndexTeam(index, out int slotIndex) == ETeam.Red)
        {
            // 짝수일 시 홍팀.
            _redSlots[slotIndex].UpdateEmptyUI();
        }
        else
        {
            // 홀수일 시 청팀.
            _blueSlots[slotIndex].UpdateEmptyUI();
        }
    }

    // 해당 유저 슬롯 레디 UI 갱신.
    private void UpdateReadyUI(int index, bool isReady)
    {
        // 홍팀, 청팀인지에 따른 UI 분배.
        if (SlotUtility.GetIndexTeam(index, out int slotIndex) == ETeam.Red)
        {
            // 짝수일 시 홍팀.
            _redSlots[slotIndex].UpdateReadyUI(isReady);
        }
        else
        {
            // 홀수일 시 청팀.
            _blueSlots[slotIndex].UpdateReadyUI(isReady);
        }
    }

    // 해당 유저 팀장 UI 갱신.
    private void UpdateLeaderUI(int index, bool isLeader)
    {
        // 홍팀, 청팀인지에 따른 UI 분배.
        if (SlotUtility.GetIndexTeam(index, out int slotIndex) == ETeam.Red)
        {
            // 짝수일 시 홍팀.
            _redSlots[slotIndex].UpdateLeaderUI(isLeader);
        }
        else
        {
            // 홀수일 시 청팀.
            _blueSlots[slotIndex].UpdateLeaderUI(isLeader);
        }
    }

    /// <summary>
    /// 게임 시작 버튼.
    /// </summary>
    public void OnClickStart()
    {
        if (_gameManager.IsStart == true)
        {
            Debug.Log("게임 중");
            return;
        }

        _roomManager.StartGame();
    }

    /// <summary>
    /// 레디 버튼.
    /// </summary>
    public void OnClickReady()
    {
        if (_gameManager.IsStart == true)
        {
            Debug.Log("게임 중");
            return;
        }

        _roomManager.ToggleReady();
    }

    /// <summary>
    /// 방 나가기 버튼.
    /// </summary>
    public void OnClickExit()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 레드팀으로 변경 버튼.
    /// </summary>
    public void OnClickChangeRed()
    {
        if (_roomManager.IsTeamFull(ETeam.Red) == true)
        {
            return;
        }

        if (_roomManager.IsReady == true)
        {
            Debug.Log("레디를 해제해주세요");
            return;
        }

        // 기존 위치 슬롯 지움.
        // 같은 팀인지 확인.
        if (SlotUtility.GetIndexTeam(_gameManager.UserRoomIndex, out _) == ETeam.Red)
        {
            Debug.Log("이미 레드팀입니다");
            return;
        }

        UpdateSlotClearUI(_gameManager.UserRoomIndex);

        // 홍팀으로 변경.
        _roomManager.ChangeTeam(_gameManager.UserRoomIndex, ETeam.Red);
    }

    /// <summary>
    /// 블루팀으로 변경 버튼.
    /// </summary>
    public void OnClickChangeBlue()
    {
        if (_roomManager.IsTeamFull(ETeam.Blue) == true)
        {
            return;
        }

        if (_roomManager.IsReady == true)
        {
            Debug.Log("레디를 해제해주세요");
            return;
        }

        // 기존 위치 슬롯 지움.
        // 같은 팀인지 확인.
        if (SlotUtility.GetIndexTeam(_gameManager.UserRoomIndex, out _) == ETeam.Blue)
        {
            Debug.Log("이미 블루팀입니다");
            return;
        }

        UpdateSlotClearUI(_gameManager.UserRoomIndex);

        // 청팀으로 변경.
        _roomManager.ChangeTeam(_gameManager.UserRoomIndex, ETeam.Blue);
    }

    /// <summary>
    /// 다른 유저가 방에 입장했을 경우 콜백.
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 호스트가 모든 유저 인덱스 설정시킴.
        if (PhotonNetwork.IsMasterClient == true)
        {
            _roomManager.SetNewPlayerIndex(newPlayer);            
        }        
    }

    /// <summary>
    /// 유저 정보가 업데이트 됐을 때 콜백 (SetCustomProperties로 인한 변경).
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable data)
    {
        UpdateProfileUI(targetPlayer, data);
    }

    /// <summary>
    /// 룸 정보가 갱신 됐을 때 콜백.
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        if (newRoomData.TryGetValue("IS_START", out bool isStart) == true)
        {
            _btnStart.interactable = !isStart;
            _btnReady.interactable = !isStart;
            _btnRed.interactable = !isStart;
            _btnBlue.interactable = !isStart;
        }
    }

    /// <summary>
    /// 다른 유저가 방에 나갈 시 호출.
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.CustomProperties.TryGetValue("INDEX", out int index) == true)
        {
            UpdateSlotClearUI(index);
        }
    }

    /// <summary>
    /// 호스트가 바뀌면 호출.
    /// </summary>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // 호스트한테 레디 버튼을 해제.
        _roomManager.SetHostPlayerReadyOff(newMasterClient);

        _btnStart.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        _btnReady.gameObject.SetActive(!PhotonNetwork.IsMasterClient);        
    }

    /// <summary>
    /// 방에 나갈 시 호출.
    /// </summary>
    public override void OnLeftRoom()
    {
        _roomManager.LeftRoom();
    }    

    /// <summary>
    /// 연결 끊길 시.
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"연결 종료 : {cause}");

        _roomManager.LeftRoom();
    }
}