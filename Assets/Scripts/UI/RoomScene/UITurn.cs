using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UITurn : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TextMeshProUGUI _txtDesc;

    private void UpdateDescUI(string desc)
    {
        _txtDesc.text = desc;
    }

    /// <summary>
    /// 방 정보 갱신됐을 때 콜백.
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        if (newRoomData.TryGetValue("STATE", out EState state) == true)
        {
            switch (state)
            {
                case EState.Wait:
                    UpdateDescUI("게임 준비중");
                    break;

                case EState.RedLeaderTurn:
                    UpdateDescUI("레드 팀 팀장은 하단에 단어를 선택하고 상단에 힌트를 입력하세요");
                    break;

                case EState.BlueLeaderTurn:
                    UpdateDescUI("블루 팀 팀장은 하단에 단어를 선택하고 상단에 힌트를 입력하세요");
                    break;

                case EState.RedMemberTurn:
                    UpdateDescUI("레드 팀 팀원은 상단에 힌트를 추리하여 하단에 단어를 선택하세요");
                    break;

                case EState.BlueMemberTurn:
                    UpdateDescUI("블루 팀 팀원은 상단에 힌트를 추리하여 하단에 단어를 선택하세요");
                    break;

                default:
                    UpdateDescUI("");
                    break;
            }
        }
    }
}