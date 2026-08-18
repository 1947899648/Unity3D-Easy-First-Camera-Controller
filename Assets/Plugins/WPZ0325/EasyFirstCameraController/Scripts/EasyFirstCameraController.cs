using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace WPZ0325.EasyFirstCameraController
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public partial class EasyFirstCameraController : MonoBehaviour
    {
    #region 【序列化字段】Serialized Fields

        [Header("需要控制的相机")]
        [SerializeField] private Transform _camera;

        [Header("控制键位设置")]
        [SerializeField, FormerlySerializedAs("_fornt")] private KeyCode _front = KeyCode.W;
        [SerializeField] private KeyCode _back      =KeyCode.S;
        [SerializeField] private KeyCode _left      =KeyCode.A;
        [SerializeField] private KeyCode _right     =KeyCode.D;
        [SerializeField] private KeyCode _up        =KeyCode.R;
        [SerializeField] private KeyCode _down      =KeyCode.F;
        [SerializeField] private KeyCode _speedUp   =KeyCode.LeftShift;
        [SerializeField] private string _AxesNameHorizontal = "Mouse X";
        [SerializeField] private string _AxesNameVertical = "Mouse Y";

        [Header("控制参数设置")]
        [SerializeField] bool _isMoveSmooth = false;
        [SerializeField] bool _isRotateSmooth = false;
        [Range(0.0f, 10.0f)] [SerializeField] float _accelerationForMove = 4.5f;
        [Range(0.0f, 20.0f)] [SerializeField] float _accelerationForRotate = 10.0f;
        [Range(0.0f,30.0f)]     [SerializeField] private float _moveSpeed       = 3.0f;
        [Range(0.0f, 360.0f * 3.0f)]   [SerializeField] private float _rotateSpeed     = 360.0f;
        [Range(1.0f, 5.0f)]     [SerializeField] private float _speedUpRate     = 2.0f;
        [Range(45.0f, 85.0f)]   [SerializeField] private float _maxElevation    = 85.0f;
        [Range(45.0f, 85.0f)]   [SerializeField] private float _maxOverlook     = 85.0f;
        [SerializeField] private bool _isEnableCollider = false;
        [Range(0.0f,0.5f)] [SerializeField] private float _colliderSize = 0.2f;
        [SerializeField] bool _isDebug;

        [Header("一些简单的事件：旋转操作不会出发")]
        [FormerlySerializedAs("OnShiftSatrt")] public UnityEvent OnShiftStart;
        public UnityEvent<Transform> OnShifting;
        public UnityEvent OnShiftEnd;

        [Header("自动加载，无需操作，人话：变量可视化区域")]
        [SerializeField] Transform _target;
        [SerializeField] Rigidbody _rigidBody;
        [SerializeField] SphereCollider _collider;
        [SerializeField] Vector3 _moveDirSelf;
        [SerializeField] Vector3 _moveDirWorld;
        [SerializeField] Vector3 _moveDir;
        [SerializeField] float _rotateDirHorizontal;
        [SerializeField] float _rotateDirVertical;
        [SerializeField] bool _isShifting = false;

    #endregion

    #region 【输入处理】Input Handling

        void RespondToInput()
        {
            _moveDirSelf = Vector3.zero;
            _moveDirWorld = Vector3.zero;
            _rotateDirHorizontal = 0.0f;
            _rotateDirVertical = 0.0f;
            _moveDirSelf += Input.GetKey(_front) ? Vector3.forward : Vector3.zero;
            _moveDirSelf += Input.GetKey(_back) ? Vector3.back : Vector3.zero;
            _moveDirSelf += Input.GetKey(_left) ? Vector3.left : Vector3.zero;
            _moveDirSelf += Input.GetKey(_right) ? Vector3.right : Vector3.zero;
            _moveDirSelf = _moveDirSelf.normalized;
            _moveDirWorld += Input.GetKey(_up) ? Vector3.up : Vector3.zero;
            _moveDirWorld += Input.GetKey(_down) ? Vector3.down : Vector3.zero;
            _moveDirWorld = _moveDirWorld.normalized;

            _rotateDirHorizontal = Input.GetAxisRaw(_AxesNameHorizontal);
            _rotateDirVertical = (-1) * Input.GetAxisRaw(_AxesNameVertical);

            _moveDir = _moveDirWorld + _target.TransformDirection(_moveDirSelf);
            _moveDir = _moveDir.normalized;

            if (_moveDirSelf.magnitude >= float.Epsilon || _moveDirWorld.magnitude > float.Epsilon)
            {
                if (_isShifting == false)
                {
                    //Debug.Log("OnShiftStart");
                    MyDebug(nameof(OnShiftStart));
                    OnShiftStart?.Invoke();
                    _isShifting = true;
                }
            }
            else
            {
                if (_isShifting == true)
                {
                    //Debug.Log("OnShiftEnd");
                    MyDebug(nameof(OnShiftEnd));
                    OnShiftEnd?.Invoke();
                    _isShifting = false;
                }
            }

            if (_isShifting)
            {
                //Debug.Log("OnShifting");
                MyDebug(nameof(OnShifting));
                OnShifting?.Invoke(_target);
            }

        }

    #endregion

    #region 【移动与碰撞】Movement & Collision

        /// <summary>
        /// 更新空间位置，具有碰撞检测功能，支持高速防穿透
        /// </summary>
        /// <param name="timeStep"></param>
        void UpdateLocation(float timeStep)
        {
            float moveSpeed = _moveSpeed * (Input.GetKey(_speedUp) ? _speedUpRate : 1.0f);

            if (_isEnableCollider)
            {
                float moveLength = moveSpeed * timeStep;
                Ray ray = new Ray(_target.position, _moveDir);
                if (Physics.Raycast(ray, out RaycastHit hit, moveLength))
                {
                    float L = Vector3.Distance(_target.position, hit.point);
                    float safeL = Mathf.Max(L, 0.02f);
                    float lerpValue = (safeL - 0.01f) / safeL;
                    Vector3 stopPoint = Vector3.Lerp(_target.position, hit.point, lerpValue);
                    _rigidBody.MovePosition(stopPoint);
                }
                else
                {
                    _rigidBody.MovePosition(_rigidBody.position + _moveDir * moveSpeed * timeStep);
                }
            }
            else
            {
                _target.Translate(_moveDir * moveSpeed * timeStep, Space.World);
            }
        }

    #endregion

    #region 【视角控制】View Control

        /// <summary>
        /// 更新视角朝向
        /// </summary>
        /// <param name="timeStep"></param>
        void UpdateView(float timeStep)
        {
            //朝向更新
            float rotateSpeed = _rotateSpeed;
            _target.Rotate(Vector3.up * _rotateDirHorizontal * rotateSpeed * timeStep, Space.World);
            if (IsCanRotateVertical(_rotateDirVertical * rotateSpeed * timeStep))
            {
                _target.Rotate(Vector3.right * _rotateDirVertical * rotateSpeed * timeStep, Space.Self);
            }
        }

        /// <summary>
        /// 判断视角是否处于视角范围之内
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        bool IsCanRotateVertical(float step)
        {
            float currentVerticalValue = _target.transform.eulerAngles.x + step;
            return !(currentVerticalValue > _maxOverlook && currentVerticalValue < (360.0f - _maxElevation));
        }

    #endregion

    #region 【相机跟随】Camera Follow

        void CameraFollowTarget(float timeStep)
        {
            if (_isMoveSmooth)
            {
                float t = Mathf.Clamp01(timeStep * _accelerationForMove);
                _camera.position = Vector3.Lerp(_camera.position,_target.position, t);
            }
            else
            {
                _camera.position = _target.position;
            }

            if (_isRotateSmooth)
            {
                float t = Mathf.Clamp01(timeStep * _accelerationForRotate);
                _camera.rotation = Quaternion.Lerp(_camera.rotation, _target.rotation, t);
            }
            else
            {
                _camera.rotation = _target.rotation;
            }
        }

    #endregion

    #region 【调试】Debug

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void MyDebug(string content)
        {
            if (_isDebug)
            {
                Debug.Log(content);
            }
        }

    #endregion

    #region 【初始化与生命周期】Lifecycle

        private void Reset()
        {
            _target = this.transform;
            _rigidBody = _target.gameObject.GetComponent<Rigidbody>();
            _collider = _target.gameObject.GetComponent<SphereCollider>();
            _rigidBody.useGravity = false;
            _rigidBody.isKinematic = false;
            _collider.radius = _colliderSize;
        }

        void Awake()
        {
            if (_camera == null)
            {
                Debug.LogWarning($"{nameof(EasyFirstCameraController)} Camera is NULL !");
#if UNITY_EDITOR
                DestroyImmediate(this.gameObject);
#else
                Destroy(this.gameObject);
#endif
                return;
            }
        }

        private void OnEnable()
        {
            _target = this.transform;
            _target.transform.position = _camera.position;
            _target.transform.rotation = _camera.rotation;
            _target.transform.SetParent(_camera.parent);
            _rigidBody = _target.gameObject.GetComponent<Rigidbody>();
            _collider = _target.gameObject.GetComponent<SphereCollider>();
            _rigidBody.useGravity = false;
            _rigidBody.isKinematic = false;
            _collider.radius = _colliderSize;
        }

        /// <summary>
        /// ①无论有无碰撞检测，视角旋转处理逻辑一律在Update里执行
        /// ②无碰撞检测（即自由模式，可穿模）。空间位移处理逻辑在Update里执行
        /// ②有碰撞检测（即常规模式，不可穿模）。空间位移处理逻辑在FixedUpdate里执行，可防止视角震动现象。
        /// </summary>
        private void Update()
        {
            if (_target == null || _camera == null || _collider == null || _rigidBody == null)
            {
                return;
            }
            _collider.enabled = _isEnableCollider;
            _collider.radius = _colliderSize;
            if (Input.GetMouseButton(1))
            {
                RespondToInput();
                UpdateView(Time.unscaledDeltaTime);
                if (!_isEnableCollider)
                {
                    UpdateLocation(Time.unscaledDeltaTime);
                }
            }
            else
            {
                _moveDirSelf = Vector3.zero;
                _moveDirWorld = Vector3.zero;
                _rotateDirHorizontal = 0.0f;
                _rotateDirVertical = 0.0f;
            }
            //防止刚体处于漂游模式
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
            //防止相机视角水平倾斜
            Vector3 angles = _target.eulerAngles;
            angles.z = 0.0f;
            _target.eulerAngles = angles;
            angles = _camera.eulerAngles;
            angles.z = 0.0f;
            _camera.eulerAngles = angles;

            CameraFollowTarget(Time.unscaledDeltaTime);
        }

        private void FixedUpdate()
        {
            if (_target == null || _camera == null || _collider == null || _rigidBody == null)
            {
                return;
            }
            if (Input.GetMouseButton(1) && _isEnableCollider)
            {
                UpdateLocation(Time.fixedUnscaledDeltaTime);
            }
            //防止刚体处于漂游模式
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

    #endregion

    #region 【编辑器】Editor

        private void OnDrawGizmos()
        {
            if (_target)
            {
                Vector3 targetPosition = _target.position;
                Gizmos.color = Color.blue;
                Ray ray = new Ray(targetPosition, _moveDir);
                Gizmos.DrawRay(ray);
            }
        }

        private void OnValidate()
        {
            if (System.Object.ReferenceEquals(_target, null))
            {
                _target = this.transform;
            }
            if (System.Object.ReferenceEquals(_collider, null))
            {
                _target.TryGetComponent<SphereCollider>(out _collider);
            }
            if (_collider)
            {
                _collider.enabled = _isEnableCollider;
                _collider.radius = _colliderSize;
            }
            if (System.Object.ReferenceEquals(_rigidBody, null))
            {
                _target.TryGetComponent<Rigidbody>(out _rigidBody);
            }
            if (_rigidBody)
            {
                _rigidBody.useGravity = false;
                _rigidBody.isKinematic = false;
            }
        }

    #endregion
    }
}
