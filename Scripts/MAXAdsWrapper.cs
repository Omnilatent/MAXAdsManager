using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.MAXWrapper
{
    public partial class MAXAdsWrapper : MonoBehaviour, IAdsNetworkHelper
    {
        [SerializeField] bool enableVerboseLogging = false;
        bool initialized = false;
        InterstitialAdObject currentInterstitialAd;

        /// <summary>
        /// Self timeout interstitial ad after this duration
        /// </summary>
        public static float TIMEOUT_LOADINTERAD_SEC = 10f;

        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onInterAdLoadedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onInterAdLoadFailedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onInterAdDisplayedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onInterAdClickedEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.AdInfo> onInterAdHiddenEvent;
        public static Action<AdPlacement.Type, MaxSdkBase.ErrorInfo> onInterAdDisplayFailedEvent;
        public static Action<AdPlacement.Type, string> onInterAdSelfTimeoutEvent; //when ad load timeout by custom duration

        static MAXAdsWrapper instance;
        private Coroutine coTimeoutInterstitial;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Initialize();
            InitializeInterstitialAdsCallbacks();
            InitializeBannerAds();
            InitializeRewardedAds();
        }

        public void Initialize()
        {
            if (initialized) return;
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                // AppLovin SDK is initialized, start loading ads
            };

            MaxSdk.SetSdkKey(MAXAdID.SdkKey);
            MaxSdk.SetVerboseLogging(enableVerboseLogging);
            //MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
            initialized = true;
        }

        InterstitialAdObject GetCurrentInterAd(bool createIfNull = true)
        {
            if (currentInterstitialAd == null)
            {
                Debug.LogError("currentInterstitialAd is null");
                if (createIfNull)
                {
                    Debug.Log("Creating new inter ad object");
                    currentInterstitialAd = new InterstitialAdObject();
                }
            }
            return currentInterstitialAd;
        }

        public void RequestInterstitialNoShow(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true)
        {
            if (currentInterstitialAd != null && currentInterstitialAd.CanShow)
            {
                onAdLoaded?.Invoke(true);
                return;
            }
            currentInterstitialAd = new InterstitialAdObject(placementType, onAdLoaded);
            currentInterstitialAd.State = AdObjectState.Loading;
            string adUnitId = MAXAdID.GetAdID(placementType);
            MaxSdk.LoadInterstitial(adUnitId);
            if (showLoading)
            {
                coTimeoutInterstitial = StartCoroutine(CoTimeoutLoadInterstitial(currentInterstitialAd));
            }
        }

        public void ShowInterstitial(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed)
        {
            string adUnitId = MAXAdID.GetAdID(placementType);
            if (currentInterstitialAd != null && currentInterstitialAd.CanShow && MaxSdk.IsInterstitialReady(adUnitId))
            {
                currentInterstitialAd.onAdClosed = onAdClosed;
                MaxSdk.ShowInterstitial(adUnitId);
                currentInterstitialAd.State = AdObjectState.Showing;
                return;
            }
            onAdClosed?.Invoke(false);
        }

        IEnumerator CoTimeoutLoadInterstitial(InterstitialAdObject interstitialAdObject)
        {
            var delay = new WaitForSeconds(TIMEOUT_LOADINTERAD_SEC);
            yield return delay;
            if (interstitialAdObject.State == AdObjectState.Loading)
            {
                interstitialAdObject.State = AdObjectState.LoadFailed;
                interstitialAdObject.onAdLoaded?.Invoke(false);
                interstitialAdObject.onAdLoaded = null;
                onInterAdSelfTimeoutEvent?.Invoke(interstitialAdObject.AdPlacementType, "Self Timeout");
            }
        }

        public static void ShowMediationDebugger()
        {
            MaxSdk.ShowMediationDebugger();
        }

        public void InitializeInterstitialAdsCallbacks()
        {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.Ready;
                GetCurrentInterAd().onAdLoaded?.Invoke(true);
                onInterAdLoadedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, adInfo);
                //.Log($"Iron source ad ready {GetCurrentInterAd().adPlacementType}");
            });
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.LoadFailed;
                GetCurrentInterAd().onAdLoaded?.Invoke(false);
                onInterAdLoadFailedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, error);

                if (coTimeoutInterstitial != null)
                {
                    StopCoroutine(coTimeoutInterstitial);
                    coTimeoutInterstitial = null;
                }
            });
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.Shown;
                onInterAdDisplayedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, adInfo);
            });
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.ShowFailed;
                onInterAdDisplayFailedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, error);
            });
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                onInterAdClickedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, adInfo);
            });
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.Closed;
                GetCurrentInterAd().onAdClosed?.Invoke(true);
                onInterAdHiddenEvent?.Invoke(GetCurrentInterAd().AdPlacementType, adInfo);
            });
        }

        public void RequestAppOpenAd(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed));
        }

        public void RequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed));
        }

        public void ShowAppOpenAd(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed = null)
        {
            onAdClosed?.Invoke(false);
        }
        public void ShowInterstitialRewarded(AdPlacement.Type placementType, RewardDelegate onAdClosed)
        {
            onAdClosed?.Invoke(new RewardResult(RewardResult.Type.LoadFailed));
        }

        public static void QueueMainThreadExecution(Action action)
        {
#if UNITY_ANDROID
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                action.Invoke();
            });
#else
        action.Invoke();
#endif
        }
    }
}