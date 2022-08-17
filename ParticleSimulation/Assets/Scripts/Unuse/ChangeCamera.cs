using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCamera : MonoBehaviour
{
    public GameObject FaceCamera, BackCamera, SideCamera, SlantingCamera;
    public Button ChangeFace, ChangeBack, ChangeSide, ChangeSlanting;

    private void Start()
    {
        ClickedFaceCamera();
    }

    public void ClickedFaceCamera()
    {
        OffAllCamera();
        FaceCamera.SetActive(true);
        ChangeFace.interactable = false;
    }

    public void ClickedBackCamera()
    {
        OffAllCamera();
        BackCamera.SetActive(true);
        ChangeBack.interactable = false;
    }

    public void ClickedSideCamera()
    {
        OffAllCamera();
        SideCamera.SetActive(true);
        ChangeSide.interactable = false;
    }

    public void ClickedSlantingCamera()
    {
        OffAllCamera();
        SlantingCamera.SetActive(true);
        ChangeSlanting.interactable = false;
    }

    private void OffAllCamera()
    {
        FaceCamera.SetActive(false);
        BackCamera.SetActive(false);
        SideCamera.SetActive(false);
        SlantingCamera.SetActive(false);
        ChangeFace.interactable = true;
        ChangeBack.interactable = true;
        ChangeSide.interactable = true;
        ChangeSlanting.interactable = true;
    }
}
