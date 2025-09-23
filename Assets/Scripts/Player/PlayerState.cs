using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState instance;
    //참조 

    [HideInInspector]  public Animator anim;
    [HideInInspector]  public SpriteRenderer sprite;
    [HideInInspector]  public Rigidbody2D rb;
    [HideInInspector]  public Collider2D playerCollider;
    [HideInInspector]  public PlayerMove playerMove;
    [HideInInspector]  public Player_atk playeratk;
    [HideInInspector]  public Player_Guard playerGuard;

    [Header("State")]
    public bool isMove;
    public bool isDash;
    public bool isDie;
    public bool isDamage;
    public bool isAttack;
    public bool isGard;
    public bool isWallSlide;
    public bool isWallJump;
    public bool isHeal;
    public bool canMove;

    [Header("Health State")]
    public int maxHP = 5;
    private int currentHP;

    [Header("힐 설정")]
    public float healHoldTime = 0.5f;
    private float healTimer = 0f;
    private bool healedThisPress = false;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }


    

    void Start()
    {
        
    }


    void Update()
    {
        // 회복
        if (Input.GetKey(KeyCode.F))
        {
            isHeal = true;
            if (!healedThisPress && currentHP < maxHP)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healHoldTime)
                {
                    Heal(1);
                    healedThisPress = true;
                }
                else if (Input.GetKeyUp(KeyCode.F))
                {
                    isHeal = false;
                    healTimer = 0f;
                    healedThisPress = false;
                }
            }
        }
    }

    

    public void TakeDamage(int damage = 1)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log("Damage");

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount = 1)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log("Healed! CurrentHP: " + currentHP);
    }

    private void Die()
    {
        canMove = false;
        Debug.Log("플레이어 사망!");
    }
}


