using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("ステータス設定")]
    public int maxHp = 1;
    private int currentHp;

    [Header("ダメージ判定設定")]
    public string playerTag = "Player";
    public float damageSpeedThreshold = 20.0f;
    public int damageAmount = 1;

    [Header("ヒットストップ演出")]
    [SerializeField] private float damageHitStop = 0.05f; // ダメージ時の停止時間
    [SerializeField] private float dieHitStop = 0.15f;     // 撃破時の停止時間

    [Header("撃破時リワード")]
    [SerializeField] private int restoreAmount = 1; // 倒した時に回復する量（インスペクターで調整）

    private GameObject lastHitter;

    void Start()
    {
        currentHp = maxHp;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            lastHitter = collision.gameObject; // プレイヤーを保存しておく

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed >= damageSpeedThreshold)
            {
                if (HitStopManager.Instance != null) HitStopManager.Instance.Stop(damageHitStop);
                TakeDamage(damageAmount);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (lastHitter != null)
        {
            PlayerMovement player = lastHitter.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.RestoreBurstCount(restoreAmount);
            }
        }

        if (HitStopManager.Instance != null) HitStopManager.Instance.Stop(dieHitStop);

        Debug.Log($"{gameObject.name} を倒して {restoreAmount} 回復！");
        Destroy(gameObject);
    }
}