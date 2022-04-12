using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster_Define : MonoBehaviour {
	public float chase_speed;
	public float attack_range;
    public NavMeshAgent agent;
    public Animator anim;
    public string State;
    public bool battle = false;
    public GameObject player;
    public bool lookat;
    private float distance;
    public float range_of_random_movement;
    Vector3 randomPOS;
    NavMeshHit navHit;
    private float escape = 0;
    public float escape_time = 7;
    private float between_angle;
    private float walktime = 0;
    public GameObject Health_Bar_Group;
    [Serializable]
    public class Random_Animation
    {
        [Header("Set bool 之string")]
        public string State_Name;
        public bool Look_at;
        [Header("權重 ， 全部+在一起後的幾分之幾")]
        public int Weight;
    }
   
    public Random_Animation[] Atk_action_group;
    public Random_Animation[] Waiting_action_group;

    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        State = "find_path";
    }

    // Update is called once per frame
    void Update()
    {
        if (battle)
        {
            Health_Bar_Group.SetActive(true);
            between_angle = calculate_angle();
            if (lookat)
            {
                RotateTowards(player.transform, 10f);
            }
            distance = Vector3.Distance(transform.position, player.transform.position);
			if (State == "choose action")
			{
				if (Mathf.Abs(between_angle) <= 30)
				{
					if (distance > attack_range)
					{
						agent.SetDestination(player.transform.position);
						agent.isStopped = false;
						anim.SetBool("chase", true);
						State = "chase";
						agent.speed = chase_speed;
					}
					else
					{
						State = "attack_action";
					}
				}
				else
				{
					RotateTowards(player.transform, 6f);
				}
			}
			else if (State == "chase")
			{
				escape = escape + Time.deltaTime;
				agent.SetDestination(player.transform.position);
				if (distance <= attack_range)
				{
					escape = 0;
					stop_nav();
					anim.SetBool("chase", false);
					anim.SetBool("choose battle action", true);
					State = "attack_action";
					agent.speed = 2.5f;
				}

				if (escape >= escape_time)
				{
					stop_nav();
					battle = false;
					GetComponent<Collider>().enabled = true;
					escape = 0;
					anim.SetBool("chase", false);
					anim.CrossFade("發神經", 0.1f);
					agent.speed = 2.5f;
				}
			}
			else if (State == "attack_action")
			{
				stop_nav();
				State = "acting";
				Random_Selected(Atk_action_group);
			}
			else
			{
				stop_nav();
			}
        }
        else
        {
            Health_Bar_Group.SetActive(false);
            if (State == "find_path")
            {
                randomPOS = UnityEngine.Random.insideUnitSphere * range_of_random_movement;
                NavMesh.SamplePosition(transform.position + randomPOS, out navHit, range_of_random_movement, NavMesh.AllAreas);
                agent.SetDestination(navHit.position);
                State = "walk";
                anim.SetBool("walk", true);
            }
            if (State == "walk")
            {
                walktime += Time.deltaTime;
                agent.isStopped = false;
                if (walktime >= 15)
                {
                    State = "find_path";
                    walktime = 0;
                }
                if (agent.remainingDistance <= agent.stoppingDistance && agent.pathPending == false)
                {
                    walktime = 0;
                    anim.SetBool("walk", false);
                    State = "acting";
                    Random_Selected(Waiting_action_group);
                }
            }
        }
    }
    void Random_Selected(Random_Animation[] ra)
    {
        int total_weight_count =0;
        int decided_index=0;
        for (int i = 0; i < ra.Length; i++)
            total_weight_count += ra[i].Weight;
        int random = UnityEngine.Random.Range(0,total_weight_count);
		int _min_counter = 0, _Max_counter = 0;
        for (int i = 0; i < ra.Length; i++)
		{
			if (i != 0)
			{
				_min_counter = _Max_counter;
			}
			_Max_counter += ra[i].Weight;
			if(_min_counter <= random && random < _Max_counter)
			{
				decided_index = i;
				break;
			}
        }
		print(ra[decided_index].State_Name + "," + random);
		if (ra[decided_index].Look_at)
            lookat = true;
        anim.SetBool(ra[decided_index].State_Name, true);
    }

    public void stop_nav()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            transform.LookAt(player.transform.position);
            battle = true;
            anim.SetBool("walk", false);
            anim.CrossFade("發現玩家", 0.1f);
            GetComponent<Collider>().enabled = false;
            stop_nav();
            State = "acting";
        }
    }
    public void RotateTowards(Transform target, float speed)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation = Quaternion.Euler(new Vector3(0f, lookRotation.eulerAngles.y, 0f));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
    }

    float calculate_angle()
    {
        Vector3 rotate_direction = player.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(rotate_direction);
        float angle = transform.eulerAngles.y - rotation.eulerAngles.y;
        angle = angle % 360;
        if (angle > 180)
        {
            angle = angle - 360;
        }
        else if (angle < -180)
        {
            angle = angle + 360;
        }
        return angle;
    }
}
