using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class SaveController : MonoBehaviour
{
    public WallController wallController;
    public NewParticleController particleController;
    public string dirN;
    public GameObject SavePanel;
    int i, j;

    public void FixedUpdate()
    {
        if (SimulationController.endSimulation)
        {
            particleController.saveParticlePosition = false;
            OutPutSaveData();
        }
    }

    public void ClickedSave()
    {
        this.GetComponent<Button>().interactable = false;
        SavePanel.SetActive(true);
    }

    public void StandardSave()
    {
        SimulationController.startSimulation = true;
        SavePanel.SetActive(false);
        dirN = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
        Directory.CreateDirectory(dirN);
        Directory.CreateDirectory(dirN + "/particles");

        wallController.step = 0;
        particleController.step = 0;
        particleController.saveParticlePosition = true;
        //wallController.useSaveWall = true;
    }

    public void ClickedFullWall()
    {
        StandardSave();
        wallController.useTimeWall = true;
    }

    public void ClickedPiston()
    {
        StandardSave();
        wallController.usePistonWall = true;
    }

    public void ClickedRepeat()
    {
        StandardSave();
        wallController.useRepeatWall = true;
    }

    public void ClickedEFM()
    {
        StandardSave();
        wallController.useEFM = true;
        WallReceivePow.Fp = new Vector3(0,0,0);
    }

    public void ClickedFilmPress()
    {
        StandardSave();
        wallController.usePressMachine = true;
    }

    public void OutPutSaveData()
    {
        //ParticlePosition
        for (i = 0; i <= SimulationController.Step; i++)
        {
            StreamWriter swP = new StreamWriter(dirN + "/particles" + "/particle_colloid_" + i.ToString("d5") + ".cdv");

            for (j = 0; j < particleController.particleNumber; j++)
                swP.WriteLine(particleController.AllParticlePosition[i, j]);

            swP.Close();
        }

        //WallPosition
        StreamWriter swW = new StreamWriter(dirN + "/Wall_Position.cdv");
        for (i = 0; i <= SimulationController.Step; i++)
            swW.WriteLine(wallController.WallPosition[i]);

        swW.Close();

        //Fp
        StreamWriter swFp = new StreamWriter(dirN + "/Fp.csv");
        for (i = 0; i <= SimulationController.Step; i++)
            swFp.WriteLine(wallController.Fp[i]);
        swFp.Close();

        Debug.Log("保存が完了しました");
        SimulationController.Step = 0;
        SimulationController.startSimulation = false;
        SimulationController.endSimulation = false;
        this.GetComponent<Button>().interactable = true;
    }
}
