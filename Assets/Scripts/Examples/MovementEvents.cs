using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class MovementEvents : MonoBehaviour
{

    private Animator characterAnim;
    void Start()
    {
        characterAnim = GameObject.FindWithTag("PlayerAnimation").GetComponent<Animator>();
    }
    void OnEnable()
    {
        var movement = GetComponent<CharacterMovement>();
        var direction = GetComponent<MovementDirection>();

        movement.StartedWalking += OnWalkStart;
        //movement.StoppedWalking += OnWalkStop;
        movement.JumpRequested += OnJumpRequest;
        movement.Jumped += OnJump;
        movement.Landed += OnLanding;
        movement.Falling += OnFalling;

        direction.movingRight += OnWalkRightStart;
        direction.movingLeft += OnWalkLeftStart;
        direction.movingUp += OnWalkUpStart;
        direction.movingDown += OnWalkDownStart;
        direction.stopped += OnWalkStop;

    }

    void OnDisable()
    {
        var movement = GetComponent<CharacterMovement>();
        var direction = GetComponent<MovementDirection>();

        movement.StartedWalking -= OnWalkStart;
        //movement.StoppedWalking -= OnWalkStop;
        movement.JumpRequested -= OnJumpRequest;
        movement.Jumped -= OnJump;
        movement.Landed -= OnLanding;
        movement.Falling -= OnFalling;

        direction.movingRight -= OnWalkRightStart;
        direction.movingLeft -= OnWalkLeftStart;
        direction.movingUp -= OnWalkUpStart;
        direction.movingDown -= OnWalkDownStart;
        direction.stopped -= OnWalkStop;
    }

    void OnWalkStart()
    {
        //Debug.Log("Character started walking");
        //characterAnim.Play("Cat_run_right");
    }

    void OnWalkRightStart()
    {
        characterAnim.Play("Cat_run_right");
    }

    void OnWalkLeftStart()
    {
        characterAnim.Play("Cat_run_left");
    }

    void OnWalkUpStart()
    {
        characterAnim.Play("Cat_run_up");
    }

    void OnWalkDownStart()
    {
        characterAnim.Play("Cat_run_down");
    }

    void OnWalkStop()
    {
        //Debug.Log("Character stopped walking");
        characterAnim.Play("Cat_idle");
    }

    void OnJumpRequest()
    {
        Debug.Log("Character wanted to jump");
    }

    void OnJump(int n)
    {
        Debug.Log($"Character did jump number {n}");
    }

    void OnLanding()
    {
        Debug.Log("Character landed");
    }

    void OnFalling()
    {
        Debug.Log("Character falling");
    }
}
