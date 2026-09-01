using UnityEngine;
using TMPro;
using Photon.Pun;

public class UICardSelectPopup : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _txtDesc;

    [SerializeField]
    private GameManager _gameManager;

    private Card _card;

    public void Show(Card card)
    {
        _card = card;

        gameObject.SetActive(true);
        _txtDesc.text = $"<b>{_card.Name}</b> 단어를 선택하시겠습니까?";        
    }

    public void OnClickOK()
    {
        _gameManager.photonView.RPC(
            "ResultCardRPC",
            RpcTarget.MasterClient,
            _card.Index,
            _gameManager.UserRoomIndex);

        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}
