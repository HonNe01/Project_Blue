using UnityEngine;

public class RocketTargeting : MonoBehaviour
{
    public float speed = 10f;
    public float rotatespeed = 200f;
    public Transform target;
    void Start()
    {
       // target = FindEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        // 방향 계산
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        direction.Normalize();

        // 회전
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        transform.Rotate(0, 0, -rotateAmount * rotatespeed * Time.deltaTime);

        // 이동
        transform.position += transform.up * speed * Time.deltaTime;

    }

    void FindEnemy() 
    {

    }

    
}
