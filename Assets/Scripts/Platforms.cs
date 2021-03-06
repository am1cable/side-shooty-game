﻿using UnityEngine;
using System.Collections;

public class Platforms : MonoBehaviour {

    public bool upDown;
    public bool leftRight;
    public float transDistance;
    public bool damaging;
    public bool disappearing;
    public float startdelay = 0;
    public float safeLength;
    public float warningLength;
    public float dangerLength;
    public int damageGiven;
	public float damageSpeed;
    Vector3 startPos;
    Vector3 endPos;
    float progress = 1f;
    public float moveSpeed = 10f;
    int stage = 0;
    bool dmgWait;
    float startTime = 0;
    Timer damageTimer = new Timer();
	Timer waitTimer = new Timer();
	Timer moveProg = new Timer();

	// Use this for initialization
	void Start () {


        if (damaging && startdelay >= 0.1)
        {
            damaging = false;
            dmgWait = true;
            waitTimer.setTimer(startdelay+safeLength);

        }

        startPos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        damageTimer.setTimer(safeLength);

		if (upDown)
		{
			endPos = new Vector3(startPos.x, (startPos.y + transDistance), startPos.z);			
		}
		if (leftRight)
		{
			endPos = new Vector3((startPos.x + transDistance), startPos.y, startPos.z);
		}
        
        	}
	
	// Update is called once per frame
	void Update () {

        if (dmgWait && waitTimer.Ok())
        {
            damaging = true;
        }

        if (upDown || leftRight)
        {
            lerpMove(); 
        }
        
        if (damaging)
        { 
            if (stage == 0 && damageTimer.Ok())
            {                
                damageTimer.setTimer(warningLength);
                damRun(1);
            }
            else if (stage == 1 && damageTimer.Ok())
            {
                damageTimer.setTimer(dangerLength);
				waitTimer.setTimer(damageSpeed);
                damRun(2);
            }
            else if (stage == 2 && damageTimer.Ok())
            {
                damageTimer.setTimer(safeLength);
                damRun(0);
            }
            
        }

		if (disappearing && stage == 2 && waitTimer.Ok ()) 
		{
				disRun(0);
				waitTimer.sleep();
		}


	}

	void OnTriggerStay(Collider collisionInfo)
    {
		if (disappearing && collisionInfo.gameObject.tag == "Player")
        {
            if (stage == 0 && damageTimer.Ok())
            {
                damageTimer.setTimer(warningLength);
                disRun(1);                
            }
            else if (stage == 1 && damageTimer.Ok())
            {
                damageTimer.setTimer(dangerLength);
                disRun(2);
            }
            else if (stage == 2 && damageTimer.Ok())
            {
                damageTimer.setTimer(safeLength);
                disRun(0);
            }
        }

		if (damaging && stage == 2 && waitTimer.Ok() && collisionInfo.gameObject.tag == "Player") 
		{
			floorDamage(damageGiven);

				}

    }

	void floorDamage (int strength)
	{
		waitTimer.setTimer(damageSpeed);
		var player = GameObject.FindGameObjectWithTag("Player");
		player.GetComponent<Character>().pureDamaged(strength);
		}

    void OnTriggerExit(Collider collisionInfo)
	{
		bool notDone = stage == 0 || stage == 1;
        if (disappearing && collisionInfo.gameObject.tag == "Player" && notDone)
		{
            disRun(0);				
		}
	}

    void colChange (Color colour)
    {
        foreach(MeshRenderer rendin2smallpieces in GetComponentsInChildren<MeshRenderer>())
        {
            rendin2smallpieces.material.color = colour;
        }
    }
    void opaChange(float visNum)
    {
        foreach (MeshRenderer rendin2smallpieces in GetComponentsInChildren<MeshRenderer>())
        {
            Material visibility = new Material(gameObject.GetComponent<Renderer>().material);
            Color vis = visibility.color;
            vis.a = visNum;
            rendin2smallpieces.material.color = vis;
            visibility.color = vis;
            gameObject.GetComponent<Renderer>().material = visibility;

            if (visNum == 0)
            {
                foreach (MeshRenderer rendin2smallpiecessub in GetComponentsInChildren<MeshRenderer>())
                {
                    rendin2smallpiecessub.GetComponent<Renderer>().enabled = false;
                }
            }
            if (visNum == 1)
            {
                foreach (MeshRenderer rendin2smallpiecessub in GetComponentsInChildren<MeshRenderer>())
                {
                    rendin2smallpiecessub.GetComponent<Renderer>().enabled = true;
                }
            }

        }
    }

    void damRun(int callstage)
    {
        stage = callstage;

        if (stage == 0)
        {
            colChange(Color.white);
        }
        else if (stage == 1)
        {

            colChange(Color.yellow);
        }
        else if (stage == 2)
        {
            colChange(Color.red);
        }
        
    }

    void disRun(int callstage)
    {
        stage = callstage;
        
        
        if (stage == 0)
        {
            opaChange(1);
            GetComponent<Collider>().enabled = true;
        }
        else if (stage == 1)
        {
            opaChange(0.5f);
        }
        else if (stage == 2)
        {
            opaChange(0);
            GetComponent<Collider>().enabled = false;
			waitTimer.setTimer(5);

        }
    }

    void lerpMove ()
    {
		progress = moveProg.progress (startTime, moveSpeed);
        transform.position = Vector3.Lerp(startPos, endPos, progress);
		if (progress >= 1)
        {
			progress = 1;
        }

		if (upDown && progress == 1)
		{
			progress = 1 - progress;
			startTime = Time.time;
			float tempy = startPos.y;
			startPos.y = gameObject.transform.position.y;
			endPos.y = tempy;

		}
		else if (leftRight && progress == 1)
		{
			progress = 1 - progress;
			startTime = Time.time;
			float tempx = startPos.x;
			startPos.x = gameObject.transform.position.x;
			endPos.x = tempx;
		}
    }
}
