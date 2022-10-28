using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public static int Step;
    public static int MaxStep = 10000;
    public static bool startSimulation, endSimulation;

    void Start()
    {
        Step = 0;
        startSimulation = false;
        endSimulation = false;
    }

    void FixedUpdate()
    {
        if (startSimulation && !endSimulation)
        {
            Step++;
        }
    }

    public void ClickedStartSimulation()
    {
        startSimulation = true;
    }
}
