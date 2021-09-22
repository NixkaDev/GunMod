using MSCLoader;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using HutongGames.PlayMaker;

namespace MSC_Gun_Mod
{
    public class Ammo : MonoBehaviour
    {
        private GameObject register;
        private PlayMakerFSM fsm;
        // Use this for initialization
        void Start()
        {
            register = GameObject.Find("STORE/StoreCashRegister/Register");
            fsm = register.GetComponent<PlayMakerFSM>();
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        }

        void OnEnable()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnGUI()
        {

        }
        string Title
        {
            get => FsmVariables.GlobalVariables.FindFsmString("GUIinteraction").Value;
            set => FsmVariables.GlobalVariables.FindFsmString("GUIinteraction").Value = value;
        }
        string Subtitle
        {
            get => FsmVariables.GlobalVariables.FindFsmString("GUIsubtitle").Value;
            set => FsmVariables.GlobalVariables.FindFsmString("GUIsubtitle").Value = value;
        }

        void OnMouseOver()
        {
            if (Vector3.Distance(Camera.main.transform.position, gameObject.transform.position) < 1.5f)
            {
                Title = "REVOLVER AMMO(14) 500 MK";
            }
            if (Vector3.Distance(Camera.main.transform.position, gameObject.transform.position) < 1.5f && Input.GetMouseButtonDown(0))
            {
                float money = FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value;
                if (money >= 500)
                {
                    fsm.SendEvent("PURCHASE");
                    Revolver.Purchase();
                    FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value -= 500f;
                }
                else
                {
                    Subtitle = "NOT ENOUGH MONEY";
                }
            }
        }
    }
}