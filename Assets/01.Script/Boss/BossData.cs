using UnityEngine;

[CreateAssetMenu(fileName = "NewBossData", menuName = "Boss/Boss Data"
)]
public class BossData : ScriptableObject
{
    [Header("보스 정보")]
    public string bossName = "오크";
    public float maxHealth = 300f;

    [Header("감지 및 이동")]
    public float detectionRange = 12f;
    public float attackRange = 3f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("공격")]
    public float attackDamage = 20f;
    public float attackCooldown = 2f;
}
