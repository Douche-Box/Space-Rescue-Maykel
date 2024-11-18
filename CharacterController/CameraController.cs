using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // Basic camera controller

    [SerializeField] private PlayerInput playerInput = null;
    public PlayerInput PlayerInput => playerInput;

    [SerializeField] PlayerController playerController;

    [SerializeField] float _isCamera;
    [SerializeField] float _isZoom;

    [SerializeField] Transform _target;
    [SerializeField] Transform _orientation;

    [SerializeField] float _minHeightAbovePlayer = 10.0f;
    [SerializeField] float heightAbovePlayer = 5.0f;
    [SerializeField] float _maxHeightAbovePlayer = 30.0f;

    [SerializeField] float rotationSpeed = 5.0f;
    [SerializeField] float zoomSpeed = 5.0f;

    [SerializeField] float _playerRotationSpeed;

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    public float minYAngle = 60f;
    public float maxYAngle = 60f;

    private void OnEnable()
    {
        playerInput.actions.FindAction("Camera").started += OnCamera;
        playerInput.actions.FindAction("Camera").performed += OnCamera;
        playerInput.actions.FindAction("Camera").canceled += OnCamera;

        playerInput.actions.FindAction("Zoom").started += OnZoom;
        playerInput.actions.FindAction("Zoom").performed += OnZoom;
        playerInput.actions.FindAction("Zoom").canceled += OnZoom;
    }

    private void OnDisable()
    {
        playerInput.actions.FindAction("Camera").started -= OnCamera;
        playerInput.actions.FindAction("Camera").performed -= OnCamera;
        playerInput.actions.FindAction("Camera").canceled -= OnCamera;

        playerInput.actions.FindAction("Zoom").started -= OnZoom;
        playerInput.actions.FindAction("Zoom").performed -= OnZoom;
        playerInput.actions.FindAction("Zoom").canceled -= OnZoom;
    }

    private void Awake()
    {
        Cursor.visible = false;
    }

    void OnCamera(InputAction.CallbackContext context)
    {
        _isCamera = context.ReadValue<float>();
    }

    void OnZoom(InputAction.CallbackContext context)
    {
        _isZoom = context.ReadValue<float>();
    }

    void Update()
    {
        currentX += _isCamera * rotationSpeed * 10 * Time.deltaTime;
        currentY -= _isCamera * rotationSpeed * 10 * Time.deltaTime;

        heightAbovePlayer += _isZoom * zoomSpeed * 10 * Time.deltaTime;

        heightAbovePlayer = Mathf.Clamp(heightAbovePlayer, _minHeightAbovePlayer, _maxHeightAbovePlayer);

        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        Vector3 cameraForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        _orientation.forward = cameraForward;
    }

    private void LateUpdate()
    {
        Vector3 direction = new Vector3(0, 0, -heightAbovePlayer);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = _target.position + rotation * direction;
        transform.LookAt(_target.position);
    }
}