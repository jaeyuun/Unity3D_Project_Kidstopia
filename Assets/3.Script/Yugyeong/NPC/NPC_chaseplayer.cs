﻿using System.Collections;
using UnityEngine;

public partial class NPC_chaseplayer : NPC_YG //플레이어 따라다니는 NPC
{
    [Header("Chase_player")]
    public GameObject player;
    [SerializeField] private Rigidbody rigid_;
    [SerializeField] private float move_speed = 1;

    override public void Awake()
    {
        base.Awake();
        TryGetComponent(out rigid_);
    }

    override public IEnumerator Find_posttion()
    {
        while (true)
        {
            if (player != null)
            {
                goal = player.transform;
                goal.position = player.transform.position;
            }
            yield return null;
        }
    }
    override public IEnumerator Set_position()
    {
        while (true)
        {
            //Debug.Log(Vector3.Distance(trans.position, goal.position));
            if (player != null)
            {
                goal = player.transform;

                if (Vector3.Distance(trans.position, goal.position) >= 1f)
                {
                    if (Vector3.Distance(trans.position, goal.position) >= 10f)
                    {
                        Debug.Log("Tele");

                        trans.position = goal.position + 3 * Vector3.right;
                    }
                    else
                    {
                        Debug.Log("Walk");
                        ani.SetBool("is_walk", true);
                        Vector3 tmprot = goal.position - transform.position;
                        tmprot.y = 0;
                        tmprot.Normalize();
                        transform.rotation = Quaternion.LookRotation(tmprot);
                        if (can_move)
                        {
                            rigid_.MovePosition(Vector3.Lerp(rigid_.position, goal.position, Time.deltaTime * move_speed));
                        }
                    }
                }
                else
                {
                    ani.SetBool("is_walk", false);
                    rigid_.velocity = Vector3.zero;
                }
            }
            yield return null;
        }
    }
}
