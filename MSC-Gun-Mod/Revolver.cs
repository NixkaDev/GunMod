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
        public override string Version => "1.2.0";
        public override string Description => "Hello! This mod adds Revolver to your My Summer Car game.";
        public override string UpdateLink => "https://www.nexusmods.com/mysummercar/mods/1021"; // Update Link

        //private static SettingToggle sickoMode;

        private static GameObject gun;
        private GameObject ammo;

        static internal SettingSlider AmmoStrength;
        static internal SettingKeybind HolsterKeybind;
        static internal SettingBoolean HideHolster;

        readonly string[] ammoStrengths = { "Light", "Realistic", "Strong", "INSANE" };


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

            SaveData RSavedata = ModSave.Load<SaveData>("Revolver"); // create savedata with filename "Revolver"

            if (RSavedata.GunPosition != Vector3.zero) // check for save file and if it exists move gun back to saved pos
            {
                gun.transform.position = RSavedata.GunPosition;
                gun.transform.localEulerAngles = RSavedata.GunRotation;
                gun.GetComponent<Logic>().bullets = RSavedata.GunAmmo;
                gun.GetComponent<Logic>().bulletsMag = RSavedata.MagAmmo;
            }
            else // if it doesn't then just return back to uncle
            {
                gun.transform.position = new Vector3(18.53404f, 1.6808104f, -55.73272f);
                gun.transform.eulerAngles = new Vector3(5.316792f, 298.0635f, 283.6909f);
            }
            ammo.transform.eulerAngles = new Vector3(0.04031519f, 47.4399f, 269.9601f);
            ammo.transform.position = new Vector3(-1551.199f, 4.717276f, 1182.889f);

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
            HolsterKeybind = modSettings.AddKeybind("holsterKeybind", "HOLSTER KEYBIND", KeyCode.Alpha3);
            HideHolster = modSettings.AddBoolean("hideBulletOnHolster", true);
            SettingToggle toggle = modSettings.AddToggle("hideBulletOnHolster", "HIDE BULLETS ON HOLSTER", HideHolster.Value, () => gun.GetComponent<Logic>().hideOnHolster = !gun.GetComponent<Logic>().hideOnHolster);
            SettingSpacer spacer = modSettings.AddSpacer(5f);
            AmmoStrength = modSettings.AddSlider("sliderAmmo", "AMMO STRENGTH", 1,0,3, () => gun.GetComponent<Logic>().ammoStrength = AmmoStrength.Value);
            AmmoStrength.gameObject.AddComponent<UITooltip>().toolTipText = 
                "<color=yellow>Light</color>: <color=white>Gun doesn't knock back items as far</color>\n" +
                "<color=yellow>Realistic (recommended)</color>: <color=white>Most realistic option for this gun</color>\n" +
                "<color=yellow>Strong</color>: <color=white>Bullets are stronger</color>\n" +
                "<color=red>INSANE:</color> <color=white>Basically explosive ammo, sends cars flying</color>";
            AmmoStrength.TextValues = ammoStrengths;
            AmmoStrength.ChangeValueText();
            spacer = modSettings.AddSpacer(5f);
            SettingHeader header = modSettings.AddHeader("INFO");
            SettingButton nexusButton = modSettings.AddButton("nexusButton", "GAME INFO", () => { Process.Start("https://nixka.net/revolver"); });
            //sickoMode = modSettings.AddToggle("sickoMode", "SICKO MODE", false);
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
            // Detect and send holster keybind
            if (Input.GetKeyDown(HolsterKeybind.keybind))
            {
                gun.gameObject.GetComponent<Logic>().HolsterPressed();
            }
        }
    }
}
