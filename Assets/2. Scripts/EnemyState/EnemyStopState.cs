using Junyoung;
using UnityEngine;

public class EnemyStopState : MonoBehaviour, IEnemyState
{
    private EnemyCtrl m_enemy;

    public void Handle(EnemyCtrl enemy)
    {
        m_enemy = enemy;

        Debug.Log($"Enemy StopState");
    }
}
