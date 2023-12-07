using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillGap : MonoBehaviour
{
    public Transform Cube1_trans, Cube2_trans;

    Transform this_trans;
    float cube1_r;
    float cube2_r;
    float cube1_hulfsize;
    float cube2_hulfsize;
    float parentScale_X;
    Vector3 Cube1_edge_position;
    Vector3 Cube2_edge_position;

    void Start()
    {
        this_trans = gameObject.GetComponent<Transform>();
        parentScale_X = transform.root.gameObject.GetComponent<Transform>().localScale.x;
        cube1_hulfsize = Cube1_trans.lossyScale.x / 2;
        cube2_hulfsize = Cube2_trans.lossyScale.x / 2;
    }
    void FixedUpdate()
    {
        cube1_r = Cube1_trans.localEulerAngles.z * Mathf.Deg2Rad;
        cube2_r = Cube2_trans.localEulerAngles.z * Mathf.Deg2Rad;
        Cube1_edge_position = new Vector3(Cube1_trans.position.x - cube1_hulfsize * Mathf.Cos(cube1_r), Cube1_trans.position.y - cube1_hulfsize * Mathf.Sin(cube1_r), Cube1_trans.position.z);
        Cube2_edge_position = new Vector3(Cube2_trans.position.x + cube2_hulfsize * Mathf.Cos(cube2_r), Cube2_trans.position.y + cube2_hulfsize * Mathf.Sin(cube2_r), Cube2_trans.position.z);
        this_trans.localScale = new Vector3((Cube1_edge_position - Cube2_edge_position).magnitude / parentScale_X, 1, 1);
        this_trans.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Cube1_edge_position.y - Cube2_edge_position.y, Cube1_edge_position.x - Cube2_edge_position.x) * Mathf.Rad2Deg);
        this_trans.position = (Cube1_edge_position + Cube2_edge_position) / 2;
    }
}
