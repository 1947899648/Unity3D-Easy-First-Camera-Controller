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
        [SerializeField] Vector3 _moveDir;
        [SerializeField] float _rotateDirHorizontal;
        [SerializeField] float _rotateDirVertical;
        [SerializeField] bool _isShifting = false;
        [SerializeField] bool _isSpeedUp = false;

    #endregion

    #region 【公开控制方法】Public Control API

        /// <summary>
        /// 当前控制目标（相机跟随的载体）
        /// </summary>
        public Transform Target => _target;

        /// <summary>
        /// 设置移动方向（世界空间，需归一化）与是否加速
        /// </summary>
        /// <param name="moveDir">世界空间移动方向，传入 Vector3.zero 表示停止移动</param>
        /// <param name="isSpeedUp">是否启用加速（乘以 _speedUpRate）</param>
        public void SetMoveDirection(Vector3 moveDir, bool isSpeedUp)
        {
            _moveDir = moveDir;
            _isSpeedUp = isSpeedUp;

            if (_moveDir.magnitude >= float.Epsilon)
            {
                if (_isShifting == false)
                {
                    MyDebug(nameof(OnShiftStart));
                    OnShiftStart?.Invoke();
                    _isShifting = true;
                }
            }
            else
            {
                if (_isShifting == true)
                {
                    MyDebug(nameof(OnShiftEnd));
                    OnShiftEnd?.Invoke();
                    _isShifting = false;
                }
            }

            if (_isShifting)
            {
                MyDebug(nameof(OnShifting));
                OnShifting?.Invoke(_target);
            }
        }

        /// <summary>
        /// 设置旋转方向（水平/垂直角速度系数）
        /// </summary>
        /// <param name="horizontal">水平旋转系数，正值向右转</param>
        /// <param name="vertical">垂直旋转系数，正值向上看</param>
        public void SetRotateDirection(float horizontal, float vertical)
        {
            _rotateDirHorizontal = horizontal;
            _rotateDirVertical = vertical;
        }

    #endregion

    #region 【移动与碰撞】Movement & Collision

        /// <summary>
        /// 更新空间位置，具有碰撞检测功能，支持高速防穿透
        /// </summary>
        /// <param name="timeStep"></param>
        void UpdateLocation(float timeStep)
        {
            float moveSpeed = _moveSpeed * (_isSpeedUp ? _speedUpRate : 1.0f);

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
            UpdateView(Time.unscaledDeltaTime);
            if (!_isEnableCollider)
            {
                UpdateLocation(Time.unscaledDeltaTime);
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
            if (_isEnableCollider)
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
