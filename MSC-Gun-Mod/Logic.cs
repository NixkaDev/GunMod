using MSCLoader;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using HutongGames.PlayMaker;

namespace MSC_Gun_Mod
{
    public class Logic : MonoBehaviour
    {
        public int bullets = 7;
        public int bulletsMag = 14;
        public float ammoStrength = 1;
        public KeyCode holsterKeybind = Revolver.HolsterKeybind.keybind;

        private GameObject fpsCamera;
        private GameObject impact;
        private GameObject heldItem;
        private PlayMakerFSM[] fsms;
        private PlayMakerFSM handFsm;
        private Rigidbody gunRB;
        
        private Animator animator;
        private ParticleSystem[] particles;
        private ParticleSystem muzzle;
        private AudioSource[] audio;

        private bool isHolding;
        private bool wait;
        public bool isHolstered;
        public bool hideOnHolster;

        enum GunState {
            Shooting,
            Reloading,
            Idle
        }

        GunState state = GunState.Idle;

        // Use this for initialization
        void Start()
        {
            // Get hand fsm
            heldItem = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
            fsms = heldItem.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM fsm in fsms)
            {
                if (fsm.FsmName == "PickUp")
                {
                    handFsm = fsm;
                    //ModConsole.Log($"FOUND FSM {handFsm.FsmName}");
                }
            }
            
            // Get fps camera and setup animations, particles, and audio
            fpsCamera = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera");
            animator = GetComponent<Animator>();
            particles = GetComponentsInChildren<ParticleSystem>();
            audio = GetComponents<AudioSource>();
            gunRB = gameObject.GetComponent<Rigidbody>();
            muzzle = particles[0];
            impact = particles[1].gameObject;
            animator.enabled = false;
        }

        void OnEnable()
        {
            // Set state and reloading to false on enable
            state = GunState.Idle;
            animator.SetBool("Reloading", false);
        }

        public void HolsterPressed()
        {
            if (state != GunState.Idle)
            {
                return;
            }
            // Check if gun is held and then Holster
            if (!isHolstered && isHolding)
            {
                isHolstered = true;
                audio[5].PlayOneShot(audio[5].clip);
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                //ModConsole.Log("Holstered ON");
            }
            else if (isHolstered)
            {
                // Unholster if holstered
                audio[5].PlayOneShot(audio[5].clip);
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                //ModConsole.Log("Holstered OFF");
                isHolstered = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (state != GunState.Idle)
            {
                return;
            }

            if (isHolstered)
            {
                // Do nothing if holstered
                return;
            }

            // Update is called once per frame
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(Shoot());
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                // If bullets is within 0-7 range, reload
                if (bullets >= 7 || bulletsMag <= 0)
                {
                    return;
                }
                StartCoroutine(Reload());
            }

            if (Input.GetKeyDown(KeyCode.F) && isHolding)
            {
                // If holding gun, drop it
                if (!wait)
                {
                    wait = true;
                    return;
                }
                // Unfreeze gun and disable animations
                gunRB.isKinematic = false;
                gunRB.detectCollisions = true;
                gunRB.useGravity = true;
                gameObject.transform.parent = null;
                isHolding = false;
                animator.enabled = false;
                wait = false;
            }
        }

        public void AddAmmo()
        {
            // Ammo bought from shop, add to mag
            bulletsMag += 14;        
        }

        void OnGUI()
        {
            // Draw unity OnGUI() here
            if (isHolding) {
                if (isHolstered && hideOnHolster)
                {
                    return;
                }
                GUIStyle style = new GUIStyle(GUI.skin.GetStyle("label"));
                style.fontSize = 24;
                GUI.Label(new Rect(Screen.width * 0.9f, Screen.height * 0.1f, 100f, 50f), $"{bullets}/{bulletsMag}", style);
            }
        }

