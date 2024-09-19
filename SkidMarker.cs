using System.Collections;
using UnityEngine;

public class SkidMarker : MonoBehaviour
{
    [SerializeField] private ParticleSystem _BurnoutFX;
    [SerializeField] private TrailRenderer _SkidMark;
    [SerializeField] private LayerMask _FaultLayer;
    private bool _isSkidding;

    private WheelCollider _wheelcollider;
    private WheelHit _hit;

    private bool _lapFault;
    private TimeKeeper _timeKeeper;

    private void Start()
    {
        _hit = new WheelHit();

        _timeKeeper = FindObjectOfType<TimeKeeper>();
        _wheelcollider = GetComponent<WheelCollider>();
    }

    private void Update()
    {
        CheckSlip();

        SetMarkerOriginPos();
    }

    private void CheckSlip()
    {
        if (_wheelcollider.GetGroundHit(out _hit))
        {
            // Check if the values are between certain points if so then the wheels should place skidmarks and emit smoke
            if (_hit.sidewaysSlip > .3 || _hit.sidewaysSlip < -.3 || _hit.forwardSlip > .8 || _hit.forwardSlip < -.8)
            {
                if (!_isSkidding)
                {
                    _BurnoutFX.Play();
                    _SkidMark.emitting = true;
                    _isSkidding = true;
                }
                else
                {
                    var emission = _BurnoutFX.emission;
                    if (_hit.sidewaysSlip < 0 || _hit.forwardSlip < 0)
                    {
                        emission.rateOverTime = 30 + 30 * -_hit.sidewaysSlip;
                    }
                    else
                    {
                        emission.rateOverTime = 30 + 30 * _hit.sidewaysSlip;
                    }
                }
            }
            else
            {
                if (_isSkidding)
                {
                    _BurnoutFX.Stop();
                    _SkidMark.emitting = false;
                    _isSkidding = false;
                }
            }
        }
        else
        {
            if (_isSkidding)
            {
                _BurnoutFX.Stop();
                _SkidMark.emitting = false;
                _isSkidding = false;
            }
        }

        // If the player goes off the track the collider hits a fault layer and the lap is invalid
        if (_hit.collider != null && ((1 << _hit.collider.gameObject.layer) & _FaultLayer) != 0 && !_lapFault)
        {
            _timeKeeper.LapFault(this);
            _lapFault = true;
        }
    }

    // Set the origin point for the skidmarker under the wheel
    private void SetMarkerOriginPos()
    {
        Vector3 _skidPos = _SkidMark.gameObject.transform.parent.transform.position;
        _skidPos = new Vector3(_skidPos.x, .01f, _skidPos.z);
        _SkidMark.gameObject.transform.position = _skidPos;
    }

    public IEnumerator ResetFault()
    {
        yield return new WaitForSeconds(.2f);
        _lapFault = false;
    }
}