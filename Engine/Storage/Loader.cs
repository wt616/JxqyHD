﻿using System;
using Engine.Gui;
using Engine.ListManager;
using Engine.Script;
using Engine.Weather;
using IniParser;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Engine.Storage
{
    public static class Loader
    {
        private static void LoadGameFile()
        {
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(StorageBase.GameIniFilePath);

                //state
                var state = data["State"];
                Globals.TheMap.LoadMap(state["Map"]);
                NpcManager.Load(state["Npc"]);
                ObjManager.Load(state["Obj"]);
                BackgroundMusic.Play(state["Bgm"]);
                Globals.PlayerIndex = int.Parse(state["Chr"]);

                //option
                var option = data["Option"];
                Map.MapTime = int.Parse(option["MapTime"]);
                WeatherManager.ShowSnow(int.Parse(option["SnowShow"]) != 0);
                if (!string.IsNullOrEmpty(option["RainFile"]))
                {
                    WeatherManager.BeginRain(option["RainFile"]);
                }
                Globals.IsWaterEffectEnabled =
                    int.Parse(option["Water"]) != 0;
                Map.DrawColor = StorageBase.GetColorFromString(option["MpcStyle"]);
                Sprite.DrawColor = StorageBase.GetColorFromString(option["AsfStyle"]);

                //Timer
                var timer = data["Timer"];
                if (timer != null)
                {
                    var isOn = timer["IsOn"] != "0";
                    if (isOn)
                    {
                        ScriptExecuter.OpenTimeLimit(int.Parse(timer["TotalSecond"]));
                        var isHide = timer["IsTimerWindowShow"] != "1";
                        if (isHide)
                        {
                            ScriptExecuter.HideTimerWnd();
                        }
                        if (timer["IsScriptSet"] != "0")
                        {
                            ScriptExecuter.SetTimeScript(int.Parse(timer["TriggerTime"]),
                                timer["TimerScript"]);
                        }
                    }
                }

                //Variables
                ScriptExecuter.LoadVariables(data["Var"]);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Game.ini", StorageBase.GameIniFilePath, exception);
            }
        }

        /// <summary>
        /// Goods and magic list must load first.
        /// Player using goods list to apply equip special effect.
        /// Player using magic list to load current use magic.
        /// </summary>
        private static void LoadPlayer()
        {
            var path = StorageBase.PlayerFilePath;
            try
            {
                Globals.ThePlayer = new Player(path);
                Globals.TheCarmera.CenterPlayerInCamera();
                GoodsListManager.ApplyEquipSpecialEffectFromList(Globals.ThePlayer);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Player", path, exception);
            }
            var isFemale = Globals.PlayerIndex != 0;
            GuiManager.StateInterface.IsFemale =
                GuiManager.EquipInterface.IsFemale = isFemale;
        }

        private static void LoadPartner()
        {
            NpcManager.LoadPartner(StorageBase.PartnerFilePath);
        }

        /// <summary>
        /// GuiManager must start first
        /// </summary>
        private static void LoadMagicGoodMemoList()
        {
            GuiManager.Load(StorageBase.MagicListFilePath,
                StorageBase.GoodsListFilePath,
                StorageBase.MemoListIniFilePath);
        }

        private static void LoadMagicGoodList()
        {
            MagicListManager.LoadList(StorageBase.MagicListFilePath);
            GoodsListManager.LoadList(StorageBase.GoodsListFilePath);
        }

        private static void LoadTraps()
        {
            Globals.TheMap.LoadTrap(StorageBase.TrapsFilePath);
        }

        private static void LoadTrapIgnoreList()
        {
            Globals.TheMap.LoadTrapIndexIgnoreList(StorageBase.TrapIndexIgnoreListFilePath);
        }

        /// <summary>
        /// Load game from "save/game" directory
        /// GuiManager must started first
        /// </summary>
        public static void LoadGame()
        {
            //Clear
            MagicManager.Clear();
            NpcManager.ClearAllNpc();
            ObjManager.ClearAllObjAndFileName();
            Globals.TheMap.Free();
            ScriptExecuter.Init();
            GuiManager.CloseTimeLimit();

            LoadGameFile();
            LoadMagicGoodMemoList();
            LoadPlayer();
            //Apply xiulian magic to player
            Globals.ThePlayer.XiuLianMagic = MagicListManager.GetItemInfo(
                MagicListManager.XiuLianIndex);

            LoadPartner();
            LoadTraps();
            LoadTrapIgnoreList();

            Globals.TheCarmera.CenterPlayerInCamera();
            
            GameState.State = GameState.StateType.Playing;
            Globals.TheGame.IsGamePlayPaused = false;
            GuiManager.ShowAllPanels(false);
        }

        /// <summary>
        /// Load game form 0-7
        /// </summary>
        /// <param name="index">Load index</param>
        public static void LoadGame(int index)
        {
            if (!StorageBase.IsIndexInRange(index)) return;
            StorageBase.ClearGameAndCopySaveToGame(index);
            LoadGame();
        }

        public static void NewGame()
        {
            ScriptExecuter.RunScript("NewGame.txt");
        }

        public static void ChangePlayer(int index)
        {
            Saver.SavePlayer();
            Globals.PlayerIndex = index;
            GuiManager.StateInterface.IsFemale = (index == 1);
            LoadMagicGoodList();
            LoadPlayer();
        }
    }
}