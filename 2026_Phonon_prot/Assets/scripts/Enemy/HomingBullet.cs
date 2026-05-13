using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("ホーミング設定")]
    public float speed = 3.0f;               // 弾の飛ぶ速度
    public float rotationSpeed = 180.0f;     // 弾がターゲットに振り向く速度（度/秒）
    public float lifeTime = 5.0f;            // 弾が消滅するまでの時間（秒）
    public string targetTag = "Player";      // 追跡するターゲットのタグ

    private Transform target;                // 追跡対象

    void Start()
    {
        // 寿命が来たら破壊する
        Destroy(gameObject, lifeTime);

        // 指定したタグ(Player)を持つオブジェクトをシーン内から探す
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
        {
            target = targetObj.transform;
        }
    }

    void Update()
    {
        // ターゲットが存在する場合は向きを調整する
        if (target != null)
        {
            // ターゲットへの方向ベクトルを計算
            Vector3 direction = target.position - transform.position;

            // 2Dの場合、Atan2で角度（ラジアン）を求め、度数法（Deg）に変換
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 現在の角度からターゲットの角度へ、rotationSpeedの速さで滑らかに回転させる
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 常に自身の右方向（向いている方向）に前進し続ける
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

}
