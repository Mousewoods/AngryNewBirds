using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMovingItem : MonoBehaviour {

	Transform m_Transform;

	[Header("Stillness")]
	[SerializeField] float m_StillnessDuration;
	[SerializeField] float m_StillnessDistance;
	[SerializeField] float m_StillnessAngle;

	Vector3 m_PrevPos;
	Quaternion m_PrevRot;
	//[SerializeField]
	//bool m_MustCountStillnessTime = false;
	float m_TimeStartedToBecomeStill;

	[SerializeField]private bool m_IsStill;
	public bool IsStill { get { return m_IsStill; } }
	[SerializeField] private bool m_IsStillSinceDuration;
	public bool IsStillSinceDuration { get { return m_IsStillSinceDuration; } }



	//public void StartCountingStillnessTime()
	//{
	//	m_MustCountStillnessTime = true;
	//	m_TimeStartedToBecomeStill = Time.time;
	//	m_IsStill = false;
	//}
	//public void StopCountingStillnessTime()
	//{
	//	m_MustCountStillnessTime = false;
	//}

	private void Awake()
	{
		m_Transform = transform;
		Reset();	
	}

	public void Reset()
	{
		m_PrevPos = m_Transform.position;
		m_PrevRot = m_Transform.rotation;
		m_IsStill = false;
		m_IsStillSinceDuration = false;
		m_TimeStartedToBecomeStill = Time.time;
	}

	// Update is called once per frame
	void Update () {

		//		Debug.Log(name + "  is   " + (m_IsStill ? "still" : "NOT still"));

		m_IsStill = Vector3.Distance(m_Transform.position, m_PrevPos) < m_StillnessDistance
			&& Quaternion.Angle(m_Transform.rotation, m_PrevRot) < m_StillnessAngle;

		if (!m_IsStill)
			m_TimeStartedToBecomeStill = Time.time;

		m_IsStillSinceDuration = Time.time - m_TimeStartedToBecomeStill > m_StillnessDuration;

		m_PrevPos = m_Transform.position;
		m_PrevRot = m_Transform.rotation;
	}
}
