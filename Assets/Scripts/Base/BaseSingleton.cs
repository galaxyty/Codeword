using Cysharp.Threading.Tasks;
using UnityEngine;

// 하이어라키 창에 이미 생성 된 오브젝트에 싱글톤 시킬 스크립트.
public abstract class BaseSingleton<T> : MonoBehaviour where T : BaseSingleton<T>
{
    private static T _instance = null;

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    /// <summary>
    /// 싱글톤 초기화.
    /// </summary>
    public abstract UniTask InitialzationAsync();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }

        DontDestroyOnLoad(this);
    }
}
