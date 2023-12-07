using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public static int Step;
    public static int MaxStep = 100000;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            startSimulation = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            startSimulation = true;
        }
    }
}
