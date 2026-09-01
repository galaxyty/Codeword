using Photon.Pun;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using TMPro;

public class UIResultPopup : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TextMeshProUGUI _txtWinner;

    [SerializeField]
    private TextMeshProUGUI _txtDesc;

    [SerializeField]
    private TextMeshProUGUI _txtRedScore;

    [SerializeField]
    private TextMeshProUGUI _txtBlueScore;

    [SerializeField]
    private GameObject _objBG;

    [SerializeField]
    private GameObject _objPopup;

    [SerializeField]
    private GameManager _gameManager;

    private const string kRED = "∑πµÂ∆¿";
    private const string kBLUE = "∫Ì∑Á∆¿";
    private const string kWIN = "Ω¬∏Æ!";
    private const string kLOSE = "∆–πË...";
    private const string kDESC = "∆¿¿Â¿« ≈¿Â¿∏∑Œ ∞‘¿”¿ª ¡æ∑·«’¥œ¥Ÿ";

    private void UpdateWinnerUI(EState state)
    {
        string result = "";

        var team = SlotUtility.GetIndexTeam(_gameManager.UserRoomIndex, out _);

        // ∑πµÂ∆¿ Ω¬∏Æ/∆–πË ¥‹æÓ ∞·¡§.
        if (team == ETeam.Red && (state == EState.BlueLose || state == EState.BlueLeaderOut))
        {
            // ∑πµÂ∆¿ Ω¬∏Æ.
            result = $"{kRED} {kWIN}";
        }
        else if (team == ETeam.Red && (state == EState.RedLose || state == EState.RedLeaderOut))
        {
            // ∑πµÂ∆¿ ∆–πË.
            result = $"{kRED} {kLOSE}";
        }
        else if (team == ETeam.Blue && (state == EState.RedLose || state == EState.RedLeaderOut))
        {
            // ∫Ì∑Á∆¿ Ω¬∏Æ.
            result = $"{kBLUE} {kWIN}";
        }
        else
        {
            // ∫Ì∑Á∆¿ ∆–πË.
            result = $"{kBLUE} {kLOSE}";
        }

        _txtWinner.text = result;
    }

    private void UpdateScoreUI()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("REMAIN_COUNT_ARRAY", out var remainCountArray) == false)
        {            
            return;
        }

        int[] remainCountArr = (int[])remainCountArray;

        _txtRedScore.text = remainCountArr[(int)ECard.Red].ToString();
        _txtBlueScore.text = remainCountArr[(int)ECard.Blue].ToString();
    }

    private void UpdateDescUI(EState state)
    {
        string result = "";

        if (state == EState.RedLeaderOut)
        {
            result = $"{kRED} {kDESC}";
        }
        else
        {
            result = $"{kBLUE} {kDESC}";
        }

        _txtDesc.gameObject.SetActive(true);
        _txtDesc.text = result;
    }

    private System.Collections.IEnumerator CoResultShow(EState state)
    {
        yield return new WaitForSeconds(1.5f);

        _objBG.SetActive(true);
        _objPopup.SetActive(true);

        UpdateWinnerUI(state);
        UpdateScoreUI();
    }

    public override void OnRoomPropertiesUpdate(Hashtable newRoomData)
    {
        if (newRoomData.TryGetValue("STATE", out EState state))
        {
            if (state == EState.Wait)
            {
                _txtDesc.gameObject.SetActive(false);
                _objBG.SetActive(false);
                _objPopup.SetActive(false);
            }

            if (state == EState.RedLose || state == EState.BlueLose)
            {
                StartCoroutine(CoResultShow(state));
            }

            if (state == EState.RedLeaderOut || state == EState.BlueLeaderOut)
            {
                UpdateWinnerUI(state);
                UpdateDescUI(state);
                UpdateScoreUI();

                _objBG.SetActive(true);
                _objPopup.SetActive(true);
            }
        }
    }
}