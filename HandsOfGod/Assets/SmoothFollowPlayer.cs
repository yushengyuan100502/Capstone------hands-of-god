using UnityEngine;

public class SmoothFollowPlayer : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public Vector3 offset;

    void Update()
    {
        if (player != null)
        {
            Vector3 targetPos = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }
}