﻿using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Object;
using MultiplayerMod.Structs;
using MultiplayerMod.Networking;

namespace MultiplayerMod
{
    public enum EnemyType
    {
        NullBody,
        FordEarlyExit,
        CorruptedNullBody
    }

    public enum GunType
    {
        EDER22,
        M1911,
        P350,
        MP5K,
        MP5KFlashlight,
        MP5KSabrelake,
        MP5,
        MK18Holo,
        MK18Sabrelake,
        MK18LaserForegrip,
        M16Naked,
        M16Ironsights,
        M16LaserForegrip,
        M16ACOG,
        Uzi
    }

    static class BWUtil
    {
        private static readonly Dictionary<GunType, GameObject> gunPrefabs = new Dictionary<GunType, GameObject>()
        { };

        public static void InitialiseGunPrefabs()
        {
            foreach (UnityEngine.Object obj in FindObjectsOfType<UnityEngine.Object>())
            {
                MelonModLogger.Log("found obj " + obj.name);
                if (obj.TryCast<GameObject>() != null)
                {
                    GameObject go = obj.Cast<GameObject>();
                    if (go.scene.name == null || go.scene.rootCount == 0)
                    {
                        MelonModLogger.Log("Found prefab: " + go.name);
                    }
                }
            }
        }

        public static GameObject SpawnGun(GunType type)
        {
            return Instantiate(gunPrefabs[type]).Cast<GameObject>();
        }

        public static GunType? GetGunType(GameObject gunObj)
        {
            string name = gunObj.name.ToLowerInvariant();
            if (name.Contains("eder22"))
            {
                return GunType.EDER22;
            }
            else
            {
                return null;
            }
        }

        public static BoneworksRigTransforms GetLocalRigTransforms()
        {
            GameObject root = GameObject.Find("[RigManager (Default Brett)]/[SkeletonRig (GameWorld Brett)]/Brett@neutral");

            return GetHumanoidRigTransforms(root);
        }

        public static BoneworksRigTransforms GetHumanoidRigTransforms(GameObject root)
        {
            Transform realRoot = root.transform.Find("SHJntGrp/MAINSHJnt/ROOTSHJnt");

            BoneworksRigTransforms brt = new BoneworksRigTransforms()
            {
                main = root.transform.Find("SHJntGrp/MAINSHJnt"),
                root = root.transform.Find("SHJntGrp/MAINSHJnt/ROOTSHJnt"),
                lHip = realRoot.Find("l_Leg_HipSHJnt"),
                rHip = realRoot.Find("r_Leg_HipSHJnt"),
                spine1 = realRoot.Find("Spine_01SHJnt"),
                spine2 = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt"),
                spineTop = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt"),
                lClavicle = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/l_Arm_ClavicleSHJnt"),
                rClavicle = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/r_Arm_ClavicleSHJnt"),
                lShoulder = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/l_Arm_ClavicleSHJnt/l_AC_AuxSHJnt/l_Arm_ShoulderSHJnt"),
                rShoulder = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/r_Arm_ClavicleSHJnt/r_AC_AuxSHJnt/r_Arm_ShoulderSHJnt"),
                lElbow = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/l_Arm_ClavicleSHJnt/l_AC_AuxSHJnt/l_Arm_ShoulderSHJnt/l_Arm_Elbow_CurveSHJnt"),
                rElbow = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/r_Arm_ClavicleSHJnt/r_AC_AuxSHJnt/r_Arm_ShoulderSHJnt/r_Arm_Elbow_CurveSHJnt"),
                lWrist = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/l_Arm_ClavicleSHJnt/l_AC_AuxSHJnt/l_Arm_ShoulderSHJnt/l_Arm_Elbow_CurveSHJnt/l_WristSHJnt"),
                rWrist = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/r_Arm_ClavicleSHJnt/r_AC_AuxSHJnt/r_Arm_ShoulderSHJnt/r_Arm_Elbow_CurveSHJnt/r_WristSHJnt"),
                neck = realRoot.Find("Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/Neck_01SHJnt"),
                lAnkle = realRoot.Find("l_Leg_HipSHJnt/l_Leg_KneeSHJnt/l_Leg_AnkleSHJnt"),
                rAnkle = realRoot.Find("r_Leg_HipSHJnt/r_Leg_KneeSHJnt/r_Leg_AnkleSHJnt"),
                lKnee = realRoot.Find("l_Leg_HipSHJnt/l_Leg_KneeSHJnt"),
                rKnee = realRoot.Find("r_Leg_HipSHJnt/r_Leg_KneeSHJnt"),
            };

            return brt;
        }

