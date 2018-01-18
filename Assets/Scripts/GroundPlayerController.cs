﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlayerController : MonoBehaviour {
    private Rigidbody Player;
    private Transform Wheels;
    [Header("Data")]
    public float speed = 400f;
    [Header("Limitations")]
    public float limit = 4.5f;
    public float wheelRotationDegree = 25f;
    public float meshRotationSpeed = 20f;
    public float changingLaneSpeed = 500f;
    public float moveForward = 1f, leftAndRight = 1f;
    public float upAndDown = 0.7f;

    private CollisionManager CollisionChecker;

    void Start () {
        Player = GetComponent<Rigidbody>();
        Wheels = transform.GetChild(1);
        CollisionChecker = GetComponent<CollisionManager>();
    }

    void FixedUpdate() {
        CarMover(); // will move the car l&r and forward
        SpeedManager(); // Manages Speed
        MeshUpdater(); // will update car Wheels and Car body when changing lanes
        //gameObject.GetComponent<CollisionManager>().CollisionDetection(); // is Managing COLLISIONS
    }

    void CarMover() {
        float moveHorizontal = Input.GetAxis("Horizontal") * leftAndRight;
        Vector3 movement = new Vector3(moveHorizontal, upAndDown, moveForward); // get axis will smpoth left and right
        movement.x *= changingLaneSpeed;
        movement.z *= speed;
        Player.velocity = movement * Time.deltaTime; // moving the player and using dt to smooth transition 
        // will limit pos to a certain interval [-limit,limit]
        Player.position = new Vector3(Mathf.Clamp(Player.position.x, -limit, limit), upAndDown, Player.position.z);
    }

    void MeshUpdater() {
        //Rotating the Wheels in relation with speed
        for (int i = 0 ; i < 4 ; i++)
            Wheels.GetChild(i).Rotate(Time.deltaTime * speed * 3, 0, 0);
        // Updating the Body to move left and right or stay idle
        if (Input.GetButton("Horizontal") && CollisionChecker.colliderType != false ) {
            if (Input.GetAxis("Horizontal") > 0) {
                // this will rotate the body to the right (previous value was  x : -1.449f and z : 2.759f)
                if (IsBraking() && speed > 500) // when breaking lean forward while turning right
                    this.gameObject.transform.GetChild(0).localRotation = 
                        Quaternion.RotateTowards(this.gameObject.transform.GetChild(0).localRotation, Quaternion.Euler(2.214f, 0.122f, 1.5f), Time.deltaTime * meshRotationSpeed);
                else // this will just turn right
                    this.gameObject.transform.GetChild(0).localRotation =
                        Quaternion.RotateTowards(this.gameObject.transform.GetChild(0).localRotation, Quaternion.Euler(0f, 0.122f, 1.5f), Time.deltaTime * meshRotationSpeed);
                // this will rotate the entire car to the right
                Player.transform.rotation = 
                    Quaternion.RotateTowards(Player.transform.rotation, Quaternion.Euler(0f, 3f, 0f), Time.deltaTime * meshRotationSpeed);
                // this will rotate the wheels to the right
                for (int i = 0; i < 2; i++)
                    Wheels.GetChild(i).localRotation =
                        Quaternion.RotateTowards(Wheels.GetChild(i).localRotation, Quaternion.Euler(0f, wheelRotationDegree, 0f), Time.deltaTime * meshRotationSpeed);
            }
            else if (Input.GetAxis("Horizontal") < 0) {
                // this will rotate the body to the left (previous value was  x : -1.449f and z : -2.759f)
                if (IsBraking() && speed > 500) // when breaking lean forward while turning left
                    this.gameObject.transform.GetChild(0).localRotation = 
                        Quaternion.RotateTowards(this.gameObject.transform.GetChild(0).localRotation, Quaternion.Euler(2.214f, 0.122f, -1.5f), Time.deltaTime * meshRotationSpeed);
                else // this will just turn left
                    this.gameObject.transform.GetChild(0).localRotation =
                        Quaternion.RotateTowards(this.gameObject.transform.GetChild(0).localRotation, Quaternion.Euler(0f, 0.122f, -1.5f), Time.deltaTime * meshRotationSpeed);
                // this will rotate the entire car to the left
                Player.transform.rotation = 
                    Quaternion.RotateTowards(Player.transform.rotation, Quaternion.Euler(0f, -3f, 0f), Time.deltaTime * meshRotationSpeed);
                // this will rotate the wheels to the left
                for (int i = 0; i < 2; i++)
                    Wheels.GetChild(i).localRotation =
                        Quaternion.RotateTowards(Wheels.GetChild(i).localRotation, Quaternion.Euler(0f, -wheelRotationDegree, 0f), Time.deltaTime * meshRotationSpeed);
            }
        }
        else { // when the body is idle
            if (IsBraking() && speed > 500) // will get car to lean forward when breaking
                this.gameObject.transform.GetChild(0).localRotation = 
                    Quaternion.RotateTowards(this.gameObject.transform.GetChild(0).localRotation, Quaternion.Euler(2.214f, 0f, 0f), Time.deltaTime * meshRotationSpeed * 2);
            else // will get car body to go slightly up as speed increases
                this.gameObject.transform.GetChild(0).localRotation =
                    Quaternion.RotateTowards(this.gameObject.transform.GetChild(0).localRotation, Quaternion.Euler(-0.001f * speed, 0f, 0f), Time.deltaTime * meshRotationSpeed * 2);
            //this will set the entire cat to idle 
            Player.transform.rotation = 
                Quaternion.RotateTowards(Player.transform.rotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * meshRotationSpeed * 2);
            // this will set the wheels to idle
            for (int i = 0; i < 2; i++)
                Wheels.GetChild(i).localRotation =
                    Quaternion.RotateTowards(Wheels.GetChild(i).localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * meshRotationSpeed * 2);
        }
        /* Player.transform.rotation will rotate the entire car left and right 
         and gameObject.transform.GetChild(0).localRotation 
         will rotate the body without the wheels */
    }

    void SpeedManager() {
        if (CollisionChecker.isColliding == false) { // when no collision is happening 
            if (speed < 2000 && speed >= 500)  // will increase speed by time 
                speed += Time.deltaTime * 50;
            else if (speed < 500) // will increase speed rapidly when starting under 500
                speed += Time.deltaTime * 100;
        }
        else { // when colliding
            if (CollisionChecker.colliderType == false && speed > 500) // will decrease speed when coliding with boundries
                speed -= Time.deltaTime * 200;
            if (CollisionChecker.colliderType == true && CollisionChecker.objectCollidedWith.tag == "AI" && CollisionChecker.objectCollidedWith.GetComponent<AIController>().backProxType == 2)
                // added the backProxType == 2 condition so that the player gets the ai speed only when colliding from the back and not from the sides
                speed = CollisionChecker.objectCollidedWith.GetComponent<AIController>().speed; // in case of collision with ai at low speed player will gain that ai's speed
        }

        if (IsBraking() && speed > 400) {
            speed -= 10;
        }
    }

    public bool IsBraking() {
        if (Input.GetKey(KeyCode.Space) == true)
            return true;
        else
            return false;
    }
    /*
    add breaks and acceleration inputs.. with mesh update for brake
    .. and maybe for acceleration not sure .. 
    maybe just maybe add gear mesh update in certain speeds 
    remember you're already on second gear so add 3 mesh updates for gear if you decide to add them
    add mobile inputs
    */
}