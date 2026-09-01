using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoginPopup : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField _txtNickName;

    [SerializeField]
    private Button _btnConnect;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    /// <summary>
    /// 서버 연결 버튼.
    /// </summary>
    public void OnClickConnect()
    {
        SoundManager.Instance.PlaySFX(Consts.kSOUND_BUTTON_CLICK);

        if (string.IsNullOrWhiteSpace(_txtNickName.text) == true)
        {
            Debug.Log("닉네임을 입력해주세요");
            return;
        }

        _btnConnect.interactable = false;

        PhotonNetwork.LocalPlayer.NickName = _txtNickName.text;
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// 서버에 성공적으로 연결 시 완료 콜백.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료");

        PhotonNetwork.LoadLevel("LobbyScene");
    }

    /// <summary>
    /// 서버에 연결 실패 시 콜백.
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"서버 연결 실패 : {cause}");

        _btnConnect.interactable = true;
    }
}