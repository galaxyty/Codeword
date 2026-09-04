using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using LitMotion;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICardSlot : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TextMeshProUGUI _txtName;

    [SerializeField]
    private Image _imgColor;

    [SerializeField]
    private Outline _outLine;

    [SerializeField]
    private GameObject _imgOpen;

    // 카드 데이터.
    private Card _card;

    private GameManager _gameManager;   

    private UICardSelectPopup _popup;

    // 카드 스프라이트들.
    private Sprite _redCard;
    private Sprite _blueCard;
    private Sprite _neutralCard;
    private Sprite _jokerCard;
    private Sprite _basicCard;

    public void SetCardSprite(Sprite red, Sprite blue, Sprite neutral, Sprite joker, Sprite basic)
    {
        _redCard = red;
        _blueCard = blue;
        _neutralCard = neutral;
        _jokerCard = joker;
        _basicCard = basic;
    }

    public void SetCardData(Card card)
    {
        _card = card;
    }

    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void SetPopup(UICardSelectPopup popup)
    {
        _popup = popup;
    }

    /// <summary>
    /// 이름 UI 갱신.
    /// </summary>
    public void UpdateNameUI()
    {
        _txtName.text = _card.Name;
    }

    /// <summary>
    /// 이름 UI 갱신.
    /// </summary>
    public void UpdateNameUI(string name)
    {
        _txtName.text = name;
    }

    /// <summary>
    /// 타입 갱신.
    /// </summary>
    public void UpdateCardUI()
    {
        if (_card.IsOpen == true)
        {
            return;
        }

        if (_gameManager.IsLeader == true)
        {
            UpdateLeaderColorUI();
        }
        else
        {
            UpdateMemberColorUI();
        }        
    }    

    /// <summary>
    /// 카드 오픈 UI.
    /// </summary>
    public void UpdateOpenUI()
    {
        Sprite target = _card.Type switch
        {
            ECard.Red => _redCard,
            ECard.Blue => _blueCard,
            ECard.Neutral => _neutralCard,
            ECard.Joker => _jokerCard,
            ECard.Basic => _basicCard,
            _ => _basicCard
        };

        _imgOpen.SetActive(true);
        PlayImage(target);
        PlayScale();
    }

    private void UpdateOutLine()
    {
        _outLine.enabled = _card.IsToggle;
    }

    // 팀장은 카드색 볼 수 있게 함.
    private void UpdateLeaderColorUI()
    {
        _imgColor.sprite = _card.Type switch
        {
            ECard.Red => _redCard,
            ECard.Blue => _blueCard,
            ECard.Neutral => _neutralCard,
            ECard.Joker => _jokerCard,
            ECard.Basic => _basicCard,
            _ => _basicCard
        };

        UpdateTextColorUI();
    }

    // 팀원은 하나의 색깔만 표시.
    private void UpdateMemberColorUI()
    {
        _imgColor.sprite = _neutralCard;
        _txtName.color = new Color(0, 0, 0);
    }

    // 글자 색 변경.
    private void UpdateTextColorUI()
    {
        _txtName.color = _card.Type switch
        {
            ECard.Neutral => new Color(0, 0, 0),
            ECard.Basic => new Color(0, 0, 0),
            _ => new Color(1, 1, 1)
        };
    }

    // 일정 시간 뒤 이미지 변경.
    private void PlayImage(Sprite targetSprite)
    {
        LMotion.Create(0f, 0f, 0.5f).WithOnComplete(() => _imgColor.sprite = targetSprite).RunWithoutBinding();
        UpdateTextColorUI();
    }

    // 카드 크기 애니메이션.
    private void PlayScale()
    {       
        Vector3 originalScale = transform.localScale;
        Vector3 smallScale = originalScale * 0.6f;
        Vector3 bigScale = originalScale * 1.5f;

        var _scaleMotion = LSequence.Create()
            .Append(
                LMotion.Create(originalScale, smallScale, 0.3f)
                    .WithEase(Ease.OutQuad)
                    .Bind(x => transform.localScale = x)
            )
            .Append(
                LMotion.Create(smallScale, bigScale, 0.6f)
                    .WithEase(Ease.OutBack)
                    .Bind(x => transform.localScale = x)
            )
            .Append(
                LMotion.Create(bigScale, originalScale, 0.1f)
                    .WithEase(Ease.OutQuad)
                    .Bind(x => transform.localScale = x)
            )
            .Run();
    }

    /// <summary>
    /// 카드 터치 이벤트.
    /// </summary>
    public void OnClickCard()
    {
        if (_card == null)
        {
            return;
        }

        if (_card.IsOpen == true)
        {
            return;
        }

        // 리더인지 확인.
        if (_gameManager.IsLeader == true)
        {
            // 자기팀 카드만 선택 가능.
            if (_gameManager.IsCardTeam(_card) == false) return;

            // 리더 차례이며 리더는 자기 팀 카드만 선택 가능.
            if (_gameManager.IsLeaderTurn() == false) return;

            _card.IsToggle = !_card.IsToggle;
            _gameManager.ToggleCardHint(_card.IsToggle);

            UpdateOutLine();
        }
        else
        {
            // 팀원 차례.
            if (_gameManager.IsMemberTurn() == false) return;

            _popup.Show(_card);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        if (_card != null)
        {
            _card.IsToggle = false;
            UpdateOutLine();
        }

        if (newRoomData.TryGetValue("STATE", out EState state) == true)
        {
            if (state == EState.RedLeaderTurn || state == EState.BlueLeaderTurn)
            {
                UpdateNameUI();
                UpdateCardUI();
            }

            if (state == EState.Wait)
            {
                if (_card == null)
                {
                    return;
                }

                _card.IsOpen = false;
                _card.Type = ECard.Basic;

                UpdateNameUI("카드");
                UpdateMemberColorUI();
                _imgOpen.SetActive(false);
            }
        }
    }
}