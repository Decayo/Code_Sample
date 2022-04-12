using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Dragon_state_info : Monster_Base
{
	bool utrl_skill = false;
	public int count_Unbalance;
	public static bool stop_utrl;
	public float max_hp;
    public float tmp_knock_back_value; 
	public static int  _partition_count;
    
	// Use this for initialization
	void Start () {
		max_hp = HP;
		tmp_knock_back_value = knock_back_value;
	}
	
	// Update is called once per frame
	void Update () {
		Hp_detect();
	}
	public float Get_Current_Dragon_HP()
	{
		return HP;
	}
	public float Get_Max_Dragon_HP()
	{
		return max_hp;
	}
	public override void Hp_detect()
	{
		if (current_knock_back_value >= knock_back_value && alive)
		{
			
			if (count_Unbalance >= 5)
			{
				count_Unbalance = 0;
				GetComponent<dragon>().State = "acting";
				GetComponent<dragon>().anim.SetBool("倒地", true);
				GetComponent<NavMeshAgent>().isStopped = true;
				GetComponent<NavMeshAgent>().velocity = Vector3.zero;
				current_knock_back_value = 0;
			}
			else {
				count_Unbalance++;
				GetComponent<dragon>().State = "acting";
				GetComponent<dragon>().anim.SetBool("失衡", true);
				GetComponent<NavMeshAgent>().isStopped = true;
				GetComponent<NavMeshAgent>().velocity = Vector3.zero;
				current_knock_back_value = 0;
			}
		}
		if (HP <= 0 && alive)
		{
			stop_utrl = true;
			alive = false;
			GetComponent<dragon>().State = "acting";
			GetComponent<NavMeshAgent>().isStopped = true;
			GetComponent<NavMeshAgent>().velocity = Vector3.zero;
			GetComponent<dragon>().anim.SetBool("死亡", true);
            GetComponent<dragon>().battle = false;
            dragon.root_boss_info.visible = false;
        }
		if (HP <= (max_hp / 2) && utrl_skill==false)
		{
			print("utrl");
			knock_back_value = 9999;
			utrl_skill = true;
			GetComponent<dragon>().State = "runnig_utrl_skill";
			GetComponent<dragon>().anim.CrossFade("向前跑",0.1f);
			StartCoroutine(GetComponent<dragon>().count_distance_utrl());
		}
	}
}
