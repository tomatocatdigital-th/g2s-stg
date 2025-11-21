using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField] float speed = 180f;

    void Update()
    {
        transform.Rotate(0, 0, -speed * Time.unscaledDeltaTime);
    }
}