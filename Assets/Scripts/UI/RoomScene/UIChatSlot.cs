using UnityEngine;
using TMPro;

public class UIChatSlot : MonoBehaviour
{
    // 닉네임 텍스트.
    [SerializeField]
    private TextMeshProUGUI _txtName;

    // 채팅 내용 텍스트.
    [SerializeField]
    private TextMeshProUGUI _txtDesc;

    public void SetName(string name)
    {
        _txtName.text = name;
    }

    public void SetDesc(string desc)
    {
        _txtDesc.text = desc;
    }
}
