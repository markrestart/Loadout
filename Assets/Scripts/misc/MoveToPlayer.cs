using UnityEngine;

public class MoveToPlayer : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private float waitTime;
    [SerializeField]
    private float goalDistance;

    private Transform targetPlayer;
    private float spawnTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnTime = Time.time;
    }

    public void SetPlayer(Transform player){
        targetPlayer = player;
    }

    // Update is called once per frame
    void Update()
    {

        if(targetPlayer != null && Time.time - spawnTime > waitTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, speed * Time.deltaTime);
            if(Vector3.Distance(transform.position, targetPlayer.position) < goalDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}
