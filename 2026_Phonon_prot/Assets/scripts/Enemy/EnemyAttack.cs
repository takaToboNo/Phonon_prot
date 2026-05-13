using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("射撃設定")]
    public GameObject bulletPrefab;          // 弾のプレハブ
    public Transform firePoint;              // 弾を発射する位置
    public float fireRate = 1.0f;            // 1秒間に何発撃つか（発射レート）
    private float nextFireTime = 0f;         // 次に撃てる時間

    void Update()
    {
        CheckAndShoot();
    }

    private void CheckAndShoot()
    {
        // 現在の時間が次に撃てる時間を超えていたら発射
        if (Time.time >= nextFireTime)
        {
            Shoot();
            // 次に発射できる時間を設定（1.0f / fireRate で発射間隔を計算）
            nextFireTime = Time.time + (1.0f / fireRate);
        }
    }

    private void Shoot()
    {
        // 弾と発射位置が設定されているか確認
        if (bulletPrefab != null && firePoint != null)
        {
            // 弾を生成（インスタンス化）
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
            if (straightBullet != null)
            {
                // ③ 同じエネミーについている「移動スクリプト(EnemyMovement)」を取得
                EnemyMovement movement = GetComponent<EnemyMovement>();

                if (movement != null)
                {
                    // 移動スクリプトがあれば、その移動方向（moveDirection）を弾にセットする
                    straightBullet.SetDirection(movement.moveDirection);
                }
                else
                {
                    // もし固定砲台などで移動スクリプトがない場合は、FirePointの右方向をセット
                    straightBullet.SetDirection(firePoint.right);
                }
            }
        }
       else
        {
            Debug.LogWarning($"{gameObject.name}: 弾のプレハブ、または発射位置（FirePoint）が設定されていません。");
        }
    }
}
