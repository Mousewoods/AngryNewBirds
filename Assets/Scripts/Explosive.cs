using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

using Random = UnityEngine.Random;

public class Explosive : MonoBehaviour, IEventHandler
{

    Rigidbody m_Rigidbody;
    Transform m_Transform;

    [Header("Destruction")]
    [SerializeField]
    float m_DestructionRadius;
    [SerializeField]
    float m_DestructionPower;
    [SerializeField] GameObject m_ExplosionPrefab;

    [Header("Time Start Check Collision")]
    [SerializeField]
    float m_WaitDurationBeforeStartCheckCollision = 1f;
    float m_TimeStartCheckCollision;

    bool m_AlreadyHit = false;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Transform = GetComponent<Transform>();
        SubscribeEvents();
    }

    protected void Start()
    {
        m_TimeStartCheckCollision = Time.time + m_WaitDurationBeforeStartCheckCollision;
    }

    void OnDestroy()
    {
        if (GameManager.Instance.IsPlaying)
        {
            EventManager.Instance.Raise(new ExplosiveHasBeenDestroyedEvent() { eCenter = m_Transform.position, eRadius = m_DestructionRadius, ePower = m_DestructionPower});
            Instantiate(m_ExplosionPrefab, m_Transform.position, Quaternion.identity);
        }
        UnsubscribeEvents();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time > m_TimeStartCheckCollision && GameManager.Instance.IsPlaying && !m_AlreadyHit)
        {
            m_AlreadyHit = true;
            Destroy(gameObject);
        }
    }

    void ExplosiveHasBeenDestroyed(ExplosiveHasBeenDestroyedEvent e)
    {
        if (Vector3.Distance(e.eCenter, m_Transform.position) <= e.eRadius)
        {
            m_AlreadyHit = true;
            Destroy(gameObject);
        }
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<ExplosiveHasBeenDestroyedEvent>(ExplosiveHasBeenDestroyed);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<ExplosiveHasBeenDestroyedEvent>(ExplosiveHasBeenDestroyed);
    }
}
