using UnityEngine;
using UnityEngine.UI;

public class CanvasResolution : MonoBehaviour
{
    [SerializeField]
    private CanvasScaler[] _scalers;

    void Awake()
    {
        // UI 해상도 스케일 대응.
        float ratio = (float)Screen.height / Screen.width;
        if (ratio < 0.5f)
        {
            foreach (var canvasScaler in _scalers)
            {
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
            }            
        }
        else
        {
            foreach (var canvasScaler in _scalers)
            {
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
        }
    }
}
