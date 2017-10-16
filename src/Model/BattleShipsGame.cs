﻿using System;
using System.Collections.Generic;

/// <summary>
/// // The BattleShipsGame controls a big part of the game. It will add the two players
/// to the game and make sure that both players ships are all deployed before starting the game.
/// It also allows players to shoot and swap turns between player. It will also check if players 
/// are destroyed.
/// </summary>
public class BattleShipsGame
{
    private Dictionary<string,uint> _counters = new Dictionary<string, uint>() { { "shots fired", 0 } };
    private const uint nightmareShotsForFiring = 2;

	/// <summary>
	/// The attack delegate type is used to send notifications of the end of an
	/// attack by a player or the AI.
	/// </summary>
	public delegate void AttackCompletedHandler(object sender, AttackResult result);

	/// <summary>
	/// The AttackCompleted event is raised when an attack has completed.
	/// This is used by the UI to play sound effects etc.
	/// </summary> 
	public event AttackCompletedHandler AttackCompleted;

	private Player[] _players = new Player[2];
	private int _playerIndex = 0;

	/// <summary>
	/// The current player.
	/// This value will switch between the two players as they have their attacks
	/// </summary>
	/// <value>The player.</value>
	public Player Player
	{
		get
		{
			return _players[_playerIndex];
		}
	}

    /// <summary>
    /// Toggles whether nightmare features are enabled
    /// </summary>
    public static bool NightMare
    {
        get;

        set;
    }

	/// <summary>
	/// AddDeployedPlayer adds both players and will make sure
	/// that the AI player deploys all ships
	/// </summary>
	public void AddDeployedPlayer(Player p)
	{
		if (_players[0] == null)
		{
			_players[0] = p;
		}
		else if (_players[1] == null)
		{
			_players[1] = p;
			CompleteDeployment();
		}
		else
		{
			throw new ApplicationException("You cannot add another player, the game already has two players.");
		}
	}

	/// <summary>
	/// Assigns each player the other's grid as the enemy grid. This allows each player
	/// to examine the details visible on the other's sea grid.
	/// </summary>
	private void CompleteDeployment()
	{
		_players[0].Enemy = new SeaGridAdapter(_players[1].PlayerGrid);
		_players[1].Enemy = new SeaGridAdapter(_players[0].PlayerGrid);
	}

	public void ResetDeployment(Player p, Player p2)
	{
		_players[0] = p2;
		_players[1] = p;
		_players[0].Enemy = new SeaGridAdapter(_players[1].PlayerGrid);
		_players[1].Enemy = new SeaGridAdapter(_players[0].PlayerGrid);
	}

	/// <summary>
	/// Shoot will swap between players and check if a player has been killed.
	/// It also allows the current player to hit on the enemygrid.
	/// </summary>
	/// <returns>returns the result of the attack</returns>
	/// <param name="row">the row fired upon</param>
	/// <param name="col">the column fired upon</param>
	public AttackResult Shoot(int row, int col)
	{
		AttackResult newAttack = null;
		int otherPlayer = (_playerIndex + 1) % 2;

		newAttack = Player.Shoot(row, col);

		// Will exit the game when all players ships are destroyed
		if (_players[otherPlayer].IsDestroyed)
		{
			newAttack = new AttackResult(ResultOfAttack.GameOver, newAttack.Ship, newAttack.Text, row, col);
		}

		if (AttackCompleted != null)
			AttackCompleted(this, newAttack);

		// Change player if the last hit was a miss
		if (newAttack.Value == ResultOfAttack.Miss)
		{
			_playerIndex = otherPlayer;
		}

        if (NightMare)
        {
            if (newAttack.Value != ResultOfAttack.ShotAlready)
            {
                _counters["shots fired"] += 1;
            }

            if (_counters["shots fired"] == nightmareShotsForFiring)
            {
                //fire randomly
                Random randy = new Random();

                int cycles = 1;

                foreach (Player p in _players)
                {
                    if ((p as AIPlayer) != null)
                    {
                        if ((p as AIPlayer).Difficulty > cycles)
                        {
                            cycles = (p as AIPlayer).Difficulty;
                        }
                    }
                }

                int i = 0;
                while (i < cycles)
                {
                    int rowSub = randy.Next(_players[0].PlayerGrid.Height);//Assumes all grids are of equal size [width, height]
                    int colSub = randy.Next(_players[0].PlayerGrid.Width);

                    foreach (Player p in _players)
                    {
                        p.Shoot(rowSub, colSub);
                    }

                    i += 1;
                }

                _counters["shots fired"] = 0;
            }
        }

		return newAttack;
	}
}
