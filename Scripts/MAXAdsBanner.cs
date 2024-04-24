using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public class MaxBannerTransform : BannerTransform
    {
        public bool Adaptive;

        public MaxBannerTransform(AdPosition adPosition, bool adaptive)
        {
            this.adPosition = adPosition;
            Adaptive = adaptive;
        }
    }

    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdLoadedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onBannerAdLoadFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdClickedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdRevenuePaidEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdExpandedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onBannerAdCollapsedEvent;
        public static Action<AdPlacement.Type> onBannerAdRequestedEvent;

        BannerAdObject currentBannerAd;

        BannerAdObject GetCurrentBannerAd(bool makeNewIfNull = true)
        {
            if (currentBannerAd == null)
            {
                Debug.LogError("currentBannerAd is null.");
                if (makeNewIfNull)
                {
                    Debug.Log("New ad will be created");
                    currentBannerAd = new BannerAdObject();
                }
            }
            return currentBannerAd;
        }

        public void RequestBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, ref BannerAdObject bannerAdObject,
            BannerLoadDelegate onAdLoaded = null)
        {
            if (currentBannerAd != null && currentBannerAd.AdPlacementType == placementType && currentBannerAd.State != AdObjectState.LoadFailed)
            {
                if (currentBannerAd.State == AdObjectState.Closed)
                {
                    onAdLoaded?.Invoke(true, currentBannerAd);
                    MaxSdk.ShowBanner(MAXAdID.GetAdID(currentBannerAd.AdPlacementType));
                }
            }
            else
            {
                MaxSdkBase.BannerPosition bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
                switch (bannerTransform.adPosition)
                {
                    case AdPosition.Top:
                        bannerPosition = MaxSdkBase.BannerPosition.TopCenter;
                        break;
                    case AdPosition.TopLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.TopLeft;
                        break;
                    case AdPosition.TopRight:
                        bannerPosition = MaxSdkBase.BannerPosition.TopRight;
                        break;
                    case AdPosition.BottomLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomLeft;
                        break;
                    case AdPosition.BottomRight:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomRight;
                        break;
                    case AdPosition.Center:
                        bannerPosition = MaxSdkBase.BannerPosition.Centered;
                        break;
                }
                currentBannerAd = new BannerAdObject(placementType, (success, adObject) => { onAdLoaded?.Invoke(success, currentBannerAd); });
                currentBannerAd.State = AdObjectState.Loading;
                string bannerAdUnitId = MAXAdID.GetAdID(placementType);

                // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
                // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
                MaxSdk.CreateBanner(bannerAdUnitId, bannerPosition);

                // Set background or background color for banners to be fully functional
                MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.black);
            }
        }

        public void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            ShowBanner(placementType, new MaxBannerTransform(AdPosition.Bottom, true), onAdLoaded);
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            if (currentBannerAd != null && currentBannerAd.AdPlacementType == placementType && currentBannerAd.State != AdObjectState.LoadFailed)
            {
                if (currentBannerAd.State == AdObjectState.Closed)
                {
                    onAdLoaded?.Invoke(true);
                    MaxSdk.ShowBanner(MAXAdID.GetAdID(currentBannerAd.AdPlacementType));
                }
            }
            else
            {
                MaxSdkBase.BannerPosition bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
                switch (bannerTransform.adPosition)
                {
                    case AdPosition.Top:
                        bannerPosition = MaxSdkBase.BannerPosition.TopCenter;
                        break;
                    case AdPosition.TopLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.TopLeft;
                        break;
                    case AdPosition.TopRight:
                        bannerPosition = MaxSdkBase.BannerPosition.TopRight;
                        break;
                    case AdPosition.BottomLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomLeft;
                        break;
                    case AdPosition.BottomRight:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomRight;
                        break;
                    case AdPosition.Center:
                        bannerPosition = MaxSdkBase.BannerPosition.Centered;
                        break;
                }
                currentBannerAd = new BannerAdObject(placementType, (success, adObject) => { onAdLoaded?.Invoke(success); });
                currentBannerAd.State = AdObjectState.Loading;
                string bannerAdUnitId = MAXAdID.GetAdID(placementType);

                // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
                // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
                MaxSdk.CreateBanner(bannerAdUnitId, bannerPosition);

                // Set background or background color for banners to be fully functional
                MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.black);
            }
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, ref BannerAdObject bannerAdObject, BannerLoadDelegate onAdLoaded = null)
        {
            if (currentBannerAd != null && currentBannerAd.AdPlacementType == placementType && currentBannerAd.State != AdObjectState.LoadFailed)
            {
                if (currentBannerAd.State == AdObjectState.Closed)
                {
                    bannerAdObject = currentBannerAd;
                    onAdLoaded?.Invoke(true, currentBannerAd);
                    MaxSdk.ShowBanner(MAXAdID.GetAdID(currentBannerAd.AdPlacementType));
                }
            }
            else
            {
                MaxSdkBase.BannerPosition bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
                switch (bannerTransform.adPosition)
                {
                    case AdPosition.Top:
                        bannerPosition = MaxSdkBase.BannerPosition.TopCenter;
                        break;
                    case AdPosition.TopLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.TopLeft;
                        break;
                    case AdPosition.TopRight:
                        bannerPosition = MaxSdkBase.BannerPosition.TopRight;
                        break;
                    case AdPosition.BottomLeft:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomLeft;
                        break;
                    case AdPosition.BottomRight:
                        bannerPosition = MaxSdkBase.BannerPosition.BottomRight;
                        break;
                    case AdPosition.Center:
                        bannerPosition = MaxSdkBase.BannerPosition.Centered;
                        break;
                }
                currentBannerAd = new BannerAdObject(placementType, onAdLoaded);
                bannerAdObject = currentBannerAd;
                currentBannerAd.State = AdObjectState.Loading;
                string bannerAdUnitId = MAXAdID.GetAdID(placementType);

                // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
                // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
                MaxSdk.CreateBanner(bannerAdUnitId, bannerPosition);

                var maxBannerInfo = bannerTransform as MaxBannerTransform;
                if (maxBannerInfo != null && maxBannerInfo.Adaptive)
                    MaxSdk.SetBannerExtraParameter(bannerAdUnitId, "adaptive_banner", "true");

                // Set background or background color for banners to be fully functional
                MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.black);
                onBannerAdRequestedEvent?.Invoke(placementType);
            }
        }

        public void HideBanner()
        {
            if (currentBannerAd != null)
            {
                MaxSdk.HideBanner(MAXAdID.GetAdID(GetCurrentBannerAd().AdPlacementType));
                GetCurrentBannerAd().State = AdObjectState.Closed;
            }
        }

        public void HideBanner(AdPlacement.Type placementType) { HideBanner(); }

        public void DestroyBanner()
        {
            if (currentBannerAd != null)
            {
                MaxSdk.DestroyBanner(MAXAdID.GetAdID(GetCurrentBannerAd().AdPlacementType));
                currentBannerAd = null;
            }
        }
        
        public void DestroyBanner(AdPlacement.Type placementType) { DestroyBanner(); }
        
        public void InitializeBannerAds()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                if (GetCurrentBannerAd().State != AdObjectState.Closed)
                {
                    GetCurrentBannerAd().State = AdObjectState.Showing;
                    GetCurrentBannerAd().onAdLoaded?.Invoke(true, GetCurrentBannerAd());
                    MaxSdk.ShowBanner(MAXAdID.GetAdID(GetCurrentBannerAd().AdPlacementType));
                }
                onBannerAdLoadedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, adInfo);
            });
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            QueueMainThreadExecution(() =>
            {
                if (GetCurrentBannerAd().State != AdObjectState.LoadFailed) //this check is required because AppLovin is calling this method too many times
                {
                    GetCurrentBannerAd().State = AdObjectState.LoadFailed;
                    GetCurrentBannerAd().onAdLoaded?.Invoke(false, GetCurrentBannerAd());
                    onBannerAdLoadFailedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, errorInfo);
                }
            });
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onBannerAdClickedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, adInfo);
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onBannerAdRevenuePaidEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, adInfo);
        }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onBannerAdExpandedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, adInfo);
        }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            onBannerAdCollapsedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, adInfo);
        }
    }
}