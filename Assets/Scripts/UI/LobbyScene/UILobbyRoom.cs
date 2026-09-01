using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class UILobbyRoom : MonoBehaviourPunCallbacks
{
    // 룸 리스트 생성 위치.
    [SerializeField]
    private Transform _roomContent;

    // 방 생성 팝업.
    [SerializeField]
    private GameObject _roomCreatePopup;

    [SerializeField]
    private GameObject _loadingPopup;

    // 룸 리스트.
    private Dictionary<string, UILobbyRoomSlot> _dicLobbyRoom = new();

    private void Start()
    {
        // 첫 접속 시.
        if (PhotonNetwork.IsConnectedAndReady == true && PhotonNetwork.InLobby == false)
        {
            // 로비 접속.
            PhotonNetwork.JoinLobby();
        }
    }

    /// <summary>
    /// 서버 연결 성공 시 완료 콜백.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // 로비 접속.
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// 방 정보 변경 시, 로비 입장 시 콜백
    /// </summary>    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 변경 된 방 반복.
        foreach (var room in roomList)
        {
            // 수정 시켜야 할 방인지 확인.
            if (_dicLobbyRoom.TryGetValue(room.Name, out var slot) == true)
            {
                // 삭제 시켜야 할 방인지 확인.
                if (room.RemovedFromList == true)
                {
                    // 오브젝트 제거.
                    Destroy(slot.gameObject);

                    // 더 이상 방이 없으니 딕셔너리에서 제거
                    _dicLobbyRoom.Remove(room.Name);
                }
                else
                {
                    // 방 정보 수정.
                    slot.SetCount(room.PlayerCount, room.MaxPlayers);

                    if (room.CustomProperties.TryGetValue("IS_START", out bool isStart) == true)
                    {
                        slot.SetRoomInfo(room);
                        slot.UpdateStateUI(isStart);
                    }
                }                    
            }
            else
            {
                // 다시 로비로 왔을 때 삭제 된 방 목록은 생성 안하게 예외 처리.
                if (room.RemovedFromList == true)
                {
                    continue;
                }

                // 방이 없으면 새로 만듬.
                slot = Resources.Load<UILobbyRoomSlot>("UILobbyRoomSlot");
                var obj = Instantiate(slot.gameObject, _roomContent);
                var component = obj.GetComponent<UILobbyRoomSlot>();

                component.SetLoadingPopup(_loadingPopup);

                if (room.CustomProperties.TryGetValue("ROOM_TITLE", out var title))
                {
                    component.SetRoomInfo(room);
                    component.SetName(title.ToString());
                    component.SetCount(room.PlayerCount, room.MaxPlayers);
                }

                if (room.CustomProperties.TryGetValue("IS_START", out bool isStart) == true)
                {
                    component.UpdateStateUI(isStart);
                }

                _dicLobbyRoom.Add(room.Name, component);
            }            
        }
    }

    /// <summary>
    /// 방 생성 버튼.
    /// </summary>
    public void OnClickRoomCreate()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        _roomCreatePopup.SetActive(true);
    }

    /// <summary>
    /// 방에 성공적으로 입장했을 경우 콜백.
    /// </summary>
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("RoomScene");
    }

    /// <summary>
    /// 방 생성에 실패했을 경우 콜백.
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패 : {returnCode} / {message}");
    }

    /// <summary>
    /// 방에 입장 실패했을 경우 콜백.
    /// </summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        switch (returnCode)
        {
            case ErrorCode.GameClosed:
                // 게임 중, 방 닫혀 있음 (IsOpen이 true인 경우).
                break;

            case ErrorCode.GameFull:
                // 방 가득 참.
                break;

            default:
                Debug.LogError($"방 입장 실패 : {returnCode} / {message}");
                break;
        }

        _loadingPopup.SetActive(false);
    }

    /// <summary>
    /// 연결 끊길 시 콜백.
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"서버 연결 종료 : {cause}");
    }
}
