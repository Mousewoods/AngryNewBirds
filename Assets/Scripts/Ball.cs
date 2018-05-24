using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class Ball : MonoBehaviour {

	Transform m_Transform;
	Rigidbody m_Rigidbody;

	[Header("Catapult")]
	[SerializeField] float m_CatapultRadius;
	[SerializeField] float m_CatapultMinAngle;
	[SerializeField] float m_CatapultMaxAngle;

	[Header("Arrow")]
	[SerializeField] Transform m_Arrow;
	[SerializeField] Transform m_ResizableArrow;
	[SerializeField] float m_MinArrowExtension;
	[SerializeField] float m_MaxArrowExtension;
					 
	[Header("Gfx")]
	[SerializeField] MeshRenderer m_MeshRenderer;
	[SerializeField] Material m_UnlockedMaterial;
	[SerializeField] Material m_LockedMaterial;

	[Header("Life Duration")]
	[SerializeField] float m_LifeDurationWhenFlying;
	[SerializeField] float m_LifeDurationWhenHit;

    [Header("Beam Reaction Coeff")]
    [SerializeField] float m_BeamReactionCoeff;

	bool m_BallLocked = false;
	bool m_IsFlying = false;

	float minImpulsionForce { get { return LevelsManager.Instance?LevelsManager.Instance.CurrentLevelBallMinImpulsionForce:0; } }
	float maxImpulsionForce { get { return LevelsManager.Instance ? LevelsManager.Instance.CurrentLevelBallMaxImpulsionForce:0; } }

    bool m_AlreadyHit = false;

	private void Awake()
	{
		m_Transform = GetComponent<Transform>();
		m_Rigidbody = GetComponent<Rigidbody>();

		if (!m_MeshRenderer)
			m_MeshRenderer = GetComponentInChildren<MeshRenderer>();
	}
	// Use this for initialization
	void Start () {
		m_BallLocked = false;
		m_IsFlying = false;
	}

	public void DestroyAndDoNotRaiseEvent()
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}

	IEnumerator WaitDestroyAndRaiseEventCoroutine(float waitDurationUntilDestruction)
	{
		yield return new WaitForSeconds(waitDurationUntilDestruction);
		if (GameManager.Instance.IsPlaying)
			EventManager.Instance.Raise(new BallHasBeenDestroyedEvent());
		StopAllCoroutines();
		Destroy(gameObject);
		yield break;
	}

	void WaitDestroyAndRaiseEvent(float waitDurationUntilDestruction)
	{
		StartCoroutine(WaitDestroyAndRaiseEventCoroutine(waitDurationUntilDestruction));
	}

	void GetCatapultExtensionAndAngle(Vector2 mousePos,out float extension,out float angle)
	{
		//extension
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z));
		Vector3 vect = mouseWorldPos - m_Transform.position;
		extension = Mathf.Clamp(vect.magnitude, 0, m_CatapultRadius);

		//angle
		angle = (Mathf.Atan2(-vect.y, -vect.x)) * Mathf.Rad2Deg + 180f; // entre 0 et 2*pi
		angle = Mathf.Clamp(angle, m_CatapultMinAngle, m_CatapultMaxAngle);
	}

	private void Update()
	{
		if (!GameManager.Instance.IsPlaying) return;

		m_MeshRenderer.material =  m_BallLocked ? m_LockedMaterial : m_UnlockedMaterial;
		m_Arrow.gameObject.SetActive(m_BallLocked && !m_IsFlying);

		//arrow size & orientation
		float extension;
		float angle;
		GetCatapultExtensionAndAngle(Input.mousePosition, out extension, out angle);

		SetArrowSize(Mathf.Lerp(m_MinArrowExtension, m_MaxArrowExtension, extension / m_CatapultRadius));
		SetArrowOrientation(angle+180f);
	}

	void SetArrowOrientation(float angle)
	{
		m_Arrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	void SetArrowSize(float size)
	{
		m_ResizableArrow.localScale = Vector3.up+Vector3.forward+ Vector3.right * size;
	}

	// Update is called once per frame
	void LateUpdate () {
		if (!GameManager.Instance.IsPlaying) return;

		if (!m_IsFlying && m_BallLocked && Input.GetMouseButtonUp(0))
		{

            SfxManager.Instance.PlaySfx(Constants.LANCER_SFX);

			float extension;
			float angle;
			GetCatapultExtensionAndAngle(Input.mousePosition, out extension, out angle);
			m_IsFlying = true;

			//m_Rigidbody.useGravity = true;
			m_Rigidbody.isKinematic = false;

			m_Rigidbody.AddForce(m_Arrow.right * Mathf.Lerp(minImpulsionForce, maxImpulsionForce, extension / m_CatapultRadius), ForceMode.Impulse);
			WaitDestroyAndRaiseEvent(m_LifeDurationWhenFlying);

			EventManager.Instance.Raise(new BallHasBeenThrownEvent());
		}
	}

	private void OnMouseOver()
	{
		if (!GameManager.Instance.IsPlaying) return;

		if (!m_IsFlying && !m_BallLocked && Input.GetMouseButtonDown(0))
		{
            SfxManager.Instance.PlaySfx(Constants.PULL_SFX);

			m_BallLocked = true;
		}
		if (!m_IsFlying && m_BallLocked && Input.GetMouseButtonUp(0))
		{
			m_BallLocked = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
        if(GameManager.Instance.IsPlaying && !m_AlreadyHit)
        {
            WaitDestroyAndRaiseEvent(m_LifeDurationWhenHit);
            m_AlreadyHit = true;
        }
        if (GameManager.Instance.IsPlaying && collision.gameObject.CompareTag("Beam"))
        {
            m_Rigidbody.velocity *= m_BeamReactionCoeff;
        }
	}
}
