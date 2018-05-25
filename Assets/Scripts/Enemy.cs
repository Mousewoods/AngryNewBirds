using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IEventHandler, IScore
{

    Rigidbody m_Rigidbody;
    Transform m_Transform;

    [Header("Destruction")]
    [SerializeField]
    float m_DestructionForce;

    [Header("Score")]
    [SerializeField]
    int m_Score;
    public int Score { get { return m_Score; } }

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
            EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
            EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() { eEnemy = this });
        }
        UnsubscribeEvents();
    }
    private void OnTriggerEnter(Collider coll)
    {
        SfxManager.Instance.PlaySfx(Constants.PAF_SFX);

        if (Time.time > m_TimeStartCheckCollision
            && GameManager.Instance.IsPlaying
            && !m_AlreadyHit)
        {

            bool toBeDestroyed = true;
            /*
            if (coll.gameObject.CompareTag("Ball")) toBeDestroyed = true;
            else if (coll.gameObject.CompareTag("Beam"))
            {
                float deltaTime = Time.deltaTime;
                Vector3 totalForce = deltaTime == 0 ? Vector3.zero : coll.impulse / deltaTime;
                if (totalForce.magnitude > m_DestructionForce)
                {
                    toBeDestroyed = true;
                    Debug.Log(name + " Collision with " + coll.gameObject.name + "   force = " + totalForce);
                }
            }
            */
            if (toBeDestroyed)
            {
                m_AlreadyHit = true;
                Destroy(gameObject);
            }
        }
        /*private void OnCollisionEnter(Collision collision)
        {
            if(Time.time> m_TimeStartCheckCollision
                && GameManager.Instance.IsPlaying 
                && !m_AlreadyHit)
            {

                bool toBeDestroyed = false;

                if (collision.gameObject.CompareTag("Ball")) toBeDestroyed = true;
                else if (collision.gameObject.CompareTag("Beam"))
                {
                    float deltaTime = Time.deltaTime;
                    Vector3 totalForce = deltaTime == 0 ? Vector3.zero : collision.impulse / deltaTime;
                    if (totalForce.magnitude > m_DestructionForce)
                    {
                        toBeDestroyed = true;
                        Debug.Log(name + " Collision with " + collision.gameObject.name + "   force = " + totalForce);
                    }
                }
                if (toBeDestroyed)
                {
                    m_AlreadyHit = true;
                    Destroy(gameObject);
                }
            }
            */

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
                                                                                                        //Rémi le 10eu