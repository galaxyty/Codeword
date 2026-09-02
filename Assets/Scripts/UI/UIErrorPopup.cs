using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIErrorPopup : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _txtDesc;

    [SerializeField]
    private Button _btnClose;

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        gameObject.SetActive(true);

        _txtDesc.text = text;
    }
}
