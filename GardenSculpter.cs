using UnityEngine;
using UnityEngine.EventSystems;

public class GardenSculpter : MonoBehaviour
{
    [SerializeField] private LayerMask _PlaceableLayer;
    [SerializeField] private LayerMask _ObjectLayer;
    [SerializeField] private float _RotateSpeed = 5;
    private Vector3 _lastRot;

    [SerializeField] private Vector3 _ClearanceRange;

    private Vector3 _target;

    private GameObject _currentlyHoldingObj;
    private GameObject _lastSelectedObj;
    private GardenManager _gardenManager;

    private bool _destroyMode;
    private GameObject _destroyable;

    private bool _objectsWithinClearanceRange = false;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        _gardenManager = FindObjectOfType<GardenManager>();
    }

    private void Update()
    {
        if (_currentlyHoldingObj != null)
        {
            _ClearanceRange = _currentlyHoldingObj.GetComponent<BoxCollider>().bounds.size / 2;
            CheckClearanceRange();
            if (_destroyMode) _destroyMode = false;
            if (Input.GetMouseButtonDown(0) && !CheckClearanceRange() && !EventSystem.current.IsPointerOverGameObject())
            {
                _gardenManager.PlaceObject(_currentlyHoldingObj.transform.position, _currentlyHoldingObj.transform.rotation, _lastSelectedObj);
                _currentlyHoldingObj = null;
                GrabObject(_lastSelectedObj);
            }

            if (Input.GetMouseButtonDown(1))
            {
                ClearHand(false);
            }

            if (Input.GetKey(KeyCode.E) && _currentlyHoldingObj != null)
            {
                _lastRot.y += Time.deltaTime * _RotateSpeed;
            }
            else if (Input.GetKey(KeyCode.Q) && _currentlyHoldingObj != null)
            {
                _lastRot.y -= Time.deltaTime * _RotateSpeed;
            }
            if (_currentlyHoldingObj != null) _currentlyHoldingObj.transform.eulerAngles = _lastRot;
        }

        if (_destroyMode) 
        {
            if (Input.GetMouseButtonDown(1))
            {
                EnterDestroyMode();
            }
            if (_destroyable != null && Input.GetMouseButtonDown(0))
            {
                _gardenManager.RemoveObject(_destroyable.transform.position, _destroyable.transform.rotation, _destroyable);
                Destroy(_destroyable);
            }
        }
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, _PlaceableLayer))
        {
            _target = hit.point;
        }

        if (_destroyMode)
        {
            RaycastHit objHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out objHit, 100, _ObjectLayer))
            {
                _destroyable = objHit.collider.gameObject;
            }
            else _destroyable = null;
        }

        if (_currentlyHoldingObj != null)
        {
            _currentlyHoldingObj.transform.position = _target;
        }
    }

    private bool CheckClearanceRange()
    {
        _objectsWithinClearanceRange = false;

        Collider[] hitColliders = Physics.OverlapBox(_target, _ClearanceRange, _currentlyHoldingObj.transform.localRotation, _ObjectLayer);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.layer == 10 && hitColliders[i].gameObject != _currentlyHoldingObj)
            {
                _objectsWithinClearanceRange = true;
            }
            else if (hitColliders[i].gameObject.layer != 10)
            {
                _objectsWithinClearanceRange = false;
            }
        }

        return _objectsWithinClearanceRange;
    }

    public void GrabObject(GameObject obj)
    {
        _currentlyHoldingObj = Instantiate(obj);
        _lastSelectedObj = obj;
    }

    public void EnterDestroyMode()
    {
        ClearHand(false);
        _destroyMode = !_destroyMode;
    }

    public void ClearHand(bool disableDestroymode)
    {
        if (disableDestroymode) _destroyMode = false;
        if (_currentlyHoldingObj != null)
        {
            Destroy(_currentlyHoldingObj);
            _currentlyHoldingObj = null;
        }
    }
}
