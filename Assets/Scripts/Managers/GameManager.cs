using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public enum GameState { gameMenu, gamePlay,gameNextLevel,gamePause,gameOver,gameVictory}

public class GameManager : Manager<GameManager> {

	#region Time
	void SetTimeScale(float newTimeScale)
	{
		Time.timeScale = newTimeScale;
	}
	#endregion

	#region Game State
	private GameState m_GameState;
	public bool IsPlaying { get { return m_GameState == GameState.gamePlay; } }
	#endregion

	#region Score
	private int m_Score;
	public int Score {
		get { return m_Score; }
		set
		{
			m_Score = value;
			BestScore = Mathf.Max(BestScore, value);
		}
	}

	public int BestScore
	{
		get
		{
			return PlayerPrefs.GetInt("BEST_SCORE", 0);
		}
		set
		{
			PlayerPrefs.SetInt("BEST_SCORE", value);
		}
	}

	void IncrementScore(int increment)
	{
		SetScore(m_Score + increment);
	}

	void SetScore(int score)
	{
		Score = score;
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore,eScore =m_Score,eNLives = m_NBalls,eNEnemiesLeftBeforeVictory = m_NEnemiesLeftBeforeVictory });
	}
	#endregion

	[Header("GameManager")]

	#region Enemies to be destroyed
	[Header("Victory condition")]
	// Victory Condition : a certain number of enemies must be destroyed
	[SerializeField] private int m_NEnemiesToDestroyForVictory;
	private int m_NEnemiesLeftBeforeVictory;
	void DecrementNEnemiesLeftBeforeVictory(int decrement)
	{
		SetNEnemiesLeftBeforeVictory(m_NEnemiesLeftBeforeVictory - decrement);
	}
	void SetNEnemiesLeftBeforeVictory(int nEnemies)
	{
		m_NEnemiesLeftBeforeVictory = nEnemies;
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = m_Score, eNLives = m_NBalls, eNEnemiesLeftBeforeVictory = m_NEnemiesLeftBeforeVictory });
	}
	#endregion

	#region Balls
	[Header("Ball")]
	[SerializeField] private GameObject m_BallPrefab;
	[SerializeField] private Transform m_BallSpawnPoint;
	Ball m_CurrentBall;

	private int m_NBalls;
	public int NBalls { get { return m_NBalls; } }

	void IncrementNBalls(int increment)
	{
		SetNBalls(m_NBalls + increment);
	}

	void DecrementNBalls(int decrement)
	{
		SetNBalls(m_NBalls - decrement);
	}

	void SetNBalls(int nLives)
	{
		m_NBalls = nLives;
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = m_Score, eNLives = m_NBalls, eNEnemiesLeftBeforeVictory = m_NEnemiesLeftBeforeVictory });
	}
	#endregion

	#region Events' subscription
	public override void SubscribeEvents()
	{
		base.SubscribeEvents();

		//MainMenuManager
		EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
		EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
		EventManager.Instance.AddListener<NextLevelButtonClickedEvent>(NextLevelButtonClicked);
		EventManager.Instance.AddListener<ResumeButtonClickedEvent>(ResumeButtonClicked);
		EventManager.Instance.AddListener<EscapeButtonClickedEvent>(EscapeButtonClicked);

		//Enemy
		EventManager.Instance.AddListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);

		//Score Item
		EventManager.Instance.AddListener<ScoreItemEvent>(ScoreHasBeenGained);

		//Level
		EventManager.Instance.AddListener<LevelHasBeenInstantiatedEvent>(LevelHasBeenInstantiated);
		EventManager.Instance.AddListener<AllMovingItemsAreStillEvent>(LevelBecameStillAfterBallDestruction);
		EventManager.Instance.AddListener<AllEnemiesOfLevelHaveBeenDestroyedEvent>(AllEnemiesOfLevelHaveBeenDestroyed);

		//Ball
		EventManager.Instance.AddListener<BallHasBeenThrownEvent>(BallHasBeenThrown);
	}

	public override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();

		//MainMenuManager
		EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
		EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
		EventManager.Instance.RemoveListener<NextLevelButtonClickedEvent>(NextLevelButtonClicked);
		EventManager.Instance.RemoveListener<ResumeButtonClickedEvent>(ResumeButtonClicked);
		EventManager.Instance.RemoveListener<EscapeButtonClickedEvent>(EscapeButtonClicked);

		//Enemy
		EventManager.Instance.RemoveListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);

		//Score Item
		EventManager.Instance.RemoveListener<ScoreItemEvent>(ScoreHasBeenGained);
		
		//Level
		EventManager.Instance.RemoveListener<LevelHasBeenInstantiatedEvent>(LevelHasBeenInstantiated);
		EventManager.Instance.RemoveListener<AllMovingItemsAreStillEvent>(LevelBecameStillAfterBallDestruction);
		EventManager.Instance.RemoveListener<AllEnemiesOfLevelHaveBeenDestroyedEvent>(AllEnemiesOfLevelHaveBeenDestroyed);

		//Ball
		EventManager.Instance.RemoveListener<BallHasBeenThrownEvent>(BallHasBeenThrown);
	}
	#endregion

	#region Manager implementation
	protected override IEnumerator InitCoroutine()
	{
		Menu();
		EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = 0, eNLives = 0, eNEnemiesLeftBeforeVictory = 0});
		yield break;
	}
	#endregion

	#region Game flow & Gameplay
	//Game initialization
	void InitNewGame()
	{
		SetScore(0);
		SetNBalls(0);
		SetNEnemiesLeftBeforeVictory(m_NEnemiesToDestroyForVictory);

		m_GameState = GameState.gameNextLevel; // le game state sera set à play après que le level est instantié
		EventManager.Instance.Raise(new GoToNextLevelEvent());
	}

	//Ball
	void DestroyCurrentBall()
	{
		if(m_CurrentBall)
			m_CurrentBall.DestroyAndDoNotRaiseEvent();
	}

	void SpawnBall()
	{
		DestroyCurrentBall();
		m_CurrentBall = Instantiate(m_BallPrefab, m_BallSpawnPoint.position, Quaternion.identity).GetComponent<Ball>();
		EventManager.Instance.Raise(new BallHasBeenInstantiatedEvent());
	}
	#endregion

	#region Callbacks to events issued by LevelManager
	private void LevelHasBeenInstantiated(LevelHasBeenInstantiatedEvent e)
	{
		DestroyCurrentBall();
		IncrementNBalls(e.eLevel.NBalls);
		SpawnBall();

		SetTimeScale(1);
		m_GameState = GameState.gamePlay;
	}
	#endregion

	#region Callbacks to events issued by Score items
	private void ScoreHasBeenGained(ScoreItemEvent e)
	{
		IncrementScore(e.eScore.Score);
	}
	#endregion

	#region Callbacks to events issued by Enemy
	private void EnemyHasBeenDestroyed(EnemyHasBeenDestroyedEvent e)
	{
		DecrementNEnemiesLeftBeforeVictory(1);

		if (m_NEnemiesLeftBeforeVictory == 0)
		{
			DestroyCurrentBall();
			Victory();
		}
	}
	#endregion

	#region Callbacks to events issued by Ball
	void BallHasBeenThrown(BallHasBeenThrownEvent e)
	{
		DecrementNBalls(1);
	}
	#endregion
	
	#region Callbacks to events issued by Level
	void LevelBecameStillAfterBallDestruction(AllMovingItemsAreStillEvent e)
	{
		if (!IsPlaying) return;

		DestroyCurrentBall();

		if (NBalls > 0)
			SpawnBall();
		else // gameOver
			Over();
	}

	private void AllEnemiesOfLevelHaveBeenDestroyed(AllEnemiesOfLevelHaveBeenDestroyedEvent e)
	{
		Debug.Log("ALL ENEMIES OF THE LEVEL HAVE BEEN DESTROYED");
		if (IsPlaying)
		{
			DestroyCurrentBall();
			m_GameState = GameState.gameNextLevel;
			SetTimeScale(0);
			EventManager.Instance.Raise(new AskToGoToNextLevelEvent());
		}
	}
	#endregion

	#region Callbacks to Events issued by MenuManager
	private void MainMenuButtonClicked(MainMenuButtonClickedEvent e)
	{
		Menu();
	}

	private void PlayButtonClicked(PlayButtonClickedEvent e)
	{
		Play();
	}

	private void NextLevelButtonClicked(NextLevelButtonClickedEvent e)
	{
		EventManager.Instance.Raise(new GoToNextLevelEvent());
	}

	private void ResumeButtonClicked(ResumeButtonClickedEvent e)
	{
		Resume();
	}

	private void EscapeButtonClicked(EscapeButtonClickedEvent e)
	{
		if(IsPlaying)
			Pause();
	}
	#endregion

	#region GameState methods
	private void Menu()
	{
		DestroyCurrentBall();
		SetTimeScale(0);
		m_GameState = GameState.gameMenu;
		MusicLoopsManager.Instance.PlayMusic(Constants.MENU_MUSIC);
		EventManager.Instance.Raise(new GameMenuEvent());
	}

	private void Play()
	{
		m_GameState = GameState.gamePlay;
		MusicLoopsManager.Instance.PlayMusic(Constants.GAMEPLAY_MUSIC);
		EventManager.Instance.Raise(new GamePlayEvent());
		InitNewGame();
	}

	private void Pause()
	{
		SetTimeScale(0);
		m_GameState = GameState.gamePause;
		EventManager.Instance.Raise(new GamePauseEvent());
	}

	private void Resume()
	{
		SetTimeScale(1);
		m_GameState = GameState.gamePlay;
		EventManager.Instance.Raise(new GameResumeEvent());
	}

	private void Over()
	{
		DestroyCurrentBall();
		SetTimeScale(0);
		m_GameState = GameState.gameOver;
		SfxManager.Instance.PlaySfx(Constants.GAMEOVER_SFX);
		EventManager.Instance.Raise(new GameOverEvent());
	}

	private void Victory()
	{
		DestroyCurrentBall();
		SetTimeScale(0);
		m_GameState = GameState.gameVictory;
		SfxManager.Instance.PlaySfx(Constants.VICTOIRE_SFX);
		EventManager.Instance.Raise(new GameVictoryEvent());
	}
	#endregion
}
