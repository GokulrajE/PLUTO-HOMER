﻿using UnityEngine;
using System.Collections;
using System;



public class BallController : MonoBehaviour
{

    //speed of the ball
    public static float speed = 3.5F;
    

    //the initial direction of the ball
    private Vector2 spawnDir;

    Vector2 preVel;
    //ball's components
    Rigidbody2D rig2D;
    // Use this for initialization

    public AudioClip[] audioClips;
    int rand = 1;
    float threshold = 2;

    
    //[SerializeField] private TrjectoryPrection _projection;
    void Start()
    {
      
        //setting balls Rigidbody 2D
        //_projection = GameObject.Find("TrajProjection").GetComponent<TrjectoryPrection>();
        rig2D = this.gameObject.GetComponent<Rigidbody2D>();

        //generating random number based on possible initial directions
        int rand = UnityEngine.Random.Range(1, 5);

        //setting initial direction
        if (rand == 1)
        {
            spawnDir = new Vector2(-1, 1);
        }
        else if (rand == 2)
        {
            spawnDir = new Vector2(-1, -1);
        }
        else if (rand == 3)
        {
            spawnDir = new Vector2(-1, -1);
        }
        else if (rand == 4)
        {
            spawnDir = new Vector2(-1, 1);
        }

        //moving ball in initial direction and adding speed
        rig2D.velocity = (spawnDir * speed);

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // rig2D.velocity = speed * (rig2D.velocity.normalized);

        preVel = rig2D.velocity;
        //Debug.Log(rig2D.velocity);

        if (rig2D.velocity.magnitude > 0.01f)
        {
            gameData.events = Array.IndexOf(gameData.pongEvents, "moving");
        }


    }
    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();

    }

    public void initVelocity(Vector2 velocity)
    {
        rig2D.velocity = velocity;

    }


    void OnCollisionEnter2D(Collision2D col)
    {
        //calculate angle
        playAudio(0);
        //tag check

        //Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Enemy")
        {

            float y = launchAngle(transform.position,
                                col.transform.position,
                                col.collider.bounds.size.y);

            //y = Mathf.Abs(y) < 0.01 ? -0.3f : y;
            //         Debug.Log(y + " laugh ang");
            // set angle and speed

            Vector2 d = new Vector2(1, y).normalized;
            initVelocity(d * speed);

            gameData.events = Array.IndexOf(gameData.pongEvents, "enemyHit");

            //_projection.SimulateTrajectory( transform.position, d * speed * 1.5F);
        }

        if (col.gameObject.tag == "Player")
        {
            //calculate angle
            // playAudio(0);
            float y = launchAngle(transform.position,
                                col.transform.position,
                                col.collider.bounds.size.y);

            //set angle and speed
            Vector2 d = new Vector2(-1, y).normalized;
            initVelocity(d * speed);
            gameData.events = Array.IndexOf(gameData.pongEvents, "playerHit");
            
            //_projection.SimulateTrajectory( transform.position, d * speed * 1.5F);
        }
        if (col.gameObject.name == "BottomBound")
        {
            if (rig2D.velocity.y == 0)
            {
                rig2D.velocity = new Vector2(rig2D.velocity.x, Mathf.Abs(preVel.y));
               

            }
            gameData.events = Array.IndexOf(gameData.pongEvents, "wallBounce");
            //Debug.Log(preVel);
        }
        if (col.gameObject.name == "TopBound")
        {
            if (rig2D.velocity.y == 0)
            {
                rig2D.velocity = new Vector2(rig2D.velocity.x, -Mathf.Abs(preVel.y));
                

            }
            gameData.events = Array.IndexOf(gameData.pongEvents, "wallBounce");
            //Debug.Log(preVel);
        }
      
    }

    //calculates the angle the ball hits the paddle at
    float launchAngle(Vector2 ballPos, Vector2 paddlePos,
                    float paddleHeight)
    {
        return Mathf.Clamp(0.2f * Mathf.Sign(ballPos.y - paddlePos.y) + (ballPos.y - paddlePos.y) / paddleHeight, -2, 2);
    }


}
