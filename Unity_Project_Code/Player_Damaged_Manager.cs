using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Damaged_Manager : MonoBehaviour {
    public HpMp_Manager player_hp;

    public float Small_dmg_value, Mid_dmg_value, Large_dmg_value;
    public float Hurt_Immune_Time = 0.1f;
    public bool test_mode = false;
    public float test_dmg;
    public float test_knock_back;
    public static Stack<float> Damaged_Stack = new Stack<float>();
    public static Stack<float> Knock_back_val_Stack = new Stack<float>();
    private Animator ani;
    public Player_Information plinfo;
    public int hurt_count = 0;
    public int detect_hurt_count = 0;
    private CharacterController plchar;
    [SerializeField] private Skill_Event_Function skef;
    public int repeat_hurt_count = 0;
    public GameObject Mainmenu;
     bool isrepeat_hurt = false;
    // Use this for initialization
    void Start()
    {
        ani = GetComponent<Animator>();
        plchar = GetComponent<CharacterController>();
        skef = GetComponent<Skill_Event_Function>();
        StartCoroutine("Damaged_Listener");
    }
    public void Start_Coroutine()
    {
        StartCoroutine("Damaged_Listener");
    }
    public  float current_total_take_damaged , current_total_take_knock_back;
    IEnumerator Damaged_Listener()
    {
        current_total_take_knock_back = plinfo.knock_back_value;
        
        while (true)
        {


            //test_1mode-----
            //----------------
            if (Character_State.isHurt == true)
            {
                if (Root_UI_Manager.isteleporting == true)
                    Root_UI_Manager._root_manager.Hurt_By_Enemy();
            
                Character_State.plyCanCtrl = false;
                //print(" player _ get _ hurt");
                Small_dmg_value = plinfo.knock_back_value * 0.7f;
                Mid_dmg_value = plinfo.knock_back_value * 0.5f;
                Large_dmg_value = plinfo.knock_back_value * 0.3f;
                Damaged_Count();
                if (repeat_hurt_count < 5)
                {

                    if (Character_State.isTank == false)
                    {
                        Damaged_Animation();
                    }
                    else
                    {
                        current_total_take_knock_back = plinfo.knock_back_value;
                    }
                    Do_Damaged();
                }

                Character_State.isHurt = false;
            }
            if (repeat_hurt_count == 5)
            {
                isrepeat_hurt = true;

                yield return new WaitForSeconds(0.2f);
                repeat_hurt_count = 0;
                Character_State.plyCanCtrl = true;
                current_total_take_knock_back = plinfo.knock_back_value;
            }
           else  if (repeat_hurt_count == 0 && isrepeat_hurt == true)
            {
                Character_State.plyCanCtrl = true;
                Character_State.isImmun = false;
                Character_State.isHurt = false;
                Character_State.hurt_knock_back = false;
                isrepeat_hurt = false;
                current_total_take_knock_back = plinfo.knock_back_value;

            }
            else if(repeat_hurt_count == 0)
            {
                if (MoveScene.Now_Scene_Name != "MainMenu")
                {
                    Character_State.plyCanCtrl = true;
                }
                if (Character_State.isdodge == false)
                {
                    Character_State.isImmun = false;
                }
                Character_State.isHurt = false;
                isrepeat_hurt = false;
                Character_State.hurt_knock_back = false;
                current_total_take_knock_back = plinfo.knock_back_value;
            }
            yield return null;
        }
    }

    public void Damaged_Count()
    {
        while (Damaged_Stack.Count != 0) // knock back and damaged 是一樣的容量
        {
            if (current_total_take_knock_back <= 0 || repeat_hurt_count == 5)
            {
                //print("is pop");
                Damaged_Stack.Pop();
                Knock_back_val_Stack.Pop();
                current_total_take_damaged = 0;
                current_total_take_knock_back =0;
            }
            else
            {
                float tmp = Damaged_Stack.Pop();
                float tmp2 = Knock_back_val_Stack.Pop();
                current_total_take_damaged += tmp;
                current_total_take_knock_back -= tmp2;
            }
        }
    }

    bool is_air_immun = false;
    bool repeat_hurt_flag = false;
    public void Damaged_Animation()
    {

        
        ani.SetLayerWeight(0, 1);
        skef.Sword_Take("Back");
        Character_State.plyCanCtrl = false;

            if (Character_State.isGrounded)
            {

                if (current_total_take_knock_back <= Large_dmg_value)
                {
                //大退
                skef.Skill_Entry(0);
                ani.Play("Large_dmg_start " + Convert.ToInt32(repeat_hurt_flag));
                Character_State.hurt_knock_back  = true;
                    repeat_hurt_count++;

                }
                else if (current_total_take_knock_back >= Large_dmg_value && current_total_take_knock_back <= Mid_dmg_value)
                {
                //中退
                skef.Skill_Entry(0);
                ani.Play("Mid_dmg " + Convert.ToInt32(repeat_hurt_flag));
                Character_State.hurt_knock_back = true;
                repeat_hurt_count++;
                }
                else if (current_total_take_knock_back > Mid_dmg_value && current_total_take_knock_back <=Small_dmg_value)
            {
                //小退
                skef.Skill_Entry(0);
                ani.Play("Small_dmg " + Convert.ToInt32(repeat_hurt_flag));
                Character_State.hurt_knock_back = true;
                repeat_hurt_count++;
                }
                else
                {
                Character_State.plyCanCtrl = true;
                //repeat_hurt_count = 0;
                Character_State.hurt_knock_back = false;
                repeat_hurt_count++;
                }
            }
            else
            {
            //空中落下
            //  print("hurt on air");
            skef.Skill_Entry(0);
            ani.Play("Large_dmg_start " + Convert.ToInt32(repeat_hurt_flag));
                repeat_hurt_count = 5;
                
            }
            repeat_hurt_flag = !repeat_hurt_flag;
        
        
    
    }
    bool get_hit_onair;
    public void Do_Damaged()
    {
        player_hp.Hp_Manager(-((int)current_total_take_damaged));
       // print("damaged : " + -((int)current_total_take_damaged));
        current_total_take_damaged = 0;
      //  Character_State.plyCanCtrl = true;
    }
    public void Damaged_Exit()
    {
            Character_State.plyCanCtrl = true;
            repeat_hurt_count = 0;
            Character_State.hurt_knock_back = false;
           current_total_take_knock_back = plinfo.knock_back_value;
    }

}
