using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Get_Hit_Manager : MonoBehaviour {
    public static int Boss_Monster_Collider_Entry =0 ; // 一堆COLLIDER 用的 只判斷第一次進入的ENTRY
	//public bool is_this_paritition_hit = false;
    public bool Get_hit_In_This_Interval = false;
    public  bool duration_hit_flag = false;
    public bool Hit_Reaction = true;
    public bool isKnock_Up = false, isKnock_Down=false;
    public Vector3 hit_point;
	public Monster_Base connect_enemy;

	[SerializeField] Player_Information pl_info;
    
    private void Update()
    {

        if (Hit_Reaction == true && Get_hit_In_This_Interval == true ) // 受擊反應
        {
            Boss_Monster_Collider_Entry++;
            if (Boss_Monster_Collider_Entry == 1) {

                //  monster.hp -= Atk_Hit_Event.now_skill_atk_damage;
                //    monster.knock_back += now_skill_knock_back;

				//  做完判斷設成FALSE
                Boss_Monster_Collider_Entry = 0;
            
            }


        }
    }
    Vector3 pos = Vector3.zero;

    private void OnCollisionStay(Collision collision)
    {
		if (collision.transform.tag == "Damaged_particle" && Atk_Hit_Event.is_now_duration_skill && Get_hit_In_This_Interval == true && !duration_hit_flag)
        {
			if (gameObject.layer == LayerMask.NameToLayer("Partition"))
			{
				if(Particle_Hit_Event.count >= 1 && Particle_Hit_Event._first_partition == this)
				{
					ContactPoint contact = collision.contacts[0];
					//print("hit_ by particle _and do particle" + collision.gameObject.name);
					duration_hit_flag = true;
					pos = contact.point;
					Monster_Take_DMG();
					//-----------------------------------------------------------------------------------------
					Player_Hit_Manager();
					//-----------------------------------------------------------------------------------------
					//collision.contact
					Hit_Reaction = false;
				}

			}
			else
			{
				ContactPoint contact = collision.contacts[0];
				//print("hit_ by particle _and do particle" + collision.gameObject.name);
				duration_hit_flag = true;
				pos = contact.point;
				Monster_Take_DMG();
				//-----------------------------------------------------------------------------------------
				Player_Hit_Manager();
				//-----------------------------------------------------------------------------------------
				//collision.contact
				Hit_Reaction = false;
			}
			
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //print("hit_ by particle _and do particle " + collision.gameObject.name);
        if (collision.transform.tag == "Damaged_particle" && Hit_Reaction == true && Atk_Hit_Event.is_now_duration_skill ==false)
        {

			if (gameObject.layer == LayerMask.NameToLayer("Partition"))
			{

				if (Particle_Hit_Event.count >= 1 && Particle_Hit_Event._first_partition == this)
				{
					ContactPoint contact = collision.contacts[0];
					//print("hit_ by particle _and do particle" + collision.gameObject.name);
					duration_hit_flag = true;
					pos = contact.point;
					Monster_Take_DMG();
					//-----------------------------------------------------------------------------------------
					Player_Hit_Manager();
					//-----------------------------------------------------------------------------------------
					//collision.contact
					Hit_Reaction = false;
				}
			}
			else
			{
				ContactPoint contact = collision.contacts[0];
				pos = contact.point;
				//-----------------------------------------------------------------------------------------
				Player_Hit_Manager();


				Monster_Take_DMG();
				//-----------------------------------------------------------------------------------------
				//collision.contacts
				Hit_Reaction = false;
			}



        }

        }
    public void Player_Hit_Manager()
    {
        bool hit_burst = Atk_Hit_Event.now_skill_burst_switch;
        GameObject hit_anchor = Atk_Hit_Event.now_Skill_anchor_point;

        GameObject tmpg;
        //   if(Atk_Hit_Event.now_skill_burst_switch)
        //     Instantiate(Hit_Point_Manager.hit_efx_list[3], pos, Quaternion.identity);
        if (Atk_Hit_Event.now_skill_hit_type != null)
            for (int i = 0; i < Atk_Hit_Event.now_skill_hit_type.Count; i++)
            {
                float z_axis_random_r = Random.Range(hit_anchor.transform.eulerAngles.z - 15, hit_anchor.transform.eulerAngles.z + 15f);
                Vector3 rotate = new Vector3(hit_anchor.transform.eulerAngles.x, hit_anchor.transform.eulerAngles.y, z_axis_random_r);
                int hit_index = Atk_Hit_Event.now_skill_hit_type[i];
                tmpg = Instantiate(Hit_Point_Manager.hit_efx_list[hit_index], pos, Quaternion.identity);
                if (Atk_Hit_Event.now_sk_ishit_view)
                    tmpg.GetComponent<Hit_Information>().is_view = true;
                tmpg.transform.localEulerAngles = rotate;
                if(Character_State.Now_Support != null)
                {
                    Support_Check();
                }
            }
    }
    public void Support_Check()
    {
        Skill_Base tmp = Character_State.Now_Support;
        int id = tmp.Skill_ID;
        //print("support_check : " + id);
        switch(id)
        {
            case 11:
                HpMp_Manager.intanse.Hp_Manager((int)(Atk_Hit_Event.now_skill_atk_damage*pl_info.now_atk * tmp.Skill_Interval_Group[0].Interval[0].atk_damage));
                break;
            case 12:
           //     Character_State.Special_Skill_Entry = true;
                break;
            case 13:
                Atk_Hit_Event.now_skill_atk_damage = Atk_Hit_Event.now_skill_atk_damage * tmp.Skill_Interval_Group[0].Interval[0].atk_damage ;
                break;
            default:
                break;
        }
    }
	void Monster_Take_DMG()
	{
        if (connect_enemy.isImmune == false)
        {
            connect_enemy.HP -= Atk_Hit_Event.now_skill_atk_damage * Skill_Combo._pl_info.now_atk;
            //	print("now ?? : " + Atk_Hit_Event.now_skill_atk_damage);
            connect_enemy.current_knock_back_value += Atk_Hit_Event.now_skill_knock_back;
        }
	//	print("0.0 " + connect_enemy.current_knock_back_value);
	}
}




