using Photon.Pun;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIRemainCardSlot : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private ECard _cardType;

    [SerializeField]
    private TextMeshProUGUI _txtCount;

    [SerializeField]
    private Image _imgColor;

    private void Start()
    {
        UpdateCardUI(_cardType);
        UpdateCountUI(0);
    }

    private void UpdateCardUI(ECard type)
    {
        _imgColor.color = _cardType switch
        {
            ECard.Red => Color.red,
            ECard.Blue => Color.blue,
            ECard.Neutral => new Color(0.95f, 0.95f, 0.85f),
            ECard.Joker => Color.gray,
            _ => Color.magenta
        };
    }

    public void UpdateCountUI(int count)
    {
        _txtCount.text = $"{count}칸";
    }

    /// <summary>
    /// 방 정보 갱신됐을 때 콜백.
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        if (newRoomData.TryGetValue("REMAIN_COUNT_ARRAY", out var remainCountArray) == true)
        {
            int[] remainCountArr = (int[])remainCountArray;

            UpdateCountUI(remainCountArr[(int)_cardType]);
        }

        if (newRoomData.TryGetValue("STATE", out EState state) == true)
        {
            switch (state)
            {
                case EState.Wait:
                    UpdateCountUI(0);
                    break;

                default:
                    break;
            }
        }
    }
}