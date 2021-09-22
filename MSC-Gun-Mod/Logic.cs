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
        //public bool sicko = false;

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
        //private bool wait2;

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


        // Update is called once per frame
        void Update()
        {
            if (state != GunState.Idle)
            {
                return;
            }

            /*if (sicko && !wait2)
            {
                ModPrompt.CreateYesNoPrompt("[WARNING] This will make the bullet destroy everything in hits path... Are you sure?", "Yes No Prompt", () => sicko = true , () => sicko = false);
                if (sicko)
                {
                    wait2 = true;
                }
                else
                {
                    
                }

            }*/

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

            yield return new WaitForSeconds(5f);
            // Reloading variable n is the amount of bullets per clip
            int n = 7;
            if (bulletsMag + bullets <= n) {
                bullets += bulletsMag;
                bulletsMag = 0;
            } else if (bullets > 0) {
                bulletsMag += bullets - n;
                bullets = n;
            } else {
                bulletsMag -= n;
                bullets = n;
            }
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

                    /*if (sicko)
                    {
                        Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
                        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
                        foreach (var hit in hits)
                        {
                            GameObject.Destroy(hit.transform.gameObject);
                        }
                    }
                    else
                    {*/

                    // Create raycast to catch where the bullet hit, then determine outcome
                    RaycastHit hit;
                    if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, 100f))
                    {
                        //ModConsole.Log(hit.transform.name);
                        PlayMakerFSM fsm;

                        if (hit.rigidbody != null)
                        {
                            if (hit.transform.name != "ammo_box(Clone)") 
                                hit.rigidbody.AddForce(-hit.normal * 1500f);
                        }

                        string name = hit.transform.name;

                        switch (name)
                        {
                            case "HumanTriggerCrime":
                                fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                fsm.SendEvent("FALLDOWN");
                                break;
                            case "BreakableWindow":
                                fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                fsm.SendEvent("FINISHED");
                                break;
                            case "BreakableWindowPub":
                                fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                fsm.SendEvent("FINISHED");
                                break;

                                /* Transform gameobject doesnt detect windshield being hit
                                    * 
                                    * case "Windshield":
                                    fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                    fsm.SendEvent("BROKEN");
                                    break;
                                case "WindshieldLeft":
                                    fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                    fsm.SendEvent("BROKEN");
                                    break;
                                case "WindshieldRight":
                                    fsm = hit.transform.GetComponent<PlayMakerFSM>();
                                    fsm.SendEvent("BROKEN");
                                    break;*/
                        }
                        UnityEngine.Object impactGo = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactGo, 2f);
                        //}
                    }
                } else { 
                    Reload();
                }

                /*if (sicko)
                {
                    yield return new WaitForSeconds(0f);
                }
                else
                {*/
                    yield return new WaitForSeconds(1f);
                //}
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
                gameObject.transform.SetParent(fpsCamera.transform);
                gameObject.transform.localPosition = new Vector3(0.1200185f, -0.2599683f, 0.04999715f);
                gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                gunRB.isKinematic = true;
                gunRB.detectCollisions = false;
                gunRB.useGravity = false;
                isHolding = true;
                animator.enabled = true;
            }
        }
    }
}