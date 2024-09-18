using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _TimeText;
    [SerializeField] private TextMeshProUGUI _BestTimeText;
    [SerializeField] private TextMeshProUGUI _ControlsText;
    private bool _fadeControlsText;
    private float _timer;

    [Header("Values")]
    [SerializeField] private float _Time;
    [SerializeField] private float _BestTime;
    [SerializeField] private bool _TimeStarted;

    [Header("Recording")]
    [SerializeField] private Recording _recording;

    private float _milliseconds, _seconds, _minutes; // variables to store the time in different units

    private bool _lapFault;
    private List <SkidMarker> _skidmarkers = new();

    private bool _halfwayPointReached;

    [SerializeField] private TextMeshProUGUI _StartCountDownText;
    private KeiTruckEngine _engine;
    private float _startCountdown = 4;
    private bool _started;
    private List<float> _bestTimes;

    [SerializeField] private bool _Test = false;

    private void Start()
    {
        _engine = FindObjectOfType<KeiTruckEngine>();
        _engine.enabled = false; // Disable the player to drive
        _BestTime = PlayerPrefs.GetFloat("BestTimeOne");

        if (_BestTime is 0 or 900)
            _BestTime = PlayerPrefs.GetFloat("BestTimeTwo");
        if (_BestTime is 0 or 900)
            _BestTime = PlayerPrefs.GetFloat("BestTimeThree");
        if (_BestTime is 0 or 900)
            _BestTime = PlayerPrefs.GetFloat("BestTimeFour");
        if (_BestTime is 0 or 900)
            _BestTime = PlayerPrefs.GetFloat("BestTimeFive");

        _minutes = (int)(_BestTime / 60f) % 60;
        _seconds = (int)(_BestTime % 60f);
        _milliseconds = (int)(_BestTime * 1000f) % 1000;
        _BestTimeText.text = "Best: " + _minutes.ToString("00") + ":" + _seconds.ToString("00") + ":" + _milliseconds.ToString("00");
        ResetTimers();
        _bestTimes = new List<float>();
        
        if (PlayerPrefs.HasKey("BestTimeOne"))
            _bestTimes.Add(PlayerPrefs.GetFloat("BestTimeOne"));
        else
            _bestTimes.Add(900);
        
        if (PlayerPrefs.HasKey("BestTimeTwo"))
            _bestTimes.Add(PlayerPrefs.GetFloat("BestTimeTwo"));
        else
            _bestTimes.Add(900);
        
        if (PlayerPrefs.HasKey("BestTimeThree"))
            _bestTimes.Add(PlayerPrefs.GetFloat("BestTimeThree"));
        else
            _bestTimes.Add(900);
        
        if (PlayerPrefs.HasKey("BestTimeFour"))
            _bestTimes.Add(PlayerPrefs.GetFloat("BestTimeFour"));
        else
            _bestTimes.Add(900);
        
        if (PlayerPrefs.HasKey("BestTimeFive"))
            _bestTimes.Add(PlayerPrefs.GetFloat("BestTimeFive"));
        else
            _bestTimes.Add(900);
    }

    private void Update()
    {
        DoCountdown();

        DisplayTime();

        FadeOutControlsText();
    }

    // Perform start countdown
    private void DoCountdown()
    {
        if (_startCountdown >= 0 && !_started && !_Test)
        {
            _startCountdown -= Time.deltaTime;
            if (_startCountdown <= 3)
            {
                _StartCountDownText.text = _startCountdown.ToString("0");
            }
            if (_startCountdown <= 0)
            {
                _StartCountDownText.color = Color.yellow;
                _StartCountDownText.text = "GO!";
                _engine.enabled = true; // Let the player drive off when the counter hits 0
                _started = true;
            }
        }

        if (_started)
        {
            if (_startCountdown <= 1)
            {
                // Fade out the text
                _startCountdown += Time.deltaTime * .4f;
                Color32 color = new Color(255f, 255f, 0f, 255f);
                color = Color32.Lerp(new Color32(255, 255, 0, 255), new Color32(255, 255, 255, 0), _startCountdown);
                _StartCountDownText.color = color;
            }
        }
    }

    private void DisplayTime()
    {
        if (_TimeStarted)
        {
            _Time += Time.deltaTime;

            if (_Time >= 3 && !_fadeControlsText)
            {
                _fadeControlsText = true;
            }

            // calculate the time in minutes, seconds, and milliseconds
            _minutes = (int)(_Time / 60f) % 60;
            _seconds = (int)(_Time % 60f);
            _milliseconds = (int)(_Time * 1000f) % 1000;

            // display the time 
            _TimeText.text = "Current: " + _minutes.ToString("00") + ":" + _seconds.ToString("00") + ":" + _milliseconds.ToString("00");
        }
    }

    // Fade out the tutorial text after the player has started their first lap
    private void FadeOutControlsText()
    {
        if (_fadeControlsText)
        {
            if (_timer <= 1) _timer += Time.deltaTime;
            Color32 colour = _ControlsText.color;
            Color32 startColour = _ControlsText.color;
            colour = Color32.Lerp(startColour, new Color32(startColour.r, startColour.g, startColour.b, 0), _timer);
            _ControlsText.color = colour;
            if (_timer >= 1) _fadeControlsText = false;
        }
    }

    // When the player finished and they did not drive an invalid lap check if it is a new record and reset
    private void StopTimerOnFinish()
    {
        if(_recording != null) _recording.StopRecording();

        if (!_lapFault && _halfwayPointReached)
        {
            CheckTimeList(_Time);
        }
        
        if (_BestTime == 0 && !_lapFault && _halfwayPointReached || _Time < _BestTime && !_lapFault && _halfwayPointReached)
        {
            if (_recording != null) _recording.SaveRecording();
            _BestTime = _Time;
            _BestTimeText.text = "Best: " + _minutes.ToString("00") + ":" + _seconds.ToString("00") + ":" + _milliseconds.ToString("00");
        }
        _TimeStarted = false;
    }

    // Check if the new time is better than one in the top 5, if so add and sort it in the list
    private void CheckTimeList(float time)
    {
        bool listChanged = false;
        for (int i = 0; i < _bestTimes.Count; i++)
        {
            if (time < _bestTimes[i])
            {
                _bestTimes.Add(time);
                listChanged = true;
                break;
            }
        }
        _bestTimes.Sort();
        if (listChanged)
            _bestTimes.RemoveAt(5);
        PlayerPrefs.SetFloat("BestTimeOne", _bestTimes[0]);
        PlayerPrefs.SetFloat("BestTimeTwo", _bestTimes[1]);
        PlayerPrefs.SetFloat("BestTimeThree", _bestTimes[2]);
        PlayerPrefs.SetFloat("BestTimeFour", _bestTimes[3]);
        PlayerPrefs.SetFloat("BestTimeFive", _bestTimes[4]);
        PlayerPrefs.Save();
    }
    
    public void ResetTimers()
    {
        _TimeStarted = false;
        if (_recording != null) _recording.StopRecording();
        _Time = 0;
        _minutes = 0;
        _seconds = 0;
        _milliseconds = 0;
        _TimeText.text = "Current: " + _minutes.ToString("00") + ":" + _seconds.ToString("00") + ":" + _milliseconds.ToString("00");
        _halfwayPointReached = false;
        
        if (_lapFault)
        {
            foreach (SkidMarker a in _skidmarkers)
            {
                StartCoroutine(a.ResetFault());
            }
            _TimeText.color = Color.white;
            _lapFault = false;
        }
        
    }

    // If the player went offtrack the lap time is invalid
    public void LapFault(SkidMarker a)
    {
        if (!_skidmarkers.Contains(a)) _skidmarkers.Add(a);
        if (_lapFault) return;
        if (_recording != null) _recording.StopRecording();
        _TimeText.color = Color.red;
        _lapFault = true;
    }

    // Check if the player drove at least half of the lap to eleminate cheating
    public void HalfwayReached()
    {
        if (_TimeStarted) _halfwayPointReached = true;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && _TimeStarted)
        {
            _TimeStarted = false;
            StopTimerOnFinish();
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && !_TimeStarted)
        {
            ResetTimers();
            _TimeStarted = true;
            if (_recording != null)
            {
                _recording.PlayRecording();
                _recording.StartRecording();
            } 
        }
    }
}