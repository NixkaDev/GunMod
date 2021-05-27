using MSCLoader;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace MSC_Gun_Mod
{
    public class Logic : MonoBehaviour
    {
        private int bullets = 7;
        private int bulletsMag = 14;

        private GameObject fpsCamera;
        private Animator animator;
        private ParticleSystem muzzle;
        private AudioSource[] audio;
        private bool isHolding;
        private bool wait;

        enum GunState {
            Shooting,
            Reloading,
            Idle
        }

        GunState state = GunState.Idle;

        // Use this for initialization
        void Start()
        {
            fpsCamera = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera");
            animator = GetComponent<Animator>();
            muzzle = GetComponentInChildren<ParticleSystem>();
            audio = GetComponents<AudioSource>();
            animator.enabled = false;
        }

        void OnEnable()
        {
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

            // Update is called once per frame
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(Shoot());
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (bullets >= 7 || bulletsMag <= 0)
                {
                    return;
                }
                StartCoroutine(Reload());
            }

            if (Input.GetKeyDown(KeyCode.F) && isHolding)
            {
                if (!wait)
                {
                    wait = true;
                    return;
                }
                gameObject.GetComponent<Rigidbody>().detectCollisions = true;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                gameObject.transform.parent = null;
                isHolding = false;
                animator.enabled = false;
                wait = false;
            }
        }

        IEnumerator waitASecond()
        {
            yield return new WaitForSeconds(1f);
            animator.SetBool("Shoot", false);
        }

        void OnGUI()
        {
            // Draw unity OnGUI() here
            if (isHolding) {
                GUIStyle style = new GUIStyle(GUI.skin.GetStyle("label"));
                style.fontSize = 24;
                GUI.Label(new Rect(Screen.height * 1.6f, Screen.width * 0.025f, 100f, 50f), $"{bullets}/{bulletsMag}", style);
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
            if (isHolding)
            {
                if (bullets > 0) {
                    bullets--;
                    state = GunState.Shooting;
                    animator.SetBool("Shoot", true);
                    audio[0].PlayOneShot(audio[0].clip);
                    muzzle.Play();
                } else { 
                    Reload();
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
                gameObject.transform.SetParent(fpsCamera.transform);
                gameObject.transform.localPosition = new Vector3(0.1200185f, -0.2599683f, 0.01999715f);
                gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                gameObject.GetComponent<Rigidbody>().useGravity = false;
                isHolding = true;
                animator.enabled = true;
            }
        }
    }
}