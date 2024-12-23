﻿using System;
using System.Diagnostics;

using Toast.Engine;
using Toast.Engine.Entities;
using Toast.Engine.Resources;

namespace Toast.Game;

/// <summary>
/// Used to initialize the game.
/// </summary>
public class Program
{
	[STAThread]
	public static void Main()
	{
		// Makes a new instance of the game and calls its initialize function
		GameProgram game = new GameProgram();
		game.Initialize();
	}
}

/// <summary>
/// The game itself.<br/>
/// This class handles initializing and running everything game-wise, meaning it holds the Update function,
/// important variables such as deltaTime, the current file, and the current scene.
/// </summary>
public class GameProgram
{
	public static GameState currentState = GameState.Active; // The state the game currently is in

	private Player mainPlayer;
	
	/// <summary>
	/// Initialize the game
	/// </summary>
	public void Initialize()
	{
		// Initialize the engine
		EngineProgram.Initialize(); // Call the engine's initialize function
		EngineProgram.OnUpdate += Update; // After the engine's done updating, this class should update
										  // The engine's update function is a lot more focused on, well, engine-wide
										  // prospects, while this class's should be focused more on the game-specific
										  // functionalities
		
		// Initialize everything necessary before the game is actually run
		// DEBUG: Setting up a basic scene to test out certain aspects of what's done
		EngineProgram.currentFile = new WTF();
		
		EntitySpawner<TestNPC> npcSpawner = new EntitySpawner<TestNPC>(new Vector3(0, 5.0f, 0));
		npcSpawner.spawnsEntityOnSpawn = true;
		EntitySpawner<Player> playerSpawner = new EntitySpawner<Player>(Vector3.Zero);
		playerSpawner.spawnsEntityOnSpawn = true;

		TriggerBrush trigger = new TriggerBrush();
		trigger.SetBBox(new BBox(new Vector3(-15), new Vector3(15)));
		trigger.triggerBy = TriggerBy.Player;
		trigger.triggerType = TriggerType.Once;
		trigger.triggerOn = TriggerOn.Trigger;
		trigger.targetEvent = EntityEvent.TakeDamage;
		trigger.targetEntity = npcSpawner.entityToSpawn;
		trigger.fValue = 100;
		
		EngineProgram.currentFile.AddEntity(npcSpawner);
		EngineProgram.currentFile.AddEntity(playerSpawner);
		EngineProgram.currentFile.AddEntity(trigger);

		EngineProgram.currentFile.Save("test.wtf");

		EngineProgram.currentScene = Scene.LoadFromFile(EngineProgram.currentFile);

		npcSpawner.Spawn();
		playerSpawner.Spawn();
		trigger.Spawn();

		Player player = playerSpawner.SpawnEntity();
		TestNPC npc = npcSpawner.SpawnEntity();

		Ray.Trace(player, npc, out object hitObject, RayIgnore.Brushes, [trigger]);
		
		// Start updating the engine
		EngineProgram.Update();
		
		// After of which, we should call the engine shutdown function
		EngineProgram.OnUpdate -= Update; // Unsubscribe ourselves from the engine update function
		EngineProgram.Shutdown(); // Actually call the engine shutdown function
	}

	/// <summary>
	/// Defines things to do every frame the game is run.
	/// </summary>
	private void Update()
	{
		// Things to do when there is a loaded scene
		if (EngineProgram.currentScene != null)
		{
		}

		if ((currentState & GameState.Active) == 0)
		{
			// We shouldn't do anything more after this!
			// Everything below this if-statement will be run only when the game is in an
			// active state
			return;
		}
	}
}

/// <summary>
/// This enum defines the different states the game can be in.
/// </summary>
[Flags]
public enum GameState
{
	Menu = 1, // Used when accessing any sort of menu
	Active = 2, // Rendering, updating, doing everything it should
	Paused = 4, // Paused for any reason, possibly maskable with menu?
}