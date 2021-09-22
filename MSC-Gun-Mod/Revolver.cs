using MSCLoader;
using UnityEngine;
using System.Collections;
using System.Diagnostics;

namespace MSC_Gun_Mod
{
    public class Revolver : Mod
    {
        public override string ID => "Revolver";
        public override string Name => "Revolver";
        public override string Author => "Nika";
        public override string Version => "1.0";
        public override string Description => "Hello! This mod adds Revolver to your My Summer Car game.";

        //private static SettingToggle sickoMode;

        private static GameObject gun;
        private GameObject ammo;

        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
            ModSave.Delete("Revolver");
            ModConsole.Log("[Revolver] Reset!");
        }
        
        public override void OnLoad()
        {
            AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate(Properties.Resources.gun);
            gun = Object.Instantiate(bundle.LoadAsset<GameObject>("Revolver.prefab"));
            gun.AddComponent<Logic>();
            ammo = Object.Instantiate(bundle.LoadAsset<GameObject>("ammo_box.prefab"));
            ammo.AddComponent<Ammo>();
            bundle.Unload(false);
            gun.tag = "PART";
            gun.layer = 19;

            SaveData RSavedata = ModSave.Load<SaveData>("Revolver"); //create savedata with filename "Revolver"

            if (RSavedata.GunPosition != Vector3.zero) //check for save file and if it exists install the dash
            {
                gun.transform.position = RSavedata.GunPosition;
                gun.transform.localEulerAngles = RSavedata.GunRotation;
                gun.GetComponent<Logic>().bullets = RSavedata.GunAmmo;
                gun.GetComponent<Logic>().bulletsMag = RSavedata.MagAmmo;
            }
            else // if it doesn't then just return back to the shop
            {
                gun.transform.position = new Vector3(29.38505f, 3.70145114f, -39.267f);
                gun.transform.eulerAngles = new Vector3(5.203771f, 237.6363f, 281.5044f);
            }
            ammo.transform.eulerAngles = new Vector3(0.04031519f, 47.4399f, 269.9601f);
            ammo.transform.position = new Vector3(-1551.199f, 4.717276f, 1182.889f);

            CheckToggle();
            ModConsole.Log($"Revolver Mod Loaded!");
        }

        public static void Purchase()
        {
            gun.GetComponent<Logic>().bulletsMag += 14;
        }

        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings.
            SettingButton nexusButton = modSettings.AddButton("nexusButton", "Game Info", () => { Process.Start("https://nixka.net/revolver"); });
            //sickoMode = modSettings.AddToggle("sickoMode", "SICKO MODE", false);
        }

        void CheckToggle()
        {
            //gun.GetComponent<Logic>().sicko = sickoMode;
        }

        // SaveData setup. Creds - Arthur
        public class SaveData // All Variables that i want to read/write
        {
            public Vector3 GunPosition = Vector3.zero;
            public Vector3 GunRotation = Vector3.zero;
            public int GunAmmo = 0;
            public int MagAmmo = 0;
        }

        public override void OnSave()
        {
            // Called once, when save and quit
            // Serialize your save file here.
            ModSave.Save("Revolver", new SaveData // Write variables to file
            {
                GunPosition = gun.transform.position,
                GunRotation = gun.transform.localEulerAngles,
                GunAmmo = gun.GetComponent<Logic>().bullets,
                MagAmmo = gun.GetComponent<Logic>().bulletsMag
            });
        }

        public override void OnGUI()
        {
            // Draw unity OnGUI() here
        }

        public override void Update()
        {
            //CheckToggle();
        }
    }
}
