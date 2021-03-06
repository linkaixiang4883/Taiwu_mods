﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;

namespace ShowRichResources
{

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

        public bool ShowMax = true; //false为显示资源，true为显示资源上限
        public int yuZhi = 150;
    }
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Settings settings;
        public static bool flag = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            logger.Log("ShowRichResources已加载");
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ShowMax = GUILayout.Toggle(settings.ShowMax, "显示资源上限");
            GUILayout.Label("勾选时根据资源上限和阈值显示图标，未勾选时，根据资源和阈值显示图标");
            GUILayout.BeginHorizontal();
            GUILayout.Label("显示阈值:", GUILayout.Width(60));
            settings.yuZhi = Int32.Parse(GUILayout.TextArea(settings.yuZhi.ToString(), GUILayout.Width(50)));
            DateFile tbl = DateFile.instance;
            if (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId))
            {
                GUILayout.Label("存档未载入!");
            }
            else
            {
                if (GUILayout.Button("应用", GUILayout.Width(80)))
                {
                    RefreshResourcesIcon();
                }
            }
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        //刷新当前地图全部资源格子
        public static void RefreshResourcesIcon()
        {
            int bianchang = Int32.Parse(DateFile.instance.partWorldMapDate[DateFile.instance.mianPartId][98]);
            int placeNum = bianchang * bianchang;
            
            for (int j = 0; j < placeNum; j++)
            {
                WorldMapPlace worldMapPlace = WorldMapSystem.instance.worldMapPlaces[j];
                if (DateFile.instance.HaveShow(DateFile.instance.mianPartId, j) > 0)
                {
                    if (int.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 89)) != 6)
                    {
                        worldMapPlace.resourceIconHolder.gameObject.SetActive(true);
                        int[] placeResource = DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, j);
                        int[] placeResourceMax = new int[]
                        {
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 1)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 2)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 3)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 4)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 5)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 6)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 7)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, j, 8))
                        };
                        int[] compare;
                        if(settings.ShowMax)
                        {
                            compare = placeResourceMax;
                        }
                        else
                        {
                            compare = placeResource;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            if (compare[i] >= settings.yuZhi)
                            {
                                worldMapPlace.resourceIcon[i].SetActive(true);
                            }
                            else
                            {
                                worldMapPlace.resourceIcon[i].SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        worldMapPlace.resourceIconHolder.gameObject.SetActive(false);
                    }
                }
                else
                {
                    worldMapPlace.resourceIconHolder.gameObject.SetActive(false);
                }
            }
        }

        //反射获取WorldMapPlace下私有变量
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindingFlags);
            return field.GetValue(instance);
        }

        //每当资源图标刷新，在其后重新刷新
        [HarmonyPatch(typeof(WorldMapPlace), "UpdatePaceResource")]
        public static class WorldMapPlace_UpdatePaceResource_Patch
        {
            public static void Postfix(WorldMapPlace __instance)
            {
                int placeId = Int32.Parse(GetInstanceField(typeof(WorldMapPlace), __instance, "placeId").ToString());
                if (DateFile.instance.HaveShow(DateFile.instance.mianPartId, placeId) > 0)
                {
                    if (int.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 89)) != 6)
                    {
                        __instance.resourceIconHolder.gameObject.SetActive(true);
                        int[] placeResource = DateFile.instance.GetPlaceResource(DateFile.instance.mianPartId, placeId);
                        int[] placeResourceMax = new int[]
                        {
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 1)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 2)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 3)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 4)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 5)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 6)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 7)),
                            Int32.Parse(DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, placeId, 8))
                        };
                        int[] compare;
                        if (settings.ShowMax)
                        {
                            compare = placeResourceMax;
                        }
                        else
                        {
                            compare = placeResource;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            if (compare[i] >= settings.yuZhi)
                            {
                                __instance.resourceIcon[i].SetActive(true);
                            }
                            else
                            {
                                __instance.resourceIcon[i].SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        __instance.resourceIconHolder.gameObject.SetActive(false);
                    }
                }
                else
                {
                    __instance.resourceIconHolder.gameObject.SetActive(false);
                }
            }
        }
    }
}
