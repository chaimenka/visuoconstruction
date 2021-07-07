﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class End : IState
{
    public void Enter()
    {
        // Call Submanagers
        GameManager.Instance.debugText.text = "End::Enter()";

        Debug.Log("End::Enter()");
        var SubManagers = GameManager.Instance.AttachedSubManagers;
        foreach (SubManager subManager in SubManagers)
        {
            subManager.OnGameStateEntered(this.ToString());
        }

        // Save Data
        DataFile.Save<ApplicationData>(GameManager.Instance.generalSettings, GameManager.Instance.mainFolder, "generalSettings"); 

        // debug
        GameManager.Instance.debugText.text = "General Settings saved.";
        Debug.Log("General Settings saved.");
    }

    /// <summary>
    /// Not called, cause game quits on enter
    /// </summary>
    public void Execute()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Not called, cause game quits on enter
    /// </summary>
    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}