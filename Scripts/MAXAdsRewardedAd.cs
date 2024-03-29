using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        RewardAdObject currentRewardAd;
        public static float TIMEOUT_LOADREWARDAD = 12f;

        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdLoadedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onRewardAdLoadFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdDisplayedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdClickedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdRevenuePaidEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdHiddenEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onRewardAdDisplayFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onRewardAdReceivedRewardEvent;
        public static Action<AdPlacement.Type> onRewardAdRequestedEvent;

        RewardAdObject GetCurrentRewardAd(bool logErrorIfNull = false)
        {
            if (currentRewardAd == null)
            {
                if (logErrorIfNull)
                {
                    Debug.LogError("currentRewardAd is null.");
                }

                Debug.Log("Creating new reward ad object");
                currentRewardAd = new RewardAdObject();
            }
            return currentRewardAd;
        }

        public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            currentRewardAd = GetCurrentRewardAd();
            currentRewardAd.onAdClosed = onFinish;
            // currentRewardAd = new RewardAdObject(placementType, onFinish);
            RequestRewardAd(placementType, loadResult =>
            {
                if (loadResult.type == RewardResult.Type.Loading)
                {
                    onFinish?.Invoke(loadResult);
                }
                else
                {
                    string adUnitId = MAXAdID.GetAdID(placementType);
                    if (MaxSdk.IsRewardedAdReady(adUnitId))
                    {
                        GetCurrentRewardAd().State = AdObjectState.Showing;
                        MaxSdk.ShowRewardedAd(adUnitId);
                    }
                    else
                    {
                        onFinish?.Invoke(loadResult);
                    }
                }
            });
            // StartCoroutine(CoReward(placementType, onFinish));
        }

        IEnumerator CoReward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            float _timeoutRequestAds = TIMEOUT_LOADREWARDAD;

            string adUnitId = MAXAdID.GetAdID(placementType);
            MaxSdk.LoadRewardedAd(adUnitId);
            GetCurrentRewardAd().State = AdObjectState.Loading;
            onRewardAdRequestedEvent?.Invoke(placementType);

            float retryInterval = 0.4f;
            WaitForSecondsRealtime delay = new WaitForSecondsRealtime(retryInterval);
            int tryTimes = 0;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _timeoutRequestAds = 3f;
            }
            while (!MaxSdk.IsRewardedAdReady(adUnitId) && tryTimes < _timeoutRequestAds / retryInterval)
            {
                yield return delay;
                tryTimes++;
            }
            //.Log("reward ad available:" + (GetCurrentRewardAd().State == AdObjectState.Loading));

            if (MaxSdk.IsRewardedAdReady(adUnitId))
            {
                GetCurrentRewardAd().State = AdObjectState.Showing;
                MaxSdk.ShowRewardedAd(adUnitId);
            }
            else
            {
                onFinish?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "Self timeout"));
            }
            //if (showLoading)
            //    Manager.LoadingAnimation(false);
        }

        public void RequestRewardAd(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            var currentAd = GetCurrentRewardAd();
            currentAd.AdPlacementType = placementType;
            if (currentAd.CanShow)
            {
                onFinish.Invoke(new RewardResult(RewardResult.Type.Loaded));
                return;
            }

            if (currentAd.State == AdObjectState.Loading)
            {
                Debug.Log($"Reward ad {currentAd.AdPlacementType} is still loading.");
                onFinish.Invoke(new RewardResult(RewardResult.Type.Loading));
                return;
            }

            StartCoroutine(CoRequestReward(placementType, onFinish));
        }
        
        IEnumerator CoRequestReward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            float _timeoutRequestAds = TIMEOUT_LOADREWARDAD;
            string adUnitId = MAXAdID.GetAdID(placementType);
            MaxSdk.LoadRewardedAd(adUnitId);
            GetCurrentRewardAd().State = AdObjectState.Loading;
            GetCurrentRewardAd().onAdLoaded = OnRewardAdLoaded;
            onRewardAdRequestedEvent?.Invoke(placementType);

            float retryInterval = 0.4f;
            WaitForSecondsRealtime delay = new WaitForSecondsRealtime(retryInterval);
            int tryTimes = 0;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _timeoutRequestAds = 3f;
            }
            while (!MaxSdk.IsRewardedAdReady(adUnitId) && tryTimes < _timeoutRequestAds / retryInterval)
            {
                yield return delay;
                tryTimes++;
            }
            //.Log("reward ad available:" + (GetCurrentRewardAd().State == AdObjectState.Loading));

            void OnRewardAdLoaded(RewardResult rewardResult)
            {
                if (MaxSdk.IsRewardedAdReady(adUnitId))
                {
                    // GetCurrentRewardAd().State = AdObjectState.Ready;
                    onFinish?.Invoke(new RewardResult(RewardResult.Type.Loaded));
                }
                else
                {
                    onFinish?.Invoke(rewardResult);
                }
            }
            
            if (GetCurrentRewardAd().State == AdObjectState.Loading)
            {
                onFinish?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "Self timeout"));
                onFinish = null;
            }
        }

        public void InitializeRewardedAds()
        {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentRewardAd(logErrorIfNull: true).State = AdObjectState.Ready;
                GetCurrentRewardAd().onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.Loaded));
                onRewardAdLoadedEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, adInfo);
            });
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentRewardAd(logErrorIfNull: true).State = AdObjectState.LoadFailed;
                GetCurrentRewardAd().onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, errorInfo.Message));
                onRewardAdLoadFailedEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, errorInfo);
            });
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                onRewardAdDisplayFailedEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, errorInfo);
            });
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                onRewardAdClickedEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, adInfo);
            });
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                RewardResult.Type rewardResultType = RewardResult.Type.Canceled;
                if (GetCurrentRewardAd(logErrorIfNull: true).State == AdObjectState.Shown)
                {
                    rewardResultType = RewardResult.Type.Finished;
                }
                GetCurrentRewardAd().onAdClosed?.Invoke(new RewardResult(rewardResultType));
                GetCurrentRewardAd().State = AdObjectState.Closed;
                onRewardAdHiddenEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, adInfo);
            });
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                //GetCurrentRewardAd().onAdClosed?.Invoke(new RewardResult(RewardResult.Type.Finished));
                GetCurrentRewardAd(logErrorIfNull: true).State = AdObjectState.Shown;
                onRewardAdReceivedRewardEvent?.Invoke(GetCurrentRewardAd().AdPlacementType, adInfo);
            });
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                // Ad revenue paid. Use this callback to track user revenue.
                onRewardAdRevenuePaidEvent?.Invoke(GetCurrentRewardAd(logErrorIfNull: true).AdPlacementType, adInfo);
            });
        }
    }
}