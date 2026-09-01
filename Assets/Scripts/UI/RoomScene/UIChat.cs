using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChat : MonoBehaviourPun
{
    [SerializeField]
    private TMP_InputField _inputField;

    // 채팅 슬롯 생성 위치.
    [SerializeField]
    private RectTransform _chatContent;

    [SerializeField]
    private ScrollRect _scrollRect;

    // 채팅 슬롯 오브젝트.
    private GameObject _chatSlot;

    // 채팅 내용을 보여줄 수 있는 최대 갯수.
    private const int kCHAT_SLOT_MAX_COUNT = 50;

    private WaitForEndOfFrame _endFrame = new();

    void Start()
    {
        _chatSlot = Resources.Load<GameObject>("UIChatSlot");

        _inputField.onSubmit.RemoveAllListeners();
        _inputField.onSubmit.AddListener(OnChatSend);
    }    

    public void OnChatSend(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        photonView.RPC(
            nameof(SendChatRPC),
            RpcTarget.All,
            PhotonNetwork.NickName,
            text);

        _inputField.text = string.Empty;        
    }

    [PunRPC]
    private void SendChatRPC(string nickName, string message)
    {
        // 최대 갯수 넘어가면 상단 채팅 내용 제거.
        if (kCHAT_SLOT_MAX_COUNT <= _chatContent.childCount)
        {
            Destroy(_chatContent.GetChild(0).gameObject);
        }

        var obj = Instantiate(_chatSlot, _chatContent);
        var slot = obj.GetComponent<UIChatSlot>();

        slot.SetName(nickName);
        slot.SetDesc(message);
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_chatContent);

        StartCoroutine(GoScrollBottom());
    }

    // Content Size Filtter가 즉각 사이즈 갱신 안돼서 그런지
    // 프레임 끝나고 스크롤 이동 줄 것.
    private IEnumerator GoScrollBottom()
    {        
        yield return _endFrame;
        _scrollRect.verticalNormalizedPosition = 0f;
    }
}