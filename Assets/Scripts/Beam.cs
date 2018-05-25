using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

using Random = UnityEngine.Random;

public class Beam : MonoBehaviour, IEventHandler, IScore
{
    Rigidbody m_Rigidbody;
    Transform m_Transform;

	[Header("Destruction")]
	[SerializeField]
	float m_DestructionForce;

	[Header("Life Duration When Hit")]
	[SerializeField]
	float m_LifeDurationWhenHit;

	[Header("Score")]
	[SerializeField]
	int m_Score;
	public int Score { get { return m_Score; } }

	bool m_AlreadyHit = false;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Transform = GetComponent<Transform>();
        SubscribeEvents();
    }

	private void OnDestroy()
	{
		if (GameManager.Instance.IsPlaying)
		{
			EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
		}
        UnsubscribeEvents();
	}

	private void OnCollisionEnter(Collision collision)
	{
        SfxManager.Instance.PlaySfx(Constants.PAF_SFX);

		if (GameManager.Instance.IsPlaying
			&& !m_AlreadyHit
			&& collision.gameObject.CompareTag("Ball"))
		{
			float deltaTime = Time.deltaTime;
			Vector3 totalForce =  deltaTime==0?Vector3.zero:collision.impulse / deltaTime;
			if (totalForce.magnitude > m_DestructionForce)
			{
				Debug.Log(name + " Collision with " + collision.gameObject.name + "   force = " + totalForce);
				m_AlreadyHit = true;
				Destroy(gameObject);
			}
		}
	}

    void ExplosiveHasBeenDestroyed(ExplosiveHasBeenDestroyedEvent e)
    {
        float distance = Vector3.Distance(e.eCenter, m_Transform.position);
        if (distance <= e.eRadius)
        {
            m_Rigidbody.AddForce((m_Transform.position - e.eCenter).normalized * Mathf.Lerp(e.ePower, 0, distance / e.eRadius), ForceMode.Impulse);
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
