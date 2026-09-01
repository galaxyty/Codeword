using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public enum ETeam
{
    Red = 0,
    Blue
}

public class RoomManager : MonoBehaviourPunCallbacks
{    
    // 현재 방 유저들이 가지고 있는 캐싱용 인덱스 리스트.    
    private List<int> _listSearchIndex = new(10);

    /// <summary>
    /// 레디 상태.
    /// </summary>
    public bool IsReady { get; private set; } = false;

    // 해당 팀에서 가장 최소 번호를 가진 빈 인덱스 반환.
    private int GetTeamEmptyMinSlotIndex(ETeam team)
    {
        AddListSearchIndex();

        int index = 0;

        if (team == ETeam.Red)
        {
            // 레드팀.
            // 가지지 않은 인덱스 중 최소 수를 찾음.
            while (_listSearchIndex.Contains(index) == true)
            {
                index += 2;
            }
        }
        else
        {
            // 블루팀.
            ++index;    // 블루팀은 1부터 시작이라 한번 증가.
            while (_listSearchIndex.Contains(index) == true)
            {
                index += 2;
            }
        }

        return index;
    }

    // 가장 최소 번호를 가진 빈 인덱스 반환.
    private int GetEmptyMinSlotIndex()
    {
        AddListSearchIndex();

        int index = 0;

        // 가지지 않은 인덱스 중 최소 수를 찾음.
        while (_listSearchIndex.Contains(index) == true)
        {
            index++;
        }

        return index;
    }

    // 현재 방에 있는 유저 인덱스 생성.
    private void AddListSearchIndex()
    {
        _listSearchIndex.Clear();

        // PlayerList의 순서는 ActorNumber임.
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("INDEX", out int currentIndex) == true)
            {
                _listSearchIndex.Add(currentIndex);
            }
        }
    }

    // 각 팀에 최소 2명 이상인지 여부 반환.
    private bool IsTeamEnough()
    {
        int redCount = 0;
        int blueCount = 0;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("INDEX", out int index))
            {
                if (SlotUtility.GetIndexTeam(index, out _) == ETeam.Red)
                {
                    redCount++;
                }
                else
                {
                    blueCount++;
                }
            }
        }

        // 각 팀에 2명씩 있는지 확인.
        if (redCount < 2 || blueCount < 2)
        {
            return false;
        }

        return true;
    }

    // 현재 바꿀려는 팀 인원이 전부 찼는지 확인.
    public bool IsTeamFull(ETeam team)
    {
        int count = 0;
        int maxCount = PhotonNetwork.CurrentRoom.MaxPlayers;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("INDEX", out int index))
            {
                if (SlotUtility.GetIndexTeam(index, out _) == team)
                {
                    count++;
                }
            }
        }

        // 방 최대 수 홀수면 +1.
        if (maxCount % 2 != 0)
        {
            maxCount++;
        }

        // 팀 변경 불가.
        if ((maxCount / 2) <= count)
        {
            return true;
        }

        return false;
    }

    // 모든 유저가 레디했는지 확인.
    private bool IsAllUserReady()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.MasterClient)
                {
                    continue;
                }

                if (player.CustomProperties.TryGetValue("IS_READY", out bool isReady) == true)
                {
                    if (isReady == false)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 호스트 초기화.
    /// </summary>
    public void InitializationHost()
    {
        // 호스트한텐 0번 부여.
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new()
            {
                {"INDEX", 0}
            }
        );
    }

    /// <summary>
    /// 레디 요청.
    /// </summary>
    public void ToggleReady()
    {
        IsReady = !IsReady;

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new()
            {
                {"IS_READY", IsReady}
            });
    }

    /// <summary>
    /// 게임 시작 요청.
    /// </summary>
    public void StartGame()
    {
        if (IsTeamEnough() == false)
        {
            Debug.Log("팀 인원수가 충분하지 않음");
            return;
        }

        if (IsAllUserReady() == false)
        {
            Debug.Log("모든 유저가 레디를 안함");
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "IS_START", true }
            });
    }

    /// <summary>
    /// 방 퇴장.
    /// </summary>
    public void LeftRoom()
    {
        // 룸에 존재하는 자신의 동기화 데이터 삭제.
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    /// <summary>
    /// 새로 입장한 플레이어한테 인덱스 부여.
    /// </summary>
    public void SetNewPlayerIndex(Player newPlayer)
    {
        // 새로 들어온 유저한테 인덱스 부여.
        newPlayer.SetCustomProperties(
            new()
            {
                { "INDEX", GetEmptyMinSlotIndex() }
            });
    }

    /// <summary>
    /// 호스트의 레디 버튼을 해제.
    /// </summary>
    public void SetHostPlayerReadyOff(Player newMasterClient)
    {
        newMasterClient.SetCustomProperties(
            new()
            {
                {"IS_READY", false}
            }
        );
    }

    /// <summary>
    /// 팀 변경.
    /// </summary>
    public void ChangeTeam(int index, ETeam team)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new()
            {
                { "PREV_INDEX", index},
                { "INDEX", GetTeamEmptyMinSlotIndex(team) }
            });
    }

    /// <summary>
    /// 유저 정보가 업데이트 됐을 때 콜백 (SetCustomProperties로 인한 변경).
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable newData)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer)
        {
            if (newData.TryGetValue("IS_READY", out bool isReady) == true)
            {
                IsReady = isReady;
            }
        }        
    }
}
