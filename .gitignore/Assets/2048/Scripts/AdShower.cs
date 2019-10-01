using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdShower : MonoBehaviour
{

    private const string GameId = "3279937";

    private const string BannerID = "Banner";

    public bool testMode = true;

    private void Start()
    {
        Advertisement.Initialize(GameId, testMode);
        StartCoroutine(ShowBannerWhenReady());
    }

    private IEnumerator ShowBannerWhenReady()
    {
        while (!Advertisement.IsReady(BannerID))
        {
            yield return new WaitForSeconds(0.5f);
        }

        Advertisement.Banner.Show(BannerID);
    }

}