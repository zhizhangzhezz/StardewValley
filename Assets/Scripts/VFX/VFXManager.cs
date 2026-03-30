using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{
    private WaitForSeconds twoSeconds;
    [SerializeField] private GameObject deciduousLeavesFallingPrefab = null;
    [SerializeField] private GameObject choppingTreeTrunk = null;
    [SerializeField] private GameObject reapingPrefab = null;

    protected override void Awake()
    {
        base.Awake();
        twoSeconds = new WaitForSeconds(2f);
    }

    private void OnEnable()
    {
        EventHandler.HarvestActionEffectEvent += displayHarvestActionEffect;
    }

    private void OnDisable()
    {
        EventHandler.HarvestActionEffectEvent -= displayHarvestActionEffect;
    }

    private IEnumerator DisableHarvestActionEffect(GameObject effectGameObject, WaitForSeconds wait)
    {
        yield return wait;
        effectGameObject.SetActive(false);
    }

    private void displayHarvestActionEffect(Vector3 position, HarvestActionEffect effect)
    {
        switch (effect)
        {
            case HarvestActionEffect.reaping:
                GameObject reapingEffect = PoolManager.Instance.ReuseObject(reapingPrefab, position, Quaternion.identity);
                reapingEffect.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(reapingEffect, twoSeconds));
                break;
            case HarvestActionEffect.deciduousLeavesFalling:
                GameObject leavesFallingEffect = PoolManager.Instance.ReuseObject(deciduousLeavesFallingPrefab, position, Quaternion.identity);
                leavesFallingEffect.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(leavesFallingEffect, twoSeconds));
                break;
            case HarvestActionEffect.choppingTreeTrunk:
                GameObject choppingEffect = PoolManager.Instance.ReuseObject(choppingTreeTrunk, position, Quaternion.identity);
                choppingEffect.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(choppingEffect, twoSeconds));
                break;
            case HarvestActionEffect.none:
                break;
            default:
                break;
        }
    }

}

