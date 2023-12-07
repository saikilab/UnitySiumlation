using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassingManager : MonoBehaviour
{
    public GameObject StartObj, GoalObj;
    public NewParticleController newParticleController;
    Transform[] ParticlesTrans;
    int i, PN;
    float StartPos, GoalPos, counter;
    bool startPassing, endPassing, endSimulation;

    private void Start()
    {
        PN = newParticleController.particleNumber;
        ParticlesTrans = new Transform[PN];
        endSimulation = false;
        for(i = 0; i < PN; i++)
        {
            ParticlesTrans[i] = newParticleController.MagneticParticle[i].transform;
        }
        StartPos = StartObj.transform.position.x;
        GoalPos = GoalObj.transform.position.x;
    }

    private void Reset()
    {
        startPassing = false;
        endPassing = false;
        counter = 0;
    }

    private void FixedUpdate()
    {
        if (!startPassing && !endSimulation)
        {
            for (i = 0; i < PN; i++)
            {
                if (ParticlesTrans[i].position.x < StartPos)
                {
                    startPassing = false;
                    return;
                }
                else
                {
                    startPassing = true;
                }
            }
        }
        else if(!endPassing && !endSimulation)
        {
            counter += Time.deltaTime;
            for (i = 0; i < PN; i++)
            {
                if (ParticlesTrans[i].position.x < GoalPos)
                {
                    endPassing = false;
                    return;
                }
                else
                {
                    endPassing = true;
                }
            }
        }

        if(startPassing && endPassing)
        {
            Debug.Log("通過所要時間：" + counter);
            endSimulation = true;
            Reset();
        }
    }
}
