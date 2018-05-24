using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

using Random = UnityEngine.Random;

public class Beam : MonoBehaviour, IScore
{
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

	private void OnDestroy()
	{
		if (GameManager.Instance.IsPlaying)
		{
			EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
        SfxManager.Instance.PlaySfx(Constants.EXPLOSION_SFX);

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
}
