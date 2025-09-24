using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    public static PlayerState instance;

    // Player Reference 플레이어 참조
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public SpriteRenderer sprite;

    [HideInInspector] public PlayerMove playerMove;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public PlayerGuard playerGuard;

    [HideInInspector] public CinemachinePositionComposer cinemachineCamera;

    [Header("=== Player State ===")]
    [Header("Move")]
    public int isRight;
    public bool isGround = true;
    public bool canMove = true;
    public bool canDash = true;
    public bool canJump = true;

    [Header("Attack")]
    public bool canAttack = true;
    public bool canGuard = true;
    public bool canSkill = true;

    [Header("")]
    public bool isHeal = false;
    

    [Header("=== Health State ===")]
    public int maxHP = 5;
    private int curHP;
    public int CurHp => curHP;


    [Header("Healing Setting")]
    public float healHoldTime = 0.5f;
    private float healTimer = 0f;
    private bool healPress = false;


    private void Awake()
    {
        // 인스턴스
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 컴포넌트 참조
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();

        // 클래스 참조
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerGuard = GetComponent<PlayerGuard>();
    }

    private void Start()
    {
        curHP = maxHP; 

        Camera vcam = Camera.main;
        cinemachineCamera = vcam.GetComponent<CinemachinePositionComposer>();
    }


    private void Update()
    {
        Healing();
    }

    private void LateUpdate()
    {
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Composition.ScreenPosition.x = 0.1f * isRight;
        }
    }

    private void Healing()
    {
        // 회복
        if (Input.GetKey(KeyCode.F))
        {
            isHeal = true;
            if (!healPress && curHP < maxHP)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healHoldTime)
                {
                    Heal(1);
                    healPress = true;
                }
                else if (Input.GetKeyUp(KeyCode.F))
                {
                    isHeal = false;
                    healTimer = 0f;
                    healPress = false;
                }
            }
        }
    }
    public void Heal(int amount = 1)
    {
        curHP += amount;
        curHP = Mathf.Clamp(curHP, 0, maxHP);
        Debug.Log("[PlayerState] Player Heal! CurrentHP: " + curHP);
    }

    public void TakeDamage(int damage = 1)
    {
        curHP -= damage;
        curHP = Mathf.Clamp(curHP, 0, maxHP);

        Debug.Log("Damage");

        if (curHP <= 0) Die();
    }

    private void Die()
    {
        canMove = false;

        Debug.Log("플레이어 사망!");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환시 카메라 할당
        Camera vcam = Camera.main;
        if (vcam != null)
            cinemachineCamera = vcam.GetComponent<CinemachinePositionComposer>();
    }
}


