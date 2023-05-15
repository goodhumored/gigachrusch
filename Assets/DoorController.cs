using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
    private bool IsClosed = true;
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Toggle()
    {
        IsClosed = !IsClosed;
        if (IsClosed)
        {
            animator.Play(0);
        }
    }
}
