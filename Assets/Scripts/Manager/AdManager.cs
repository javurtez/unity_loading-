using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AdManager : MonoManager<AdManager>
{
    public AdScriptable adScriptable;

    public bool isShowingAds = false;

    private BannerView bannerView;
    private InterstitialAd interstitial;

    protected override void Start()
    {
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            MaxAdContentRating = MaxAdContentRating.G
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.Initialize(initStatus =>
        {
            OnBannerShow();
            RequestInterstitial();
        });
    }

    private void RequestBanner()
    {
        if (adScriptable.appBannerId == "") return;
        OnBannerClose();

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adScriptable.appBannerId, AdSize.Banner, adScriptable.bannerPosition);

        // Called when the user returned from the app after an ad click.
        bannerView.OnAdFullScreenContentClosed += HandleOnBannerClosed;

        // Create an empty ad request.
        var adRequest = new AdRequest();

        // Load the banner with the request.
        bannerView.LoadAd(adRequest);
    }

    private void RequestInterstitial()
    {
        if (adScriptable.appInterstitialId == "") return;
        OnInterstitialClose();

        //// create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(adScriptable.appInterstitialId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                interstitial = ad;

                interstitial.OnAdFullScreenContentClosed += HandleOnInterstitialAdClosed;
                interstitial.OnAdFullScreenContentOpened += HandleOnInterstitialAdOpen;
            });
    }

    public void OnBannerShow()
    {
        RequestBanner();
    }
    public void OnBannerClose()
    {
        if (bannerView != null)
            bannerView.Destroy();
    }

    public void OnInterstitialShow()
    {
        if (interstitial == null) return;
        if (interstitial.CanShowAd())
        {
            interstitial.Show();
        }
    }
    public void OnInterstitialClose()
    {
        if (interstitial != null)
            interstitial.Destroy();
    }

    public void HandleOnBannerClosed()
    {
        //this.RequestBanner();
    }

    private void HandleOnInterstitialAdOpen()
    {
        isShowingAds = true;
    }
    public void HandleOnInterstitialAdClosed()
    {
        this.RequestInterstitial();
        isShowingAds = false;
    }

    private void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        isShowingAds = true;
    }
    private void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
    }
    private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        isShowingAds = false;
    }
}