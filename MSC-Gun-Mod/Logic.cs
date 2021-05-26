using MSCLoader;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace MSC_Gun_Mod
{
    public class Logic : MonoBehaviour
    {
        private int bullets = 6;
        private int bulletsMag = 18;
        private GameObject fpsCamera;

        // Use this for initialization
        void Start()
        {
            fpsCamera = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera");
        }

        // Update is called once per frame
        void Update()
        {
            // Update is called once per frame
            if (Input.GetMouseButtonDown(0))
            {
                ShootBullet();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Reload());
            }
        }

        void OnGUI()
        {
            // Draw unity OnGUI() here
            //if (true)
            //{
                GUI.Label(new Rect(Screen.height / 2f, Screen.width / 2f, 100f, 50f), $"{bullets} / {bulletsMag}");
            //}
        }

        IEnumerator Reload()
        {
            yield return new WaitForSeconds(2.5f);
            // Reloading variable n is the amount of bullets per clip
            int n = 6;
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
        }

        void ShootBullet()
        {
            if (bullets > 0)
                bullets--;

        }

        void OnMouseOver()
        {
            if (Vector3.Distance(Camera.main.transform.position, gameObject.transform.position) < 1.5f && Input.GetKeyDown(KeyCode.F))
            {
                gameObject.transform.SetParent(fpsCamera.transform);
            }
        }
    }
}