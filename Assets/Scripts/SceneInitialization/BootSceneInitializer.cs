using Cysharp.Threading.Tasks;
using UnityEngine;

public class BootSceneInitializer : MonoBehaviour, ISceneInitializer
{
    public UniTask InitializationAsync()
    {
        return UniTask.CompletedTask;
    }

    async UniTask Start()
    {
        await SoundManager.Instance.InitialzationAsync();
    }
}
