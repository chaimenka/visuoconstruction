﻿using UnityEngine;
/// <summary>
/// Gamestate, which is called in gameStateManager::Start after Initialization
/// </summary>
public class SettingsMenu : IState
{
    public void Enter()
    {
        Debug.Log("OpenSettingsMenu::Enter()");

        var SubManagers = GameManager.Instance.AttachedSubManagers;
        foreach (SubManager subManager in SubManagers)
        {
            subManager.OnGameStateEntered(this.ToString());
        }
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        Debug.Log("OpenSettingsMenu::Exit()");

        var SubManagers = GameManager.Instance.AttachedSubManagers;
        foreach (SubManager subManager in SubManagers)
        {
            subManager.OnGameStateLeft(this.ToString());
        }
    }
}
