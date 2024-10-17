using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharaDataSet : ScriptableObject
{
	public List<CharaData> list = null;
	
	void OnEnable()
	{
		hideFlags = HideFlags.NotEditable;
		
		if(list == null)
		{
			Debug.Log("new List<CharaData>();");
			list = new List<CharaData>();
		}
	}
	
	public void Add(CharaData _data)
	{
		if(list != null)
		{
			list.Add(_data);
		}
	}
	
	public void Clear()
	{
		if(list != null)
		{
			list.Clear();
		}
	}
}

[System.SerializableAttribute]
public class CharaData
{
	public string  m_name = "";
	public int	   m_hp = 0;
	public Vector3 m_position = Vector3.zero;
}
