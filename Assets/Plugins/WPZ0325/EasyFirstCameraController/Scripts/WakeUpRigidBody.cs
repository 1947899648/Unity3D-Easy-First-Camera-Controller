using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WPZ0325.EasyFirstCameraController
{
    public class WakeUpRigidBody : MonoBehaviour
    {
        [SerializeField] Rigidbody rigidbody;
        private void Awake()
        {
            rigidbody = this.GetComponent<Rigidbody>();
        }

        private void Update()
        {

            if (rigidbody && rigidbody.IsSleeping())
            {
                rigidbody.WakeUp();
                rigidbody.angularVelocity = Vector3.up * 10;
            }
        }
    }
}

