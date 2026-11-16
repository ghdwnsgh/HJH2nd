
using UnityEngine;

public class AltarManager : MonoBehaviour
{
    [Header("제단 목록")]
    [SerializeField] private AltarHealth[] altars;

    [Header("등장할 보스")]
    [SerializeField] private GameObject bossObject;

    private bool bossActivated;


    private void Update()
    {
        if (bossActivated)
            return;

        CheckAltars();
    }

    private void CheckAltars()
    {
        foreach (AltarHealth altar in altars)
        {
            if (altar != null)
                return;
        }

        ActivateBoss();
    }

    private void ActivateBoss()
    {
        bossActivated = true;

        Debug.Log("모든 제단 파괴! 보스 각성!");

        if (bossObject != null)
        {
            MutantBossController boss = bossObject.GetComponent<MutantBossController>();

            if (boss != null)
                boss.Awaken();
        }
    }
}
