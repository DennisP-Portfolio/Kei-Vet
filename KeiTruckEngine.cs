using System.Collections;
using TMPro;
using UnityEngine;

public class KeiTruckEngine : MonoBehaviour
{
    [Header("Wheels")]
    [SerializeField] private Wheel[] _Wheels;
    [SerializeField] private TrailRenderer[] _SkidMarkers;
    [SerializeField] private float _Power; 
    [SerializeField] private float _MaxAngle; // max turn angle 

    //Input values
    private float _forward;
    private float _angle;
    private float _brake;

    [Header("Lights")]
    [SerializeField] private Light[] _Headlights;
    [SerializeField] private Light[] _Backlights;
    [SerializeField] private float _BrakeLightIntensity = 1;
    private float _normalBacklightIntensity = .3f;

    [Header("Physics")]
    [SerializeField] private Rigidbody _Rb;
    [SerializeField] private float _LowerCOMAmount; // Center of Mass*

    private bool _respawning;

    [SerializeField] private LayerMask _Ground;

    [Header("Sound")]
    [SerializeField] private AudioSource _EngineSFX;
    [SerializeField] private int _PitchAmp = 20;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _SpeedIndicator;
    [SerializeField] private RectTransform _speedNeedle;

    [SerializeField] private Transform _RespawnPoint;
    private TimeKeeper _timeKeeper;

    private PlayerControls _playerControls;
    private ControllerManager _controllerManager;

    private void Awake()
    {
        _Rb = GetComponent<Rigidbody>();
        _Rb.centerOfMass = new Vector3(_Rb.centerOfMass.x, _Rb.centerOfMass.y - _LowerCOMAmount, _Rb.centerOfMass.z);

        if (_EngineSFX != null) _EngineSFX.Play();

        _timeKeeper = FindObjectOfType<TimeKeeper>();

        _controllerManager = FindObjectOfType<ControllerManager>();
        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    private void Update()
    {
        CheckForEngineInput();

        CheckForBrake();
         
        CheckForRespawn();

        CalculateEnginePitch();

        CalculateSpeed();

        if (_controllerManager == null)
        {
            _controllerManager = FindObjectOfType<ControllerManager>();
        }
    }

    // Calculate the pitch of the noise the engine makes based on the speed
    private void CalculateEnginePitch()
    {
        float currentSpeed = _Rb.velocity.magnitude * 3.6f;
        float pitch = currentSpeed / _Power;

        if (_EngineSFX != null)
        {
            if (pitch * _PitchAmp > 1.1f) _EngineSFX.pitch = 1.1f + pitch * (_PitchAmp / 15);
            else
            {
                _EngineSFX.pitch = 1.1f;
            }
        }
    }

    // Calculate the speed for UI purposes
    private void CalculateSpeed()
    {
        float mag = _Rb.velocity.magnitude;
        float speed = mag * 3.6f;
        _SpeedIndicator.text = $"{(int)speed} km/h";

        _speedNeedle.localRotation = Quaternion.Euler(0f, 0f, -speed);
    }

    // Check for controller or keyboard input and drive the truck
    private void CheckForEngineInput()
    {
        if (!_controllerManager._PlayingWithGamepad)
        {
            _forward = Input.GetAxis("Vertical");
            _angle = Input.GetAxis("Horizontal");
        }
        else
        {
            _forward = _playerControls.Controls.Gas.ReadValue<float>();
            if (_playerControls.Controls.Brake.ReadValue<float>() > .1f) _forward = -_playerControls.Controls.Brake.ReadValue<float>();
            _angle = _playerControls.Controls.Turn.ReadValue<Vector2>().x;
        }

        if (_forward == 0)
        {
            _Rb.drag = .7f;
        }
        else
        {
            _Rb.drag = 0;
        }
    }

    private void CheckForBrake()
    {
        if (Input.GetKey(KeyCode.Space) || _controllerManager._PlayingWithGamepad && _playerControls.Controls.HandBrake.IsPressed())
        {
            foreach (Wheel wheel in _Wheels)
            {
                wheel.Brake(_Power * 2);
                _brake = 1;
            }
        }
        else
        {
            foreach (Wheel wheel in _Wheels)
            {
                wheel.Brake(0);
                _brake = 0;
            }
        }

        if (_forward < 0 || _brake > 0)
        {
            foreach (Light light in _Backlights)
            {
                light.intensity = _BrakeLightIntensity;
            }
        }
        else
        {
            foreach (Light light in _Backlights)
            {
                light.intensity = _normalBacklightIntensity;
            }
        }
    }

    // Check if the player wants(input) to respawn
    private void CheckForRespawn()
    {
        if (Input.GetKeyDown(KeyCode.R) && !_respawning ||
            _controllerManager._PlayingWithGamepad && _playerControls.Controls.Respawn.WasPerformedThisFrame())
        {
            StartCoroutine(Respawn());
            _respawning = true;
        }
    }

    private void FixedUpdate()
    {
        // Update physics data for the wheels
        foreach(Wheel wheel in _Wheels)
        {
            wheel.Accelerate(_forward * _Power);
            wheel.Turn(_angle * _MaxAngle);
        }
    }

    public IEnumerator Respawn()
    {
        bool respawning = true;
        int times = 0;

        // Make the truck static untill it is placed back at the spawnpoint
        _Rb.useGravity = false;
        _Rb.velocity = Vector3.zero;
        _Rb.angularVelocity = Vector3.zero;
        _Rb.constraints = RigidbodyConstraints.FreezeAll;

        yield return new WaitForSeconds(1f);

        // Reset the wheels so the car doesn't drive off on respawn
        foreach (Wheel wheel in _Wheels)
        {
            wheel.Brake(_Power * 10);
            wheel.ResetTorque();
            wheel.GetComponent<WheelCollider>().rotationSpeed = 0;
        }

        // Set the trucks pos and rot multiple times because sometimes it doesn't work?
        while (respawning)
        {
            transform.position = _RespawnPoint.position;
            transform.rotation = _RespawnPoint.rotation;
            times++;
            if (times == 10)
            {
                respawning = false;
            }
            yield return new WaitForSeconds(.01f);
        }

        yield return new WaitForSeconds(.1f);

        foreach (Wheel wheel in _Wheels)
        {
            wheel.Brake(0);
            wheel.ResetTorque();
            wheel.GetComponent<WheelCollider>().rotationSpeed = 0;
        }
        foreach (TrailRenderer skid in _SkidMarkers)
        {
            skid.Clear();
        }

        // Reset the lap timer
        if (_timeKeeper != null) _timeKeeper.ResetTimers();

        // Unlock the truck so it can drive off again
        _Rb.velocity = Vector3.zero;
        _Rb.angularVelocity = Vector3.zero;
        _Rb.useGravity = true;
        _Rb.constraints = RigidbodyConstraints.None;
        _respawning = false;
    }
}
