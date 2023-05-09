using System.Collections.Generic;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using FPS.Scripts.Game.Shared;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace FPS.Scripts.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class ZombieController : EnemyController
    {
        
    }
}