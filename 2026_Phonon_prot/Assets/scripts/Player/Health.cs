using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("HP設定")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private LayerMask damageLayer; // ダメージを受けるレイヤー（EnemyやTrapなど）

    [Header("UI連携")]
    [SerializeField] private Slider hpSlider; // ヒエラルキーのSliderをここにドラッグ＆ドロップ

    private float currentHp;

    void Start()
    {
        currentHp = maxHp;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }
    }

    // 衝突判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 指定したレイヤーに触れたかチェック
        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            TakeDamage(20f); // とりあえず20ダメージ
        }
    }

    // トリガー（Is Triggerのトゲなど）にも対応
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & damageLayer) != 0)
        {
            TakeDamage(20f);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); // 0〜最大値に収める

        // UIに反映
        if (hpSlider != null) hpSlider.value = currentHp;

        // ★演出：ダメージを受けた瞬間にもヒットストップを入れると「痛さ」が出ます
        if (HitStopManager.Instance != null) HitStopManager.Instance.Stop(0.1f);

        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("ゲームオーバー");
        // ここにリロード処理やリスタートの演出を入れる
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}