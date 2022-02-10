using BepInEx;
using System;
using UnityEngine;
using UnityEngine.XR;
using GorillaLocomotion;
using Utilla;

namespace IronMonke
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class IronMonkePlugin : BaseUnityPlugin
    {
        bool inRoom;

        private XRNode rNode = XRNode.RightHand;
        private XRNode lNode = XRNode.LeftHand;

        Rigidbody RB;
        Transform rHandT;
        Transform lHandT;

        private float thrust;
        private float maxSpeed = 100f;

        GameObject quitBox;
        Transform quitBoxT;

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            Utilla.Events.GameInitialized += OnGameInitialized;

            if (RB != null) RB.useGravity = true;
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();

            if(RB != null) RB.useGravity = true;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            RB = Player.Instance.bodyCollider.attachedRigidbody;

            rHandT = Player.Instance.rightHandTransform;
            lHandT = Player.Instance.leftHandTransform;

            quitBox = GameObject.Find("QuitBox");
            quitBoxT = quitBox.transform;
            quitBoxT.position = new Vector3 (999999999, 999999999, 999999999);

        }

        void FixedUpdate()
        {
            if(inRoom)
            {
                bool primaryRight;

                InputDevices.GetDeviceAtXRNode(rNode).TryGetFeatureValue(CommonUsages.primaryButton, out primaryRight);
                bool B = primaryRight;

                bool primaryLeft;

                InputDevices.GetDeviceAtXRNode(lNode).TryGetFeatureValue(CommonUsages.primaryButton, out primaryLeft);
                bool Y = primaryLeft;

                if(B)
                {
                    RB.AddForce(thrust * rHandT.right, ForceMode.Acceleration);
                }
                if(Y)
                {
                    RB.AddForce(-thrust * lHandT.right, ForceMode.Acceleration);
                }

                if(Y&&B)
                {
                    thrust = 3.0f;
                } else
                {
                    thrust = 5.0f;
                }

                bool wasInput = Y | B;
                if (wasInput && RB.useGravity) RB.useGravity = false;
                if (!wasInput && !RB.useGravity) RB.useGravity = true;

                if(B||Y) RB.velocity = Vector3.ClampMagnitude(RB.velocity, maxSpeed);
            }
            
        }

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;

            RB.useGravity = true;
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = true;

            RB.useGravity = true;
        }
    }
}
