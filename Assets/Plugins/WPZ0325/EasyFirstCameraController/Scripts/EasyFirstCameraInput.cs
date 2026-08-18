using UnityEngine;
using UnityEngine.Serialization;
using EasyFirstCameraControllerType = WPZ0325.EasyFirstCameraController.EasyFirstCameraController;

namespace WPZ0325.EasyFirstCameraController
{
    [RequireComponent(typeof(EasyFirstCameraControllerType))]
    [DefaultExecutionOrder(-100)]
    public class EasyFirstCameraInput : MonoBehaviour
    {
    #region 【序列化字段】Serialized Fields

        [Header("控制键位设置")]
        [SerializeField, FormerlySerializedAs("_fornt")] private KeyCode _front = KeyCode.W;
        [SerializeField] private KeyCode _back = KeyCode.S;
        [SerializeField] private KeyCode _left = KeyCode.A;
        [SerializeField] private KeyCode _right = KeyCode.D;
        [SerializeField] private KeyCode _up = KeyCode.R;
        [SerializeField] private KeyCode _down = KeyCode.F;
        [SerializeField] private KeyCode _speedUp = KeyCode.LeftShift;
        [SerializeField] private string _axesNameHorizontal = "Mouse X";
        [SerializeField] private string _axesNameVertical = "Mouse Y";

    #endregion

        private EasyFirstCameraControllerType _controller;

        private void Awake()
        {
            _controller = GetComponent<EasyFirstCameraControllerType>();
        }

        private void Update()
        {
            Vector3 moveDirSelf = Vector3.zero;
            Vector3 moveDirWorld = Vector3.zero;
            float rotateDirHorizontal = 0.0f;
            float rotateDirVertical = 0.0f;
            bool isShiftingInput = Input.GetMouseButton(1);
            if (isShiftingInput)
            {
                moveDirSelf += Input.GetKey(_front) ? Vector3.forward : Vector3.zero;
                moveDirSelf += Input.GetKey(_back) ? Vector3.back : Vector3.zero;
                moveDirSelf += Input.GetKey(_left) ? Vector3.left : Vector3.zero;
                moveDirSelf += Input.GetKey(_right) ? Vector3.right : Vector3.zero;
                moveDirSelf = moveDirSelf.normalized;
                moveDirWorld += Input.GetKey(_up) ? Vector3.up : Vector3.zero;
                moveDirWorld += Input.GetKey(_down) ? Vector3.down : Vector3.zero;
                moveDirWorld = moveDirWorld.normalized;

                rotateDirHorizontal = Input.GetAxisRaw(_axesNameHorizontal);
                rotateDirVertical = (-1) * Input.GetAxisRaw(_axesNameVertical);
            }

            Vector3 moveDir = moveDirWorld + _controller.Target.TransformDirection(moveDirSelf);
            _controller.SetMoveDirection(moveDir.normalized, isShiftingInput && Input.GetKey(_speedUp));
            _controller.SetRotateDirection(rotateDirHorizontal, rotateDirVertical);
        }
    }
}
