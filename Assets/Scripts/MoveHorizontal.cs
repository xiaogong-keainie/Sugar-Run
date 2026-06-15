using UnityEngine;

public class MoveHorizontal : MonoBehaviour
{
    public float distance = 2f;
    public float speed = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}
