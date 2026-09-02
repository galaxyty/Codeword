using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public enum EState
{
    Wait = 0,           // 대기.
    Leader,             // 팀장 정하기.
    AttackFirst,        // 선공 정하기.
    Word,               // 단어 선별.
    SettingCard,        // 카드판 셋팅.
    SettingSyncCard,    // 카드 데이터 동기화.
    RedLeaderTurn,      // 레드팀 팀장 차례.
    RedMemberTurn,      // 레드팀 팀원 차례.
    BlueLeaderTurn,     // 블루팀 팀장 차례.
    BlueMemberTurn,     // 블루팀 팀원 차례.
    RedLose,            // 레드팀 패배.
    BlueLose,           // 블루팀 패배.
    RedLeaderOut,       // 레드팀 리더 퇴장.
    BlueLeaderOut,      // 블루팀 리더 퇴장.
    GameOver,           // 팀장 혹은 팀원 전부 이탈로 인한 게임 종료.
}

public class GameManager : MonoBehaviourPunCallbacks
{
    // 카드 힌트 갯수.    
    private int _cardHintCount = 0;

    // 현재 턴 수.
    private int _currentHintCount = 0;

    // 코드네임 단어들.
    private WordData _wordData;

    // 코드네임 단어 복사용 리스트.
    private List<string> _copyWordList = new();

    // 카드판.
    private Card[] _cardBoard = new Card[25];

    // 남은 카드 수 배열.
    private int[] _remainCountArr = new int[4];

    // UI 카드판.
    private List<UICardSlot> _listCardSlotUI = new();

    // UI 남은 카드판.
    [SerializeField]
    private List<UIRemainCardSlot> _listRemainCardSlot = new();

    // 각 팀 플레이어들.
    private List<Player> _redPlayers = new();
    private List<Player> _bluePlayers = new();

    // 각 팀 인원 수.
    private int _redTeamCount;
    private int _blueTeamCount;

    // 각 팀 리더.
    private Player _redLeader;
    private Player _blueLeader;

    // 각 팀 리더 고유 번호.
    private int _redLeaderActor;
    private int _blueLeaderActor;

    // TODO :: 포톤은 배열로 데이터를 보낼 때 원시 자료타입만 지원한다.
    // 클래스, 리스트 등 타입을 지원 안한다...
    private int[] _indexArr = new int[25];
    private string[] _nameArr = new string[25];             // 이름.
    private int[] _typeArr = new int[25];                   // 타입.

    // 추출 단어.
    private string[] _wordArray = new string[25];

    private List<int> _randomIndexes = new(25);

    /// <summary>
    /// 유저 방 인덱스.
    /// </summary>
    public int UserRoomIndex { get; private set; }

    /// <summary>
    /// 현재 게임 상태.
    /// </summary>
    public EState State { get; private set; } = EState.Wait;

    /// <summary>
    /// 리더 여부.
    /// </summary>
    public bool IsLeader { get; private set; }

    /// <summary>
    /// 게임 시작 여부.
    /// </summary>
    public bool IsStart { get; private set; } = false;

    private void Start()
    {
        var textAsset = Resources.Load<TextAsset>("json/WORDS");
        _wordData = JsonUtility.FromJson<WordData>(textAsset.text);

        for (int i = 0; i < _cardBoard.Length; i++)
        {
            _cardBoard[i] = new();
            _cardBoard[i].Type = ECard.Basic;
        }
    }

