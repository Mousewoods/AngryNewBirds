using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;
using System.Linq;

public class Level : MonoBehaviour,IEventHandler {

	enum LevelState { none, beforeBallDestruction,afterBallDestruction};

	LevelState m_LevelState;

	[Header("Balls")]
	[SerializeField] int m_NBalls;
	public int NBalls { get { return m_NBalls; } }

	[Header("Ball Impulsion")]
	[SerializeField] float m_MinImpulsionForce;
	public float MinImpulsionForce { get { return m_MinImpulsionForce; } }
	[SerializeField] float m_MaxImpulsionForce;
	public float MaxImpulsionForce { get { return m_MaxImpulsionForce; } }
	[Header("EnemyMaterial")]
	[SerializeField]Material m_EnemyMaterial;

	List<Enemy> m_Enemies = new List<Enemy>();

	List<LevelMovingItem> m_MovingItems = new List<LevelMovingItem>();
	bool areAllMovingItemsStill
	{
		get
		{
			foreach (var item in m_MovingItems)
				if (item && !item.IsStillSinceDuration) return false;
			return true;
		}
	}

	public void SubscribeEvents()
	{
		EventManager.Instance.AddListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);
		EventManager.Instance.AddListener<BallHasBeenDestroyedEvent>(BallHasBeenDestroyed);
		EventManager.Instance.AddListener<BallHasBeenInstantiatedEvent>(BallHasBeenInstantiated);
	}

	public void UnsubscribeEvents()
	{
		EventManager.Instance.RemoveListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);
		EventManager.Instance.RemoveListener<BallHasBeenDestroyedEvent>(BallHasBeenDestroyed);
		EventManager.Instance.RemoveListener<BallHasBeenInstantiatedEvent>(BallHasBeenInstantiated);
	}

	private void OnDestroy()
	{
		UnsubscribeEvents();
	}

	private void Awake()
	{
		SubscribeEvents();
	}

	private void Start()
	{
		//enemies
		m_Enemies = GetComponentsInChildren<Enemy>().ToList();
		foreach (var item in m_Enemies) {
			item.GetComponent<MeshRenderer> ().material = m_EnemyMaterial;
		}
		
		//moving items
		m_MovingItems = GetComponentsInChildren<LevelMovingItem>().ToList();
		m_LevelState = LevelState.beforeBallDestruction;
	}

	private void Update()
	{
		if(m_LevelState == LevelState.afterBallDestruction)
		{
			if (areAllMovingItemsStill)
			{
				m_LevelState = LevelState.none;

				if (GameManager.Instance.IsPlaying)
					EventManager.Instance.Raise(new AllMovingItemsAreStillEvent());
			}
		}
	}

	void ResetMovingItems()
	{
		foreach (var item in m_MovingItems)
		{
			if (item) item.Reset();
		}
	}

	void BallHasBeenInstantiated(BallHasBeenInstantiatedEvent e)
	{
		m_LevelState = LevelState.beforeBallDestruction;
		ResetMovingItems();
	}

	void BallHasBeenDestroyed(BallHasBeenDestroyedEvent e)
	{
		m_LevelState = LevelState.afterBallDestruction;
	}

	void EnemyHasBeenDestroyed(EnemyHasBeenDestroyedEvent e)
	{
		m_Enemies.RemoveAll(item => item.Equals(null));
		m_Enemies.Remove(e.eEnemy);

		if (m_Enemies.Count == 0)
		{
			if (GameManager.Instance.IsPlaying)
				EventManager.Instance.Raise(new AllEnemiesOfLevelHaveBeenDestroyedEvent());
		}
	}
}
