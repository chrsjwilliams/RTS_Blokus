﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Vector3 basePos;
    private float shakeTime;
    private bool shaking;
    private float currentSeed;
    private float shakeSpeed;
    private float shakeMag;

	// Use this for initialization
	void Start () {
        
	}
	
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        basePos = pos;
    }

	// Update is called once per frame
	void Update () {
        if (shaking) Shake();	
	}

    public void StartShake(float dur, float speed, float magnitude)
    {
        currentSeed = Random.Range(0, 1000);
        shaking = true;
        shakeTime = dur;
        shakeSpeed = speed;
        shakeMag = magnitude;
    }

    void Shake()
    {
        shakeTime -= Time.deltaTime;
        float noiseMovement = shakeTime * shakeSpeed;
        float xOffset = Mathf.PerlinNoise(currentSeed, noiseMovement) 
            * shakeMag - (shakeMag / 2);
        float yOffset = Mathf.PerlinNoise(currentSeed + 1000, noiseMovement) 
            * shakeMag - (shakeMag / 2);

        transform.position = basePos + new Vector3(xOffset, yOffset);
        if (shakeTime <= 0)
        {
            shaking = false;
            shakeTime = 0;
        }
    }


}
