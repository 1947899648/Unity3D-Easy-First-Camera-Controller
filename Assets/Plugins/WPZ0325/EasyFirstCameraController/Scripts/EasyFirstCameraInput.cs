using UnityEngine;
using UnityEngine.Serialization;
using EasyFirstCameraControllerType = WPZ0325.EasyFirstCameraController.EasyFirstCameraController;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// 原因找到了：旧输入系统的 sensitivity 缩放。
        /// 旧分支：Input.GetAxisRaw("Mouse X") 返回的值受 Input Manager 中 "Mouse X" 轴默认 Sensitivity: 0.1 缩放 → 约 delta × 0.1
        /// 新分支：Mouse.current.delta.x 返回原始像素增量，无缩放 → delta × 1.0
        /// 差了 10 倍，所以新模式下旋转明显更快（移动本身无差异，是旋转让整体观感变快）。修复：新分支加一个灵敏度缩放字段，默认 0.1 对齐旧默认值：
        /// </summary>
        [Header("新输入系统参数")]
        [Tooltip("鼠标灵敏度缩放，默认 0.1 对齐旧输入管理器 Mouse 轴默认 Sensitivity")]
        [SerializeField] private float _mouseSensitivity = 0.1f;
    #endif

    #endregion

        private EasyFirstCameraControllerType _controller;

        private void Awake()
        {
            _controller = GetComponent<EasyFirstCameraControllerType>();
        }

    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// KeyCode 转新输入系统 Key（键盘键位全量手写映射，无对应按键返回 Key.None）
        /// </summary>
        private static Key ToKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                // 功能键区
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Pause: return Key.Pause;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Print: return Key.PrintScreen;
                // 单字符符号区
                case KeyCode.Quote: return Key.Quote;
                case KeyCode.Comma: return Key.Comma;
                case KeyCode.Minus: return Key.Minus;
                case KeyCode.Period: return Key.Period;
                case KeyCode.Slash: return Key.Slash;
                case KeyCode.Semicolon: return Key.Semicolon;
                case KeyCode.Equals: return Key.Equals;
                case KeyCode.LeftBracket: return Key.LeftBracket;
                case KeyCode.Backslash: return Key.Backslash;
                case KeyCode.RightBracket: return Key.RightBracket;
                case KeyCode.BackQuote: return Key.Backquote;
                // 数字键
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                // 字母键
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;
                // 编辑与导航键
                case KeyCode.Delete: return Key.Delete;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.Insert: return Key.Insert;
                case KeyCode.Home: return Key.Home;
                case KeyCode.End: return Key.End;
                case KeyCode.PageUp: return Key.PageUp;
                case KeyCode.PageDown: return Key.PageDown;
                // 小键盘
                case KeyCode.Keypad0: return Key.Numpad0;
                case KeyCode.Keypad1: return Key.Numpad1;
                case KeyCode.Keypad2: return Key.Numpad2;
                case KeyCode.Keypad3: return Key.Numpad3;
                case KeyCode.Keypad4: return Key.Numpad4;
                case KeyCode.Keypad5: return Key.Numpad5;
                case KeyCode.Keypad6: return Key.Numpad6;
                case KeyCode.Keypad7: return Key.Numpad7;
                case KeyCode.Keypad8: return Key.Numpad8;
                case KeyCode.Keypad9: return Key.Numpad9;
                case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
                case KeyCode.KeypadDivide: return Key.NumpadDivide;
                case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
                case KeyCode.KeypadMinus: return Key.NumpadMinus;
                case KeyCode.KeypadPlus: return Key.NumpadPlus;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.KeypadEquals: return Key.NumpadEquals;
                // 功能键 F1-F15
                case KeyCode.F1: return Key.F1;
                case KeyCode.F2: return Key.F2;
                case KeyCode.F3: return Key.F3;
                case KeyCode.F4: return Key.F4;
                case KeyCode.F5: return Key.F5;
                case KeyCode.F6: return Key.F6;
                case KeyCode.F7: return Key.F7;
                case KeyCode.F8: return Key.F8;
                case KeyCode.F9: return Key.F9;
                case KeyCode.F10: return Key.F10;
                case KeyCode.F11: return Key.F11;
                case KeyCode.F12: return Key.F12;
                // 修饰键
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightCommand: return Key.RightMeta;
                case KeyCode.LeftCommand: return Key.LeftMeta;
                case KeyCode.LeftWindows: return Key.LeftWindows;
                case KeyCode.RightWindows: return Key.RightWindows;
                case KeyCode.AltGr: return Key.RightAlt;
                // 无对应按键（组合符号、鼠标、手柄等）
                default: return Key.None;
            }
        }
    #endif

        private void Update()
        {
    #if ENABLE_INPUT_SYSTEM
            Vector3 moveDirSelf = Vector3.zero;
            Vector3 moveDirWorld = Vector3.zero;
            float rotateDirHorizontal = 0.0f;
            float rotateDirVertical = 0.0f;
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            bool isShiftingInput = mouse != null && mouse.rightButton.isPressed;
            if (isShiftingInput)
            {
                if (keyboard != null)
                {
                    moveDirSelf += keyboard[ToKey(_front)].isPressed ? Vector3.forward : Vector3.zero;
                    moveDirSelf += keyboard[ToKey(_back)].isPressed ? Vector3.back : Vector3.zero;
                    moveDirSelf += keyboard[ToKey(_left)].isPressed ? Vector3.left : Vector3.zero;
                    moveDirSelf += keyboard[ToKey(_right)].isPressed ? Vector3.right : Vector3.zero;
                    moveDirSelf = moveDirSelf.normalized;
                    moveDirWorld += keyboard[ToKey(_up)].isPressed ? Vector3.up : Vector3.zero;
                    moveDirWorld += keyboard[ToKey(_down)].isPressed ? Vector3.down : Vector3.zero;
                    moveDirWorld = moveDirWorld.normalized;
                }
                rotateDirHorizontal = mouse.delta.x.ReadValue() * _mouseSensitivity;
                rotateDirVertical = (-1) * mouse.delta.y.ReadValue() * _mouseSensitivity;
            }
            Vector3 moveDir = moveDirWorld + _controller.Target.TransformDirection(moveDirSelf);
            bool isSpeedUp = isShiftingInput && keyboard != null && keyboard[ToKey(_speedUp)].isPressed;
            _controller.SetMoveDirection(moveDir.normalized, isSpeedUp);
            _controller.SetRotateDirection(rotateDirHorizontal, rotateDirVertical);
    #else
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
    #endif
        }
    }
}
