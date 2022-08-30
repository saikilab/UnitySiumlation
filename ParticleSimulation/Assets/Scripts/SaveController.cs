using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SaveController : MonoBehaviour
{
    public WallController wallController;
    public MagnetDipole magnetDipole;
    public string dirN;

    public void ClickedSave()
    {
        dirN = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
        Directory.CreateDirectory(dirN);
        Directory.CreateDirectory(dirN + "/particles");
        magnetDipole.dirN = dirN + "/particles";
        wallController.dirN = dirN;
        wallController.sw = new StreamWriter(dirN + "/Wall_Position.cdv");

        wallController.step = 0;
        magnetDipole.step = 0;
        magnetDipole.switch_save_mag = true;
        wallController.switch_save_wall = true;
        wallController.timeWall = true;
    }
}
