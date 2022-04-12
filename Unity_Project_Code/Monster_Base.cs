using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public abstract class Monster_Base : MonoBehaviour {
    public bool isImmune = false;
	public float HP;
    public float Max_Hp;
	public float current_knock_back_value;
	public float knock_back_value;
	public float knock_back_value_ATTACK;
	public float attack_damage;
	public bool alive = true;
    public int Exp;
	public GameObject connect_event_function;
	public NavMeshAgent navagent;
    public SimpleHealthBar healthBar;
    public GameObject Health_bar_group;
	// Use this for initialization
	void Start()
	{
		navagent = GetComponent<NavMeshAgent>();
      

    }
	// Update is called once per frame
	void Update () {
		
	}
	public abstract void Hp_detect();
	IEnumerator DEAD()
	{
		float time = 0;
        Exp_Manager.Set_Exp(Exp);
        yield return new WaitForSeconds(8f);

        StartCoroutine("clear_body");
		yield return 0;
	}
	IEnumerator clear_body()
	{
		float time = 0;
		while (time <= 7)
		{
			navagent.baseOffset -= 0.01F;

			//print("get nave msesh : " + transform.GetComponentInParent<NavMeshAgent>().baseOffset);
			time += Time.deltaTime;
			yield return new WaitForSeconds(0.001f);
		}
		Destroy(gameObject);
		yield return 0;
	}
}
