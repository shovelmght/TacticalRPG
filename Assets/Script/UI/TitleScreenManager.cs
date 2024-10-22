using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenManager : MonoBehaviour
{

    [SerializeField] private Animator _TitleTextAnimator;
    [SerializeField] private Animator _MainMenuAnimator;
    private static readonly int Hide = Animator.StringToHash("Hide");
    private static readonly int Show = Animator.StringToHash("Show");
    private BattlesTacticInputAction playerInput;
    private bool _DoOnce;

    private void Awake()
    {
        playerInput = new BattlesTacticInputAction();
    }

    void Update()
    {
        if (Input.anyKey && !_DoOnce)
        {
            _DoOnce = true;
            _TitleTextAnimator.SetTrigger(Hide);
            _MainMenuAnimator.SetTrigger(Show);
        }
    }

    private void Start()
    {
        playerInput.BattleTacticIA.NavigationDown.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.NavigationUp.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.NavigationRight.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.NavigationLeft.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.RotateCameraLeft.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.RotateCameraRight.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.Select.performed += _ => ShowMainMenu();
        playerInput.BattleTacticIA.Back.performed += _ => ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        if (!_DoOnce)
        {
            _DoOnce = true;
            _TitleTextAnimator.SetTrigger(Hide);
            _MainMenuAnimator.SetTrigger(Show);
        }
    }

    public void OnEnable()
    {
        Debug.Log("OnEnable called, trying to enable playerInput");
        if (playerInput != null)
        {
            playerInput.Enable();
        }
        else
        {
            Debug.LogError("playerInput is null in OnEnable");
        }
    }
    public void OnDisable()
    {
        playerInput.Disable();
    }
}


