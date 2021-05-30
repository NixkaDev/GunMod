﻿using MSCLoader;
using UnityEngine;
using System.Collections;

namespace MSC_Gun_Mod
{
    public class Revolver : Mod
    {
        public override string ID => "Revolver";
        public override string Name => "La Revolver";
        public override string Author => "Nika";
        public override string Version => "1.0";

        private GameObject gun;

        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
        }
        
        public override void OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate(Properties.Resources.gun);
            gun = Object.Instantiate(bundle.LoadAsset<GameObject>("Revolver.prefab"));
            gun.AddComponent<Logic>();
            bundle.Unload(false);
            Vector3[] ass = { new Vector3(13431f, 93184193f, 1823f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) };
            //gun.transform.position = ass[Random.Range(0, ass.Length)];
            gun.tag = "PART";
            gun.layer = 19;
            ModConsole.Log($"La Revolver Loaded!");
        }

        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings.
            
        }

        public override void OnSave()
        {
            // Called once, when save and quit
            // Serialize your save file here.
        }

        public override void OnGUI()
        {
            // Draw unity OnGUI() here
        }

        public override void Update()
        {
            
        }

    }
}
