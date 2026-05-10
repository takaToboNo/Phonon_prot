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

    void Start()
    {
        currentHp = maxHp;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed >= damageSpeedThreshold)
            {
                // ★ダメージを受けた瞬間にヒットストップ
                if (HitStopManager.Instance != null)
                {
                    HitStopManager.Instance.Stop(damageHitStop);
                }

                TakeDamage(damageAmount);
                Debug.Log($"プレイヤーが激突！ (衝撃速度: {impactSpeed:F1})");
            }
            else
            {
                Debug.Log($"速度が足りない (衝撃速度: {impactSpeed:F1})");
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
        // ★トドメを刺した瞬間に長めのヒットストップ
        if (HitStopManager.Instance != null)
        {
            HitStopManager.Instance.Stop(dieHitStop);
        }

        Debug.Log($"{gameObject.name} が破壊されました！");
        Destroy(gameObject);
    }
}