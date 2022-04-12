using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Goblin_state_info : Monster_Base
{
	Monster_Define MD;

	private void Start()
	{
		MD = GetComponent<Monster_Define>();
	}
	public override void Hp_detect()
	{
		if (HP <= 0 && alive)
		{
			alive = false;
			MD.State = "acting";
			MD.lookat = false;
			navagent.isStopped = true;
			navagent.velocity = Vector3.zero;
			MD.RotateTowards(MD.player.transform, 50f);
			MD.anim.SetBool("dead", true);
			MD.lookat = false;
			StartCoroutine("DEAD");
		}
		else if (current_knock_back_value >= knock_back_value && alive)
		{
			MD.State = "acting";
			MD.lookat = false;
			navagent.isStopped = true;
			navagent.velocity = Vector3.zero;
			MD.anim.SetBool("knock back", true);
			MD.RotateTowards(MD.player.transform, 50f);
			
			MD.anim.SetBool("chase", false);
			current_knock_back_value = 0;
		}
		
	}
	void Update()
	{
		Hp_detect();
	}
}