        IEnumerator Reload()
        {
            state = GunState.Reloading;
            animator.SetBool("Reloading", true);
            audio[1].PlayOneShot(audio[1].clip);

            // Calculate amount of bullets transferred using math then perform reload animation
            int bullets1 = bullets;
            int bullets2 = bullets;
            int bulletsMag1 = bulletsMag;
            if (bulletsMag1 + bullets1 <= 7) {
                bullets1 += bulletsMag1;
            }
            else {
                bullets1 = 7;
            }

            yield return new WaitForSeconds(1.02f);
            for (int i = 0; i < bullets1-bullets2; i++)
            {
                audio[2].PlayOneShot(audio[2].clip);
                bullets++;
                bulletsMag--;
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(0.24f);
            audio[3].PlayOneShot(audio[3].clip);
            state = GunState.Idle;
            animator.SetBool("Reloading", false);
        }

        IEnumerator Shoot()
        {
            if (isHolding && state == GunState.Idle)
            {
                if (bullets > 0) {
                    bullets--;
                    state = GunState.Shooting;
                    animator.SetBool("Shoot", true);
                    audio[0].PlayOneShot(audio[0].clip);
                    muzzle.Play();

                    // New Raycast system to now detect layers and allow shooting thru for example windows
                    Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
                    PlayMakerFSM fsm;

                    if (hits.Length > 0)
                    {
                        int i = 0;

                        if (hits[hits.Length - 1].rigidbody != null)
                        {
                            if (hits[hits.Length - 1].transform.name != "ammo_box(Clone)")
                            {
                                switch (ammoStrength)
                                {
                                    case 0:
                                        hits[hits.Length - 1].rigidbody.AddForce(-hits[hits.Length - 1].normal * 1000f);
                                        break;
                                    case 1:
                                        hits[hits.Length - 1].rigidbody.AddForce(-hits[hits.Length - 1].normal * 2000f);
                                        break;
                                    case 2:
                                        hits[hits.Length - 1].rigidbody.AddForce(-hits[hits.Length - 1].normal * 20000f);
                                        break;
                                    case 3:
                                        hits[hits.Length - 1].rigidbody.AddForce(-hits[hits.Length - 1].normal * 750000f);
                                        break;
                                }
                            }
                        }

                        foreach (var hit in hits)
                        {
                            // ModConsole.Log(hit.transform.name);
                            // Bullet should not penetrate more than 3 layers 
                            if (i > 3)
                                break;

                            switch (hit.transform.name)
                            {
                                case "HumanTriggerCrime":
                                    fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                    fsm.SendEvent("FALLDOWN");
                                    break;
                                case "HumanTriggerCrime(Clone)":
                                    try
                                    {
                                        // Check if npc shot is inside a vehicle then send a crash event  
                                        string parent = hit.transform.parent.transform.parent.transform.name;
                                        string parent2 = hit.transform.parent.transform.parent.transform.parent.transform.name;
                                        string parent3 = hit.transform.parent.transform.parent.transform.parent.transform.parent.transform.name;

                                        if (parent == "FITTAN" || parent == "KYLAJANI" || parent == "AMIS2" || parent == "KUSKI")
                                        {
                                            fsm = hit.transform.parent.transform.parent.transform.FindChild("CrashEvent").gameObject.GetComponent<PlayMakerFSM>();
                                            fsm.SendEvent("CRASH");
                                        }
                                        else if (parent2 == "FITTAN" || parent2 == "KYLAJANI" || parent2 == "AMIS2" || parent2 == "KUSKI")
                                        {
                                            fsm = hit.transform.parent.transform.parent.transform.parent.transform.FindChild("CrashEvent").gameObject.GetComponent<PlayMakerFSM>();
                                            fsm.SendEvent("CRASH");
                                        }
                                        else if (parent3 == "FITTAN" || parent3 == "KYLAJANI" || parent3 == "AMIS2" || parent3 == "KUSKI")
                                        {
                                            fsm = hit.transform.parent.transform.parent.transform.parent.transform.parent.transform.FindChild("CrashEvent").gameObject.GetComponent<PlayMakerFSM>();
                                            fsm.SendEvent("CRASH");
                                        }
                                        else
                                        {
                                            fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                            fsm.SendEvent("FALLDOWN");
                                        }
                                    }
                                    catch(Exception)
                                    {
                                        fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                        fsm.SendEvent("FALLDOWN");
                                    }
                                    break;
                                case "BreakableWindow":
                                    fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                    fsm.SendEvent("FINISHED");
                                    break;
                                case "BreakableWindowPub":
                                    fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                    fsm.SendEvent("FINISHED");
                                    break;
                                default:
                                    break;
                            }
                            i++;
                        }
                        // Create impact effect
                        UnityEngine.Object impactGo = Instantiate(impact, hits[hits.Length - 1].point, Quaternion.LookRotation(hits[hits.Length - 1].normal));
                        Destroy(impactGo, 2f);
                    }
                } else {
                    state = GunState.Shooting;
                    audio[4].PlayOneShot(audio[4].clip);
                }

                yield return new WaitForSeconds(1f);

                animator.SetBool("Shoot", false);
                state = GunState.Idle;
            }
        }

        void OnMouseOver()
        {
            if (Vector3.Distance(Camera.main.transform.position, gameObject.transform.position) < 1.5f && Input.GetKeyDown(KeyCode.F) && !isHolding)
            {
                // Drop the gun if player is holding it
                handFsm.SendEvent("PROCEED Drop");

                // Teleport gun to rightful place
                gameObject.transform.SetParent(fpsCamera.transform);
                gameObject.transform.localPosition = new Vector3(0.1200185f, -0.2599683f, 0.04999715f);
                gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                // Disable gun movement
                gunRB.isKinematic = true;
                gunRB.detectCollisions = false;
                gunRB.useGravity = false;

                // Enable animations and isHolding
                isHolding = true;
                animator.enabled = true;
            }
        }
    }
}