    private void SetWait()
    {
        PhotonNetwork.CurrentRoom.IsOpen = true;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "IS_START", false }
            });

        if (_redLeader != null)
        {
            _redLeader.SetCustomProperties(new()
            {
                { "IS_LEADER", false}
            });
        }
        
        if (_blueLeader != null)
        {
            _blueLeader.SetCustomProperties(new()
            {
                { "IS_LEADER", false}
            });
        }
    }

    // 각 팀에서 팀장 뽑기.
    private void SetTeamLeader()
    {
        // 다음 차례로 교체.
        State = EState.AttackFirst;

        _redPlayers.Clear();
        _bluePlayers.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            // 인덱스 여부 확인.
            if (player.CustomProperties.TryGetValue("INDEX", out int index) == false)
            {
                continue;
            }                

            if (SlotUtility.GetIndexTeam(index, out _) == ETeam.Red)
            {
                _redPlayers.Add(player);
            }                
            else
            {
                _bluePlayers.Add(player);
            }                
        }

        if (_redPlayers.Count <= 0 || _bluePlayers.Count <= 0)
        {
            return;
        }

        Player redLeader = _redPlayers[Random.Range(0, _redPlayers.Count)];
        Player blueLeader = _bluePlayers[Random.Range(0, _bluePlayers.Count)];

        _redLeader = redLeader;
        _blueLeader = blueLeader;

        if (_redLeader != null)
        {
            _redLeader.SetCustomProperties(new()
            {
                { "IS_LEADER", true}
            });
        }        

        if (_blueLeader != null)
        {
            _blueLeader.SetCustomProperties(new()
            {
                { "IS_LEADER", true}
            });
        }        

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "STATE", State},
                { "LEADER_RED", redLeader.ActorNumber},
                { "LEADER_BLUE", blueLeader.ActorNumber},
                { "RED_TEAM_COUNT", _redPlayers.Count},
                { "BLUE_TEAM_COUNT", _bluePlayers.Count}
            });
    }

    // 선공 정하기 (0이면 레드 선공, 1이면 블루 선공).
    private void SetAttackFirst()
    {
        // 다음 차례로 교체.
        State = EState.Word;

        int attackFirst = Random.Range(0, 2);

        if (attackFirst == (int)ECard.Red)
        {
            _remainCountArr[(int)ECard.Red] = 9;
            _remainCountArr[(int)ECard.Blue] = 8;
        }
        else
        {
            _remainCountArr[(int)ECard.Red] = 8;
            _remainCountArr[(int)ECard.Blue] = 9;
        }

        _remainCountArr[(int)ECard.Neutral] = 8;
        _remainCountArr[(int)ECard.Joker] = 1;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "STATE", State},
                { "ATTACK_FIRST", attackFirst},
                { "REMAIN_COUNT_ARRAY", _remainCountArr}
            });
    }
    
    // 단어 선별.
    private void SetWord()
    {
        // 다음 차례로 교체.
        State = EState.SettingCard;

        SetRandomWord();

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "STATE", State}
            });
    }

    // 보드판 셋팅.
    private void SetCardBoard()
    {
        // 다음 차례로 교체.
        State = EState.SettingSyncCard;

        SetCardData();        

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "STATE", State},
                { "CARD_INDEX_ARRAY", _indexArr},
                { "CARD_NAME_ARRAY", _nameArr},
                { "CARD_TYPE_ARRAY", _typeArr},
                { "REMAIN_COUNT_ARRAY", _remainCountArr}
            });
    }    

    // 랜덤으로 25개 단어 추출.
    private void SetRandomWord()
    {
        // 원본 데이터 복제.
        _copyWordList.Clear();
        _copyWordList.AddRange(_wordData.WORDS);

        for (int i = 0; i < 25; i++)
        {
            int randomIndex = Random.Range(0, _copyWordList.Count);

            _wordArray[i] = _copyWordList[randomIndex];
            _copyWordList.RemoveAt(randomIndex);
        }
    }

    // 랜덤 보드판 생성.
    private void SetCardData()
    {
        _randomIndexes.Clear();

        // 0~24 인덱스 생성
        for (int i = 0; i < 25; i++)
        {
            _randomIndexes.Add(i);
        }
        
        for (int i = _randomIndexes.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);
            (_randomIndexes[i], _randomIndexes[random]) = (_randomIndexes[random], _randomIndexes[i]);
        }

        ECard firstTeam;
        ECard secondTeam;

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ATTACK_FIRST"] == 0)
        {
            firstTeam = ECard.Red;
            secondTeam = ECard.Blue;
        }
        else
        {
            firstTeam = ECard.Blue;
            secondTeam = ECard.Red;
        }

        int index = 0;

        // 선공 9장
        for (int i = 0; i < 9; i++)
        {
            if (_cardBoard[_randomIndexes[index]] == null)
            {
                _cardBoard[_randomIndexes[index]] = new();
            }

            _cardBoard[_randomIndexes[index]].Index = _randomIndexes[index];
            _cardBoard[_randomIndexes[index]].Type = firstTeam;
            _cardBoard[_randomIndexes[index]].Name = _wordArray[_randomIndexes[index]];

            index++;
        }

        // 후공 8장
        for (int i = 0; i < 8; i++)
        {
            if (_cardBoard[_randomIndexes[index]] == null)
            {
                _cardBoard[_randomIndexes[index]] = new();
            }

            _cardBoard[_randomIndexes[index]].Index = _randomIndexes[index];
            _cardBoard[_randomIndexes[index]].Type = secondTeam;
            _cardBoard[_randomIndexes[index]].Name = _wordArray[_randomIndexes[index]];

            index++;
        }

        // 조커 1장
        if (_cardBoard[_randomIndexes[index]] == null)
        {
            _cardBoard[_randomIndexes[index]] = new();
        }

        _cardBoard[_randomIndexes[index]].Index = _randomIndexes[index];
        _cardBoard[_randomIndexes[index]].Type = ECard.Joker;
        _cardBoard[_randomIndexes[index]].Name = _wordArray[_randomIndexes[index]];

        index++;

        // 중립 7장
        while (index < _randomIndexes.Count)
        {
            if (_cardBoard[_randomIndexes[index]] == null)
            {
                _cardBoard[_randomIndexes[index]] = new();
            }

            _cardBoard[_randomIndexes[index]].Index = _randomIndexes[index];
            _cardBoard[_randomIndexes[index]].Type = ECard.Neutral;
            _cardBoard[_randomIndexes[index]].Name = _wordArray[_randomIndexes[index]];

            index++;
        }

        // 포톤에 보낼 배열에 각 속성 추가.
        for (int i = 0; i < 25; i++)
        {
            _cardBoard[i].IsOpen = false;
            _cardBoard[i].IsToggle = false;

            _indexArr[i] = _cardBoard[i].Index;
            _nameArr[i] = _cardBoard[i].Name;
            _typeArr[i] = (int)_cardBoard[i].Type;
        }
    }

    // 호스트가 셋팅한 카드 데이터로 동기화.
    private void SetSyncCardData(Hashtable newRoomData)
    {
        // 다음 차례로 교체.
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ATTACK_FIRST"] == 0)
        {
            State = EState.RedLeaderTurn;
        }
        else
        {
            State = EState.BlueLeaderTurn;
        }

        if (newRoomData.TryGetValue("CARD_INDEX_ARRAY", out var cardIndexArr) == true)
        { }

        if (newRoomData.TryGetValue("CARD_NAME_ARRAY", out var cardNameArr) == true)
        { }

        if (newRoomData.TryGetValue("CARD_TYPE_ARRAY", out var cardTypeArr) == true)
        { }        

        int[] indexes = (int[])cardIndexArr;
        string[] names = (string[])cardNameArr;
        int[] types = (int[])cardTypeArr;

        for (int i = 0; i < 25; i++)
        {
            Card card = _cardBoard[i];

            card.Index = indexes[i];
            card.Name = names[i];
            card.Type = (ECard)types[i];
            card.IsToggle = false;
            card.IsOpen = false;
        }        

        if (PhotonNetwork.IsMasterClient == true)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new()
                {
                    { "STATE", State}
                });
        }        
    }

    // 팀장 차례.
    private void SetLeader()
    {
        _cardHintCount = 0;
        _currentHintCount = 0;
    }

    // 팀 패배.
    private void SetTeamLose()
    {
        StartCoroutine(CoResultWait(7f));
    }

    // 리더 퇴장으로 인한 게임 종료.
    private void SetLeaderOutLose()
    {
        StartCoroutine(CoResultWait(5f));
    }

    // 리더 조건 갱신.
    private void UpdateLeaderState()
    {
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;

        int red = (int)PhotonNetwork.CurrentRoom.CustomProperties["LEADER_RED"];
        int blue = (int)PhotonNetwork.CurrentRoom.CustomProperties["LEADER_BLUE"];

        IsLeader = actor == red || actor == blue;
    }

    // 카드 오픈 RPC.
    [PunRPC]
    private void OpenCardRPC(int index)
    {
        ECard type = _cardBoard[index].Type;

        _cardBoard[index].IsOpen = true;
        _cardBoard[index].IsToggle = false;

        _listCardSlotUI[index].UpdateOpenUI();
        _listRemainCardSlot[(int)type].UpdateCountUI(_remainCountArr[(int)type]);
    }

    // 마스터 클라이언트가 카드 결과 판정.
    [PunRPC]
    private void ResultCardRPC(int cardIndex, int index)
    {
        var team = SlotUtility.GetIndexTeam(index, out _);

        // 같은 카드 동시에 누를 경우 방어.
        Card card = _cardBoard[cardIndex];

        if (card.IsOpen == true)
        {
            return;
        }        

        // 선택한 유저가 레드팀인지 확인.
        if (team == ETeam.Red)
        {
            // 하지만 레드팀 차례가 아니면 리턴 처리.
            if (State != EState.RedMemberTurn)
            {
                return;
            }
        }

        // 선택한 유저가 블루팀인지 확인.
        if (team == ETeam.Blue)
        {
            // 하지만 블루팀 차례가 아니면 리턴 처리.
            if (State != EState.BlueMemberTurn)
            {
                return;
            }
        }

        // 선택한 카드 수 감소.
        _remainCountArr[(int)card.Type]--;

        // 패배 판정.
        if (card.Type == ECard.Joker && State == EState.RedMemberTurn)
        {
            // 레드팀 패배.
            State = EState.RedLose;
        }
        else if (card.Type == ECard.Joker && State == EState.BlueMemberTurn)
        {
            // 블루팀 패배.
            State = EState.BlueLose;
        }
        else if (_remainCountArr[(int)card.Type] <= 0)
        {
            // 카드 전부 소모한 팀 승리.
            switch (card.Type)
            {
                case ECard.Red:
                    // 블루 팀 패배.
                    State = EState.BlueLose;
                    break;

                case ECard.Blue:
                    // 레드 팀 패배.
                    State = EState.RedLose;
                    break;

                default:
                    break;
            }
        }

        // 현재 턴 수 증가.
        _currentHintCount++;

        // 턴 종료 판정.
        if ((card.Type == ECard.Neutral || card.Type == ECard.Blue) && State == EState.RedMemberTurn)
        {
            // 레드 차례 종료.
            State = EState.BlueLeaderTurn;
        }
        else if ((card.Type == ECard.Neutral || card.Type == ECard.Red) && State == EState.BlueMemberTurn)
        {
            // 블루 차례 종료.
            State = EState.RedLeaderTurn;
        }
        else if (card.Type == ECard.Red && State == EState.RedMemberTurn && _currentHintCount >= _cardHintCount)
        {
            // 모든 횟수 끝나서 레드 차례 종료.
            State = EState.BlueLeaderTurn;
        }
        else if (card.Type == ECard.Blue && State == EState.BlueMemberTurn && _currentHintCount >= _cardHintCount)
        {
            // 모든 횟수 끝나서 블루 차례 종료.
            State = EState.RedLeaderTurn;
        }        

        // UI 갱신 동기화.
        photonView.RPC(
            "OpenCardRPC",
            RpcTarget.All,
            cardIndex);

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "STATE", State},
                { "HINT_CURRENT_COUNT", _currentHintCount},
                { "REMAIN_COUNT_ARRAY", _remainCountArr}
            });
    }

    // 일정 시간 뒤 Wait 상태로 변경.
    private System.Collections.IEnumerator CoResultWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (PhotonNetwork.IsMasterClient == false)
        {
            yield break;
        }

        State = EState.Wait;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new()
            {
                { "STATE", State }
            });
    }

    public Card GetBoard(int index)
    {
        return _cardBoard[index];
    }

    public void AddUISlot(UICardSlot slot)
    {
        _listCardSlotUI.Add(slot);
    }    

    /// <summary>
    /// 힌트 수 증가/감소.
    /// </summary>
    public void ToggleCardHint(bool isToggle)
    {
        if (isToggle == true)
        {
            _cardHintCount++;
        }
        else
        {
            _cardHintCount--;
        }
    }
    
    // 게임 시작.
    private void GameStart(Hashtable data)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        if (data.TryGetValue("IS_START", out bool isStart) == true)
        {
            if (isStart == true)
            {
                // 모든 유저 레디 상태 해제.
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    player.SetCustomProperties(new()
                    {
                        {"IS_READY", false}
                    });
                }

                // 팀장 정함.
                State = EState.Leader;

                PhotonNetwork.CurrentRoom.SetCustomProperties(
                    new()
                    {
                        { "STATE", State}
                    });
            }
        }
    }

    /// <summary>
    /// 힌트 룸 데이터 갱신.
    /// </summary>
    public void SendHint(string title)
    {
        if (State == EState.RedLeaderTurn)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new()
                {
                    { "STATE", EState.RedMemberTurn},
                    { "HINT_TITLE", title},
                    { "HINT_CURRENT_COUNT", 0},
                    { "HINT_COUNT", _cardHintCount}
                });
        }
        else if (State == EState.BlueLeaderTurn)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new()
                {
                    { "STATE", EState.BlueMemberTurn},
                    { "HINT_TITLE", title},
                    { "HINT_CURRENT_COUNT", 0},
                    { "HINT_COUNT", _cardHintCount}
                });
        }
    }

    /// <summary>
    /// 현재 내 턴인지 확인.
    /// </summary>
    public bool IsMyTurn()
    {
        var team = SlotUtility.GetIndexTeam(UserRoomIndex, out _);

        return (team == ETeam.Red && (State == EState.RedLeaderTurn || State == EState.RedMemberTurn))
            || (team == ETeam.Blue && (State == EState.BlueLeaderTurn || State == EState.BlueMemberTurn));
    }

    /// <summary>
    /// 현재 리더가 자기 턴인지 확인.
    /// </summary>
    public bool IsLeaderTurn()
    {
        var team = SlotUtility.GetIndexTeam(UserRoomIndex, out _);

        return (team == ETeam.Red && State == EState.RedLeaderTurn)
            || (team == ETeam.Blue && State == EState.BlueLeaderTurn);
    }

    /// <summary>
    /// 현재 멤버가 자기 턴인지 확인.
    /// </summary>
    public bool IsMemberTurn()
    {
        var team = SlotUtility.GetIndexTeam(UserRoomIndex, out _);

        return (team == ETeam.Red && State == EState.RedMemberTurn)
            || (team == ETeam.Blue && State == EState.BlueMemberTurn);
    }

    /// <summary>
    /// 자기 팀 카드인지 확인.
    /// </summary>
    public bool IsCardTeam(Card card)
    {
        var team = SlotUtility.GetIndexTeam(UserRoomIndex, out _);

        return (ETeam)card.Type == team;
    }

    /// <summary>
    /// 유저 정보가 업데이트 됐을 때 콜백 (SetCustomProperties로 인한 변경).
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable newData)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer)
        {
            if (newData.TryGetValue("INDEX", out int index) == true)
            {
                UserRoomIndex = index;
            }
        }        
    }

    /// <summary>
    /// 방 정보 갱신됐을 때 콜백.
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        GameStart(newRoomData);

        if (newRoomData.TryGetValue("STATE", out EState state) == true)
        {
            // 마스터 클라이언트만 동기화.
            if (PhotonNetwork.IsMasterClient == true)
            {
                switch (state)
                {
                    case EState.Wait:
                        SetWait();
                        break;

                    case EState.Leader:
                        // 팀장 정하기.
                        SetTeamLeader();                        
                        break;

                    case EState.AttackFirst:
                        // 선공 정하기.
                        SetAttackFirst();                        
                        break;

                    case EState.Word:
                        // 단어 추출 시작.
                        SetWord();
                        break;

                    case EState.SettingCard:
                        // 카드판 셋팅.
                        SetCardBoard();
                        break;

                    default:
                        break;
                }
            }

            // 모든 유저 동기화.
            switch (state)
            {
                case EState.SettingSyncCard:
                    // 마스터가 셋팅한 카드 데이터로 동기화.
                    SetSyncCardData(newRoomData);
                    break;

                case EState.RedLeaderTurn:
                    // 레드 팀장 차례.
                    SetLeader();
                    break;

                case EState.BlueLeaderTurn:
                    // 파란 팀장 차례.
                    SetLeader();
                    break;

                case EState.RedLose:
                    // 레드 패배.
                    SetTeamLose();
                    break;

                case EState.BlueLose:
                    // 블루 패배.
                    SetTeamLose();
                    break;

                case EState.RedLeaderOut:
                    // 레드 리더 강제 퇴장.
                    SetLeaderOutLose();
                    break;

                case EState.BlueLeaderOut:
                    // 블루 리더 강제 퇴장.
                    SetLeaderOutLose();
                    break;
            }

            State = state;
        }

        // 룸 데이터 동기화 (마스터 클라이언트가 나갈 경우를 대비하여 데이터 동기화 시킴).
        if (newRoomData.TryGetValue("HINT_COUNT", out int hintCount) == true)
        {
            _cardHintCount = hintCount;
        }

        if (newRoomData.TryGetValue("HINT_CURRENT_COUNT", out int hintCurrentCount) == true)
        {
            _currentHintCount = hintCurrentCount;
        }

        if (newRoomData.TryGetValue("REMAIN_COUNT_ARRAY", out var remainCountArr) == true)
        {
            _remainCountArr = (int[])remainCountArr;
        }

        if (newRoomData.TryGetValue("LEADER_RED", out int redActorNumber) == true)
        {
            _redLeaderActor = redActorNumber;

            UpdateLeaderState();
        }

        if (newRoomData.TryGetValue("LEADER_BLUE", out int blueActorNumber) == true)
        {
            _blueLeaderActor = blueActorNumber;

            UpdateLeaderState();
        }

        if (newRoomData.TryGetValue("IS_START", out bool isStart) == true)
        {
            IsStart = isStart;
        }

        if (newRoomData.TryGetValue("RED_TEAM_COUNT", out int redTeamCount) == true)
        {
            _redTeamCount = redTeamCount;
        }

        if (newRoomData.TryGetValue("BLUE_TEAM_COUNT", out int blueTeamCount) == true)
        {
            _blueTeamCount = blueTeamCount;
        }
    }

    /// <summary>
    /// 유저 나갔을 때 콜백.
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            // 게임 결과 판정에서는 무시.
            if (State == EState.RedLeaderOut || State == EState.BlueLeaderOut ||
                State == EState.RedLose || State == EState.BlueLose) return;

            // 게임 중에만.
            if (IsStart == true)
            {
                // 팀 확인.
                var team = SlotUtility.GetIndexTeam((int)otherPlayer.CustomProperties["INDEX"], out _);

                // 팀장 나가면 게임 강제 종료.
                if (otherPlayer.CustomProperties.TryGetValue("IS_LEADER", out bool isLeader) == true)
                {
                    // 리더인지 확인.
                    if (isLeader == true)
                    {
                        // 게임 종료 처리
                        if (team == ETeam.Red)
                        {
                            State = EState.RedLeaderOut;
                        }
                        else
                        {
                            State = EState.BlueLeaderOut;
                        }

                        PhotonNetwork.CurrentRoom.SetCustomProperties(
                            new()
                            {
                                { "STATE", State }
                            });

                        return;
                    }
                }

                // 팀원 아무도 없으면 게임 강제 종료.
                if (team == ETeam.Red)
                {
                    _redTeamCount--;
                }
                else
                {
                    _blueTeamCount--;
                }

                if (_redTeamCount <= 1)
                {
                    State = EState.RedLeaderOut;
                }
                else
                {
                    State = EState.BlueLeaderOut;
                }

                // 게임 종료 처리
                PhotonNetwork.CurrentRoom.SetCustomProperties(
                    new()
                    {
                        { "STATE", State }
                    });
            }
        }
    }

    /// <summary>
    /// 마스터 클라이언트가 변경 됐을 경우 호출.
    /// </summary>    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LEADER_RED", out int redActor))
        {
            _redLeaderActor = redActor;
            _redLeader = PhotonNetwork.CurrentRoom.GetPlayer(_redLeaderActor);
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LEADER_BLUE", out int blueActor))
        {
            _blueLeaderActor = blueActor;
            _blueLeader = PhotonNetwork.CurrentRoom.GetPlayer(_blueLeaderActor);
        }
    }
}
