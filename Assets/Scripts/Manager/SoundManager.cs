using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : BaseSingleton<SoundManager>
{
    // 리소스 폴더용 로드.
    private IResourceLoader _resourceLoader;

    // 어드레서블용 로드.
    private IResourceLoader _addressableLoader;

    [SerializeField]
    private AudioSource _bgmAudio;

    [SerializeField]
    private AudioSource _sfxAudio;

    // 사운드 캐싱.
    private Dictionary<string, AudioClip> _dicSounds = new();

    public async override UniTask InitialzationAsync()
    {
        _resourceLoader = new ResourceLoader();

        // 사운드 캐싱.

        // BGM.
        _dicSounds.Add(Consts.kSOUND_MAIN_BGM, await _resourceLoader.LoadAsync<AudioClip>(Consts.kSOUND_MAIN_BGM));

        // SFX.
        _dicSounds.Add(Consts.kSOUND_BUTTON_CLICK, await _resourceLoader.LoadAsync<AudioClip>(Consts.kSOUND_BUTTON_CLICK));
    }

    /// <summary>
    /// BGM 재생.
    /// </summary>
    public void PlayBGM(string path)
    {
        if (_dicSounds[path] == null)
        {
            return;
        }

        _bgmAudio.PlayOneShot(_dicSounds[path]);
    }

    /// <summary>
    /// 효과음 재생.
    /// </summary>
    public void PlaySFX(string path)
    {
        if (_dicSounds[path] == null)
        {
            return;
        }

        _sfxAudio.PlayOneShot(_dicSounds[path]);
    }
}
