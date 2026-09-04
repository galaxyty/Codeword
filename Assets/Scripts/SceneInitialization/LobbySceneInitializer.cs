using UnityEngine;

public class LobbySceneInitializer : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM(Consts.kSOUND_MAIN_BGM);
    }
}