        public static void ApplyRigTransform(BoneworksRigTransforms rigTransforms, RigTFMsgBase tfMsg)
        {
            rigTransforms.main.position = tfMsg.posMain;
            rigTransforms.main.rotation = tfMsg.rotMain;

            rigTransforms.root.position = tfMsg.posRoot;
            rigTransforms.root.rotation = tfMsg.rotRoot;

            rigTransforms.lHip.position = tfMsg.posLHip;
            rigTransforms.lHip.rotation = tfMsg.rotLHip;

            rigTransforms.rHip.position = tfMsg.posRHip;
            rigTransforms.rHip.rotation = tfMsg.rotRHip;

            rigTransforms.lAnkle.position = tfMsg.posLAnkle;
            rigTransforms.lAnkle.rotation = tfMsg.rotLAnkle;

            rigTransforms.rAnkle.position = tfMsg.posRAnkle;
            rigTransforms.rAnkle.rotation = tfMsg.rotRAnkle;

            rigTransforms.lKnee.position = tfMsg.posLKnee;
            rigTransforms.lKnee.rotation = tfMsg.rotLKnee;

            rigTransforms.rKnee.position = tfMsg.posRKnee;
            rigTransforms.rKnee.rotation = tfMsg.rotRKnee;

            rigTransforms.spine1.position = tfMsg.posSpine1;
            rigTransforms.spine1.rotation = tfMsg.rotSpine1;

            rigTransforms.spine2.position = tfMsg.posSpine2;
            rigTransforms.spine2.rotation = tfMsg.rotSpine2;

            rigTransforms.spineTop.position = tfMsg.posSpineTop;
            rigTransforms.spineTop.rotation = tfMsg.rotSpineTop;

            rigTransforms.lClavicle.position = tfMsg.posLClavicle;
            rigTransforms.lClavicle.rotation = tfMsg.rotLClavicle;

            rigTransforms.rClavicle.position = tfMsg.posRClavicle;
            rigTransforms.rClavicle.rotation = tfMsg.rotRClavicle;

            rigTransforms.neck.position = tfMsg.posNeck;
            rigTransforms.neck.rotation = tfMsg.rotNeck;

            rigTransforms.lShoulder.position = tfMsg.posLShoulder;
            rigTransforms.lShoulder.rotation = tfMsg.rotLShoulder;

            rigTransforms.rShoulder.position = tfMsg.posRShoulder;
            rigTransforms.rShoulder.rotation = tfMsg.rotRShoulder;

            rigTransforms.lElbow.position = tfMsg.posLElbow;
            rigTransforms.lElbow.rotation = tfMsg.rotLElbow;

            rigTransforms.rElbow.position = tfMsg.posRElbow;
            rigTransforms.rElbow.rotation = tfMsg.rotRElbow;

            rigTransforms.lWrist.position = tfMsg.posLWrist;
            rigTransforms.lWrist.rotation = tfMsg.rotLWrist;

            rigTransforms.rWrist.position = tfMsg.posRWrist;
            rigTransforms.rWrist.rotation = tfMsg.rotRWrist;
        }

        public static string GetFullNamePath(GameObject obj)
        {
            if (obj.transform.parent == null)
                return obj.name;

            return GetFullNamePath(obj.transform.parent.gameObject) + "/" + obj.name + "|" + obj.transform.GetSiblingIndex();
        }

        public static GameObject GetObjectFromFullPath(string path)
        {
            string[] pathComponents = path.Split('/');

            // First object won't have a sibling index -
            // better hope that the game doesn't have identically named roots!
            // TODO: Could potentially work around this by
            // manually assigning IDs to each root upon scene load
            // but bleh

            GameObject rootObj;
            rootObj = GameObject.Find(pathComponents[0]);
            if (rootObj == null)
                return null;

            if (rootObj.transform.parent != null)
            {
                throw new Exception("Tried to find a root object but didn't get a root object. Try again, dumbass.");
            }

            GameObject currentObj = rootObj;

            for (int i = 1; i < pathComponents.Length; i++)
            {
                string[] splitComponent = pathComponents[i].Split('|');

                int siblingIdx = int.Parse(splitComponent[1]);
                string name = splitComponent[0];

                GameObject newObj = rootObj.transform.GetChild(siblingIdx).gameObject;

                if (newObj.name != name)
                {
                    throw new Exception("Name didn't match expected name at sibling index. Try again, dumbass.");
                }

                currentObj = newObj;
            }

            return currentObj;
        }
    }
}
