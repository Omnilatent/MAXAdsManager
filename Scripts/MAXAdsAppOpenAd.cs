using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        //App Open Ad event
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onAOAdLoadedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onAOAdClickedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onAOAdDisplayEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onAOAdDisplayFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onAOAdHiddenEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onAOAdLoadFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onAOAdRevenuePaidEvent;

        public static float TIMEOUT_LOADING_APPOPENAD = 3;

        AdPlacement.Type currentAppOpenAdPlacement;
        AppOpenAdObject appOpenAdObject;


        public void RequestAppOpenAd(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            //currentAppOpenAdPlacement = placementType;
            //if (appOpenAdObject != null && appOpenAdObject.State == AdObjectState.Ready)
            //{
            //    onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.Finished));
            //    return;
            //}
            //appOpenAdObject = new AppOpenAdObject(placementType, onAdLoaded);
            //appOpenAdObject.State = AdObjectState.Loading;
            //string adUnitId = MAXAdID.GetAdID(placementType);
            //MaxSdk.LoadAppOpenAd(adUnitId);
            StartCoroutine(Co_RequestAppOpenAd(placementType, onAdLoaded));
        }

        private IEnumerator Co_RequestAppOpenAd(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            string adUnitId = MAXAdID.GetAdID(placementType);
            if (MaxSdk.IsAppOpenAdReady(adUnitId))
            {
                onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.Finished));
                yield break;
            }

            currentAppOpenAdPlacement = placementType;

            appOpenAdObject = new AppOpenAdObject(placementType, onAdLoaded);
            appOpenAdObject.State = AdObjectState.Loading;

            MaxSdk.LoadAppOpenAd(adUnitId);

            float timer = 0;
            while(timer < TIMEOUT_LOADING_APPOPENAD && MaxSdk.IsAppOpenAdReady(adUnitId))
            {
                yield return null;
                timer += Time.deltaTime;
            }
            onAdLoaded?.Invoke(MaxSdk.IsAppOpenAdReady(adUnitId) ? new RewardResult(RewardResult.Type.Finished) : new RewardResult(RewardResult.Type.Loading));
        }

        public void ShowAppOpenAd(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed = null)
        {
            string adUnitId = MAXAdID.GetAdID(placementType);
            if (appOpenAdObject != null && MaxSdk.IsAppOpenAdReady(adUnitId))
            {
                appOpenAdObject.onAdClosed = onAdClosed;
                MaxSdk.ShowAppOpenAd(adUnitId);
                appOpenAdObject.State = AdObjectState.Showing;
            }
            else
                onAdClosed?.Invoke();
        }

        public void InitializeAOAds()
        {
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += AppOpen_OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += AppOpen_OnAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += AppOpen_OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += AppOpen_OnAdHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += AppOpen_OnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += AppOpen_OnAdRevenuePaidEvent;
        }

        private void AppOpen_OnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            QueueMainThreadExecution(() =>
            {
                onAOAdRevenuePaidEvent?.Invoke(currentAppOpenAdPlacement, arg2);
            });
        }

        private void AppOpen_OnAdLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo error)
        {
            QueueMainThreadExecution(() =>
            {
                appOpenAdObject.State = AdObjectState.LoadFailed;
                appOpenAdObject.onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed));
                onAOAdLoadFailedEvent?.Invoke(currentAppOpenAdPlacement, error);
            });
        }

        private void AppOpen_OnAdHiddenEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            QueueMainThreadExecution(() =>
            {
                appOpenAdObject.State = AdObjectState.Closed;
                appOpenAdObject.onAdClosed(true);
                onAOAdHiddenEvent?.Invoke(currentAppOpenAdPlacement, arg2);
            });
        }

        private void AppOpen_OnAdDisplayFailedEvent(string arg1, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo arg3)
        {
            QueueMainThreadExecution(() =>
            {
                appOpenAdObject.State = AdObjectState.ShowFailed;
                onAOAdDisplayFailedEvent?.Invoke(currentAppOpenAdPlacement, error);
            });
        }

        private void AppOpen_OnAdDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            QueueMainThreadExecution(() =>
            {
                appOpenAdObject.State = AdObjectState.Shown;
                onAOAdDisplayEvent?.Invoke(currentAppOpenAdPlacement, arg2);
            });
        }

        private void AppOpen_OnAdClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            QueueMainThreadExecution(() =>
            {
                onAOAdClickedEvent?.Invoke(currentAppOpenAdPlacement, arg2);
            });
        }

        private void AppOpen_OnAdLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            QueueMainThreadExecution(() =>
            {
                appOpenAdObject.State = AdObjectState.Ready;
                appOpenAdObject.onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.Finished));
                onAOAdLoadedEvent?.Invoke(currentAppOpenAdPlacement, arg2);
            });
        }
    }
}
