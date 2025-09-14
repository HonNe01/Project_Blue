using UnityEngine;

[System.Serializable]
public class atk
{
    public string StateName;          // 애니메이션 이름
    public float MaxStepTime = 0.8f;     // 콤보 종료시간
    public float ComboTimingStart = 0.3f;  // 다음 콤보로 넘어갈 수 있는 시간
    public float ComboTimingEnd = 0.7f;    // 동일
}

public class Player_atk : MonoBehaviour
{
    [SerializeField] atk[] _steps;

    [SerializeField] float _inputBufferTime = 0.2f;      // 공격 입력시간 보정
    Animator _anim;

    int _currentCombo = -1;
    float _comboStrartTime = -1;
    bool _queuedNextCombo;
    float _lastInputTime;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            _lastInputTime = Time.time;
            StartComboAttack();
        }

        if (_currentCombo >= 0)
            UpdateCombo();
    }

    void StartComboAttack()
    {
        if(_currentCombo < 0)    // 공격을 안했을때 1번부터 시작
        {
            StartCombo(0);
            return;
        }

        var step = _steps[_currentCombo];          
        float elapsed = Time.time - _comboStrartTime;  // 현재 콤보가 시작된지 얼마나 지났는지

        bool isInTiming = elapsed >= step.ComboTimingStart && elapsed <= step.ComboTimingEnd;
        bool withisnBuffer = (Time.time - _lastInputTime) <= _inputBufferTime;
        
        if (isInTiming && withisnBuffer)   // 타이밍 안에 있고 입력버퍼시간 안에 있으면 다음콤보 출력
            _queuedNextCombo = true;
    }

    void StartCombo(int index)
    {
        _currentCombo = Mathf.Clamp(index, 0, _steps.Length - 1);
        _comboStrartTime = Time.time;
        _queuedNextCombo = false;

        _anim.CrossFade(_steps[_currentCombo].StateName, 0.05f);
    }

    void UpdateCombo()
    { 
        var step = _steps[_currentCombo];
        float elapsed = Time.time - _comboStrartTime;

        if(_queuedNextCombo && elapsed >= step.ComboTimingStart)   // 다음콤보가 예약되어있고 타이밍이 되면 다음콤보 실행
        {
            int next = _currentCombo + 1;
            if (next < _steps.Length)
                StartCombo(next);
            else
                ResetCombo();
            return;
        }

        if (elapsed > step.MaxStepTime)   // 콤보시간이 끝나면 콤보 초기화
        {
            ResetCombo();
        }
    }

    void ResetCombo()
    {
        _currentCombo = -1;
        _queuedNextCombo = false;
    }
}


