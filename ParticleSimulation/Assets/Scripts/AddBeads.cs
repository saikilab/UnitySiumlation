using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBeads : MonoBehaviour
{
    public NewParticleController newParticleController;
    public GameObject Polybeads;
    GameObject tmp;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (newParticleController.addBeadsByWallPos)
            {
                int i;
                Vector3 newPos = Vector3.zero;
                for (i = 0; i < newParticleController.WallObjects.Length; i++)
                {
                    newPos += newParticleController.WallObjects[i].transform.position;
                }
                Instantiate(Polybeads).transform.position = newPos / newParticleController.WallObjects.Length;
            }
            else
            {
                tmp = Instantiate(Polybeads);
            }
        }
    }
}
