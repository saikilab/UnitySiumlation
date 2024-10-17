using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour {
	
	public CharaDataSet m_charaDataSet;

	// Use this for initialization
	void Start () 
	{
		Debug.Log("[LOG]TestScript");
		
		if(m_charaDataSet != null)
		{
			foreach(CharaData data in m_charaDataSet.list)
			{
				if(data != null)
				{
					Debug.Log("name:"+data.m_name+", hp:"+data.m_hp);
				}
				else
				{
					Debug.Log("NULL!!");
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
