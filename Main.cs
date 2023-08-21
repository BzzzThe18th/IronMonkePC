using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Utilla;
using HoneyLib.Utils;

namespace IronMonke
{
    public class ModInfo
    {
        public const string _id = "buzzbb.ironmonke";
        public const string _name = "Iron Monke";
    }

    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.7")]
    [BepInDependency("com.buzzbzzzbzzbzzzthe18th.gorillatag.HoneyLib", "1.0.4")]
    [BepInPlugin(ModInfo._id, ModInfo._name, "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        bool modded;
        bool isEnabled;
        GameObject gL;
        AudioSource aL;
        ParticleSystem psL;
        GameObject gR;
        AudioSource aR;
        ParticleSystem psR;

        Main()
        {
            if (!File.Exists(BeeLog.LogPath(false))) File.Create(BeeLog.LogPath(false));
            if (!File.Exists(BeeLog.LogPath(true))) File.Create(BeeLog.LogPath(true));
        }

        void Start()
        {
            if (File.ReadAllLines(BeeLog.LogPath(false)).Length > 0)
            {
                var ogFile = File.ReadAllLines(BeeLog.LogPath(false));
                File.WriteAllLines(BeeLog.LogPath(true), ogFile);
                File.WriteAllText(BeeLog.LogPath(false), "");
            }
            Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            isEnabled = true;
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
            isEnabled = false;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            try
            {
                gL = EasyAssetLoading.InstantiateAsset(Assembly.GetExecutingAssembly(), "IronMonke.gloven", "gloveL");
                aL = gL.GetComponent<AudioSource>();
                gL.transform.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find("rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L"), false);
                psL = gL.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();

                gR = EasyAssetLoading.InstantiateAsset(Assembly.GetExecutingAssembly(), "IronMonke.gloven", "gloveR");
                aR = gR.GetComponent<AudioSource>();
                gR.transform.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find("rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R"), false);
                psR = gR.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
            }
            catch (Exception ex)
            {
                BeeLog.Log(ex.ToString(), true, 1);
            }
        }

        void FixedUpdate()
        {
            if (modded)
            {
                try
                {
                    if (EasyInput.FaceButtonY)
                    {
                        GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(11 * -gL.transform.parent.right, ForceMode.Acceleration);
                        if (!psL.isPlaying) psL.Play();
                        if (!aL.isPlaying) aL.Play();
                        GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 50f * GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                        aL.volume = 0.03f * GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity.magnitude;
                    }
                    else
                    {
                        psL.Stop();
                        aL.Stop();
                    }

                    if (EasyInput.FaceButtonB)
                    {
                        GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(11 * gR.transform.parent.right, ForceMode.Acceleration);
                        if (!psR.isPlaying) psR.Play();
                        if (!aR.isPlaying) aR.Play();
                        GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 50f * GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                        aR.volume = 0.03f * GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity.magnitude;
                    }
                    else
                    {
                        psR.Stop();
                        aR.Stop();
                    }
                }
                catch (Exception e)
                {
                    BeeLog.Log(e.ToString(), true, 1);
                }
            }
        }

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            modded = true;
            if (isEnabled)
            {
                gL.SetActive(true);
                gR.SetActive(true);
            }
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            modded = false;
            if (isEnabled)
            {
                gL.SetActive(false);
                gR.SetActive(false);
            }
        }
    }

    public class BeeLog
    {
        public static void Log(string toLog, bool nonUnity, int? idx)
        {
            if (idx.HasValue)
            {
                switch (idx)
                {
                    case 0:
                        if (!nonUnity) Debug.Log($"Log of {ModInfo._name}: {toLog}");
                        break;
                    case 1:
                        if (!nonUnity) Debug.LogError($"Error of {ModInfo._name}: {toLog}");
                        break;
                    case 2:
                        if (!nonUnity) Debug.LogWarning($"Warning of {ModInfo._name}: {toLog}");
                        break;
                }
            }
            SaveLog(toLog);
        }

        public static string LogPath(bool old) => string.Format("{0}/{1}.log", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ModInfo._name + (old ? "Previous" : ""));

        private static void SaveLog(string toLog)
        {
            var ogFile = File.ReadAllLines(LogPath(false));
            File.WriteAllLines(LogPath(false), ogFile.AddItem(toLog));
        }
    }

    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                {
                    instance = new Harmony(ModInfo._id);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}