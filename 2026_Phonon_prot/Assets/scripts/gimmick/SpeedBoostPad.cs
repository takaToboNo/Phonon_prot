using UnityEngine;

public class SpeedBoostPad : MonoBehaviour
{
    [SerializeField] private float boostMultiplier = 1.5f; // 何倍にするか
    [SerializeField] private float minimumBoostSpeed = 30f; // 最低でもこの速度まで上げる

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                HitStopManager.Instance.Stop(0.035f);

                Vector2 currentVelocity = rb.linearVelocity;
                Vector2 boostedVelocity = currentVelocity * boostMultiplier;

                if (boostedVelocity.magnitude < minimumBoostSpeed)
                {
                    boostedVelocity = currentVelocity.normalized * minimumBoostSpeed;
                    AfterImageManager.Instance.StartEmitting();
                }

                rb.linearVelocity = boostedVelocity;
            }
        }
    }
}