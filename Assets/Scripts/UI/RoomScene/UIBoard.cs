using Photon.Pun;
using UnityEngine;

public class UIBoard : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform _content;

    [SerializeField]
    private GameManager _gameManager;

    [SerializeField]
    private UICardSelectPopup _popup;

    private void Start()
    {
        var obj = Resources.Load<GameObject>("UICardSlot");

        var cardRed = Resources.Load<Sprite>($"Images/Card/card_{(int)ECard.Red}");
        var cardBlue = Resources.Load<Sprite>($"Images/Card/card_{(int)ECard.Blue}");
        var cardNeutual = Resources.Load<Sprite>($"Images/Card/card_{(int)ECard.Neutral}");
        var cardJoker = Resources.Load<Sprite>($"Images/Card/card_{(int)ECard.Joker}");
        var cardBasic = Resources.Load<Sprite>($"Images/Card/card_{(int)ECard.Basic}");

        for (int i = 0; i < 25; i++)
        {
            var newObj = Instantiate(obj, _content);
            UICardSlot slot = newObj.GetComponent<UICardSlot>();
            slot.SetCardSprite(cardRed, cardBlue, cardNeutual, cardJoker, cardBasic);
            slot.SetCardData(_gameManager.GetBoard(i));
            slot.SetGameManager(_gameManager);
            slot.SetPopup(_popup);
            slot.UpdateNameUI("Ä«µå");
            slot.UpdateCardUI();            

            _gameManager.AddUISlot(slot);
        }
    }
}