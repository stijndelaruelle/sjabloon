using UnityEngine;
using System.Collections;
using Sjabloon;

public class BasicPlayer : MonoBehaviour
{
    [SerializeField]
    private int m_PlayerID;

    [SerializeField]
    private CharacterController2D m_CharacterController;

    [SerializeField]
    private float m_Speed;

    private void Start()
    {
        InitializeControls();
    }

    private void OnDestroy()
    {
    }

    private void InitializeControls()
    {
        //Movement
        InputManager inputManager = InputManager.Instance;
        inputManager.BindAxis("HorizontalAxis_" + m_PlayerID, KeyCode.RightArrow, KeyCode.LeftArrow);
        inputManager.BindAxis("HorizontalAxis_" + m_PlayerID, m_PlayerID, ControllerButtonCode.Right, ControllerButtonCode.Left);
        inputManager.BindAxis("HorizontalAxis_" + m_PlayerID, m_PlayerID, ControllerAxisCode.LeftStickX);

        inputManager.BindAxis("VerticalAxis_" + m_PlayerID, KeyCode.UpArrow, KeyCode.DownArrow);
        inputManager.BindAxis("VerticalAxis_" + m_PlayerID, m_PlayerID, ControllerButtonCode.Up, ControllerButtonCode.Down);
        inputManager.BindAxis("VerticalAxis_" + m_PlayerID, m_PlayerID, ControllerAxisCode.LeftStickY);
    }

    private void Update()
    {
        UpdateMovement();
    }

    public void UpdateMovement()
    {
        //Horizontal
        float horizInput = InputManager.Instance.GetAxis("HorizontalAxis_" + m_PlayerID);
        float vertInput = InputManager.Instance.GetAxis("VerticalAxis_" + m_PlayerID);

        m_CharacterController.Move(horizInput * m_Speed, vertInput * m_Speed);
    }
}
