using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResourceLoader : IResourceLoader
{
    public async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        var result = await Resources.LoadAsync<T>(path);

        return result as T;
    }
}