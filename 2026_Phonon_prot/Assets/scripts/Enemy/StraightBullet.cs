using UnityEngine;

public class StraightBullet : MonoBehaviour
{
    [Header("弾の設定")]
    public float speed = 5.0f;       // 弾の飛ぶ速度
    public float lifeTime = 3.0f;    // 弾が消滅するまでの時間（秒）

    void Start()
    {
        // 生成されてから lifeTime 秒後に自身を破壊する
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 毎フレーム、自身の左方向（2Dの標準的な前方）へ移動する
        // ※3DゲームでZ軸を前方にしている場合は Vector3.forward に変更してください
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.Self);
    }
}
