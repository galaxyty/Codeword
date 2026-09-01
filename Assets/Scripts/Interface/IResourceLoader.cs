using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IResourceLoader
{
    /// <summary>
    /// 비동기용 로드.
    /// </summary>
    public UniTask<T> LoadAsync<T>(string path) where T : Object;
}
