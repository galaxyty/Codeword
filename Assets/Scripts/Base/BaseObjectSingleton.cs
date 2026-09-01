using Photon.Pun;
using System;
using UnityEngine;

// 하이어라키 창에 생성하는 싱글톤.
public abstract class BaseObjectSingleton<T> : MonoBehaviourPunCallbacks where T : BaseObjectSingleton<T>
{
    private static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 타입.
                Type type = typeof(T);

                // 오브젝트 생성.
                GameObject obj = new GameObject();

                // 오브젝트 이름 재정의.
                obj.name = type.ToString();

                // 해당 싱글톤 컴포넌트 생성.
                _instance = obj.AddComponent<T>();
            }

            return _instance;
        }
    }

    // 싱글톤 초기화.
    public abstract void Initialzation();

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
