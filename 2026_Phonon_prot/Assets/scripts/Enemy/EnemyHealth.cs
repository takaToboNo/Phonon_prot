using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("ステータス設定")]
    public int maxHp = 1;                     // 最大HP
    private int currentHp;                    // 現在のHP

    [Header("ダメージ判定設定")]
    public string playerTag = "Player";       // プレイヤーのタグ
    public float damageSpeedThreshold = 20.0f; // ダメージを受ける最低の衝突速度
    public int damageAmount = 1;              // 1回の衝突で受けるダメージ量

    void Start()
    {
        // ゲーム開始時にHPを最大値にセット
        currentHp = maxHp;
    }

    // 他のオブジェクトと衝突した瞬間に呼ばれる処理 (2D用)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ぶつかった相手が指定したタグ（Player）かどうか確認
        if (collision.gameObject.CompareTag(playerTag))
        {
            // 衝突時の「相対速度（ぶつかった勢い）」の大きさを取得
            float impactSpeed = collision.relativeVelocity.magnitude;

            // ぶつかった勢いが、設定した閾値（最低速度）以上ならダメージ処理
            if (impactSpeed >= damageSpeedThreshold)
            {
                TakeDamage(damageAmount);
                // テスト用のログ表示
                Debug.Log($"プレイヤーが激突！ 敵に {damageAmount} のダメージ！ (衝撃速度: {impactSpeed:F1} / 残りHP: {currentHp})");
            }
            else
            {
                // テスト用のログ表示（勢いが足りなかった時）
                Debug.Log($"速度が足りないためノーダメージ (衝撃速度: {impactSpeed:F1})");
            }
        }
    }

    // ダメージを受ける処理
    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        // HPが0以下になったら破壊処理へ
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 破壊時の処理
    private void Die()
    {
        // 必要であれば、ここに爆発エフェクトの生成や効果音を鳴らす処理を書きます
        Debug.Log($"{gameObject.name} が破壊されました！");

        // 自身（エネミー）のGameObjectをシーンから削除
        Destroy(gameObject);
    }
}
