﻿using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public class Control : MonoBehaviour
	
	//I did not make the player movement physics! Maxime did the entire base, and I tweaked a few things that I a) could understand and b) wanted to change.
    //If it looks complicated, it's not me.
	
{
    public bool isControllable = true;
	public Attack BasicBullet;
    Souls equippedSoul;
	//public float Shootrate = 0.5f;
	//private float Pause = 0.0f;
    private Vector3 _view;
    public bool hangYes = false;
	float hangtime = 0.35f;
	float backforce = 0.0f;
	float yforce = 0.0f;
	bool hitback = false;
	float aircontrol = 1.75f; //Change to control speed when in air
	bool _grounded = false;
    bool _defending = false;
	public Vector3 movement = Vector3.zero;
    public static Control mainControl;
    public Animator animator;
    bool isSolid;
    Collision lastHit;

    public Vector3 View
    {
        get
        {
            return _view;
        }
        set
        {
            _view = value;
            animator.SetFloat("direction", _view.x);
        }
    }

    public bool defending
    {
        get
        {
            return _defending;
        }

        set
        {
            if (value)
            {
                if (!_defending)
                {
                    isControllable = false;
                }
            }

            if (!value)
            {
                if (_defending)
                {
                  isControllable = true;
              }
            }
            _defending = value;
        }
    }

    public bool grounded
    {
        get
        {
            return _grounded;
        }
        set
        {
            if (!value)
            {
                transform.parent = null;
                Cam.mainCam.jumpin = true;
            }
            if (value)
            {
                if (!_grounded)
                {
                    Cam.mainCam.jumpin = false;
                    Cam.mainCam.resetView();
                }
            }
            _grounded = value;
        }
    }
	
	void Start()
    {

        isSolid = true;
        View = new Vector3(1f, 0f, 0f);
        equippedSoul = gameObject.GetComponent<Souls>();
        mainControl = this;
    }

    public void Dead(bool dead)
    {
        if (!dead)
        {
            isSolid = true;
        }

        if (dead)
        {
            isSolid = false;
        }
    }

	public void Shoot()
	{
		if (equippedSoul.Energy >= 0.0)
        {
            animator.Play(Animator.StringToHash("Attack"));
            playSound.p.Play(0);
			Vector3 SpawnPoint = transform.position + (View * 1);
            SpawnPoint.y += 1f;
			GameObject swing = Instantiate(BasicBullet.gameObject, SpawnPoint, transform.rotation) as GameObject;
			Attack shooted = swing.GetComponent<Attack>();
			shooted.dir = View;
			shooted.Speed = equippedSoul.Speed;
			shooted.Strength = equippedSoul.Strength;
			shooted.Shooter = gameObject;
			equippedSoul.Energy -= equippedSoul.useEnergy;
			Physics.IgnoreCollision(shooted.GetComponent<Collider>(), GetComponent<Collider>());
		}
		
	}
	
	
	//public bool Shootpause()
	//{
	
	
	//if (Time.time >= Pause)
	//{
	//	Pause = Time.time + Shootrate;
	//	return true;
	//}
	
	//	return false;
	//}	
	
	void Update()
	{
        GetComponent<Rigidbody>().sleepVelocity = 0;
		//Shootrate = equippedSoul.AttSpeed;
		bool ShootNow = Input.GetKeyDown(KeyCode.Space) /*&& Shootpause()*/;

		if (isControllable)
        { 
		    if (ShootNow)
		    {
			    Shoot();
		    }
		   
        }
        bool MoveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool MoveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        bool Jump = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool Hang = Input.GetKey(KeyCode.K);
        defending = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);	
        movement = Vector3.zero;

        if (!isControllable && defending)
        {

            animator.SetBool("blocking", true);

            if (MoveRight)
            {
                View = Vector3.right;
            }
            if (MoveLeft)
            {
                View = Vector3.left;
            }
        }
        if (isControllable)
        {

            animator.SetBool("blocking", false);

		    if (MoveRight)
		    {
			    movement += Vector3.right * (grounded ? 1.0f : aircontrol);
                View = Vector3.right;
                animator.SetFloat("direction", View.x);
                if (grounded == true)
                {
                   // playSound.p.loopPlay(5, true); shitty sfx lol
                    animator.SetBool("running", true);
                }
                
		    }
		    if (MoveLeft)
		    {
			    movement += Vector3.left * (grounded ? 1.0f : aircontrol);
                View = Vector3.left;
                animator.SetFloat("direction", View.x);
                animator.SetBool("running", true); 
                if (grounded == true)
                {
                    // playSound.p.loopPlay(5, true); shitty sfx lol
                    animator.SetBool("running", true);
                }
			
		    }
            if (!MoveLeft && !MoveRight)
            {
                playSound.p.loopPlay(5, false);
                animator.SetFloat("direction", View.x);
                animator.SetBool("running", false);
            }
		    if (Jump && grounded == true)
            {
                playSound.p.Play(2);
			    yforce = 4.5f; //Intial jump force
			    grounded = false;
		    }

            if (grounded == false)
            {
                animator.SetBool("jumping", true);
                animator.SetBool("running", false);
            }
            if (grounded == true)
            {
                animator.SetBool("jumping", false);
            }
        }
            if (Hang && hangYes)
            {
                hangtime = 0.05f;
            }

            if (!Hang)
            {
                hangtime = 0.35f;
            }
          
		movement.y = yforce;
		
		if (hitback) 
		{

            if (backforce <= -1.5f)
            {

                backforce += 0.95f; //Descent speedup rate
            }

            else if (backforce <= 0.0f)
            {
                backforce += 0.25f; //Ascent slowdown rate
            }

			movement.x = backforce; 			
		}

        if (grounded && transform.parent != null)
        {
            TestGround(transform.parent.GetComponent<Collider>());
        }
		
		if (!grounded)
		{
			if (yforce > 0.0f)
			{
				yforce -= 0.35f; //Ascent slowdown rate
			}
			else if (yforce < 0.7f && yforce > -0.7f)
			{
				yforce -= hangtime;
			}
			else if (yforce > -1.5f) //Max slowdown speed
			{
				yforce -= 0.75f; //Descent speedup rate
			}
		}
        if (isSolid && lastHit != null) //this and all things involving isSolid & lastHit are not by maxime which is why they're giving errors :'D
        {
                Physics.IgnoreCollision(lastHit.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>(), false);

        }
	}
	
	void FixedUpdate()
	{
		//Needs to be done in fixed update because rigidbodies digs it
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + (movement * 10.0f * Time.deltaTime));
	}
	
	void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Enemy" && !isSolid)
        {
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>(), true);
            lastHit = collision;
        }

		if (collision.gameObject.tag == "Ground")
		{
            if (collision.contacts.All(x => x.normal == Vector3.down)) // MOAR MAJICKS
            {
                yforce = 0.0f;
            }
        }

        if (collision.gameObject.tag == "Enemy" && defending && isSolid)
        {
            lastHit = collision;
        }

		if (collision.gameObject.tag == "Enemy" && !defending && isSolid)
        {
			BroadcastMessage("GotHit", collision);
			hitback = true;
            backforce = 1f * -View.x;
            lastHit = collision;

		}

        if (collision.gameObject.name == "Bullet(Clone)" && !defending && isSolid)
        {
            if (collision.gameObject.GetComponent<Attack>().Shooter.name == "AngryEnemy")
            {
                BroadcastMessage("GotHit", collision);
                lastHit = collision;
            }
        }

	}

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Ground" && grounded == false)
        {
            if (collision.contacts.All(x => x.normal == Vector3.up))
            {
                yforce = 0.0f;
                grounded = true;
                transform.parent = collision.gameObject.transform;
                if (hitback)
                {
                    hitback = false;
                }
            }
        }
    }
	

	void OnCollisionExit(Collision collision)
	{
        if (collision == null)
        {
            grounded = false;
            transform.parent = null;
        }
		else if (collision.gameObject.tag == "Ground")
		{
            TestGround(collision.collider);
		}
	}

    void TestGround(Collider col)
    {
        RaycastHit rayhit;
        Physics.Raycast(transform.position, Vector3.down, out rayhit, GetComponent<Collider>().bounds.extents.y + 0.4f, 1 << 10 | 1 << 11);
        if (rayhit.collider == null || rayhit.collider != col)
        {
            grounded = false;
            transform.parent = null;
        }
    }
}