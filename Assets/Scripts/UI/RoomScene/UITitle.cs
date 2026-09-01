using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITitle : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField _fieldTitle;

    [SerializeField]
    private TextMeshProUGUI _txtCount;

    [SerializeField]
    private Button _btnSend;

    [SerializeField]
    private GameManager _gameManager;

    void Start()
    {
        _fieldTitle.text = string.Empty;
        _txtCount.text = string.Empty;

        _fieldTitle.interactable = false;
        _btnSend.interactable = false;
    }

    public void OnClickSend()
    {
        _gameManager.SendHint(_fieldTitle.text);
    }

    /// <summary>
    /// ·ë Á¤º¸°¡ °»½Å µÆÀ» ¶§ ÄÝ¹é.
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        _fieldTitle.interactable = false;
        _btnSend.interactable = false;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("STATE", out EState state) == true)
        {
            if (state == EState.Wait)
            {
                _fieldTitle.text = string.Empty;
                _txtCount.text = string.Empty;
            }

            if (state == EState.RedLeaderTurn)
            {
                // ·¹µå ÆÀÀå Â÷·Ê.
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LEADER_RED", out int redActorNumber) == true)
                {
                    if (PhotonNetwork.LocalPlayer.ActorNumber == redActorNumber)
                    {
                        _fieldTitle.interactable = true;
                        _fieldTitle.text = string.Empty;
                        _btnSend.interactable = true;
                    }
                }
            }
            else if (state == EState.BlueLeaderTurn)
            {
                // ºí·ç ÆÀÀå Â÷·Ê.
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LEADER_BLUE", out int blueActorNumber) == true)
                {
                    if (PhotonNetwork.LocalPlayer.ActorNumber == blueActorNumber)
                    {
                        _fieldTitle.interactable = true;
                        _fieldTitle.text = string.Empty;
                        _btnSend.interactable = true;
                    }
                }
            }
            else if (state == EState.RedMemberTurn || state == EState.BlueMemberTurn)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("HINT_TITLE", out var title) == true)
                {
                    string hintTitle = (string)title;

                    _fieldTitle.text = hintTitle;
                }

                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("HINT_COUNT", out int hintCount) == true)
                {
                    _txtCount.text = hintCount.ToString();
                }
            }
        }
    }
}