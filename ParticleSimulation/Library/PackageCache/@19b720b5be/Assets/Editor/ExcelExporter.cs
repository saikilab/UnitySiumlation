using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class ExcelExporter
{
	[MenuItem("Assets/ExcelExport")]
	static void ExcelExport()
	{
		GameObject objRoot = GameObject.Find("ObjRoot");
		if(objRoot == null){ return; }
		
		Transform[] arrChara = objRoot.GetComponentsInChildren<Transform>();
		if(arrChara == null){ return; }
		
		//--------------------------------------------------------------------------------------
		//  Ready Asset
		//--------------------------------------------------------------------------------------

		//Object[] selection = Selection.GetFiltered(typeof(CharaDataSet), SelectionMode.DeepAssets);
		Object _dataSet = Selection.activeObject;
		if(_dataSet.GetType() != typeof(CharaDataSet))
		{
			Debug.LogError("Selection.activeObject is not CharaDataSet");
			return;
		}
		CharaDataSet dataSet = _dataSet as CharaDataSet;			
		dataSet.Clear();
				
		//--------------------------------------------------------------------------------------
		//  Ready Excel
		//--------------------------------------------------------------------------------------

		// Create Excel
		IWorkbook book = new HSSFWorkbook();
		
		// Create Sheet
		ISheet sheet = book.CreateSheet("chara data1");
		
		// CreateHeader Row
		CreateHeaderRow(sheet);
		
		//--------------------------------------------------------------------------------------
		//  Recoad Data
		//--------------------------------------------------------------------------------------

		int rowIdx = 2;
		
		foreach(Transform chara in arrChara)
		{
			if(objRoot == chara.gameObject){ continue; }
			
			// Add To Assets
			CharaData data = AddToAsset(dataSet, chara);
			
			// Add To Excel
			AddToExcel(sheet, data, rowIdx++);
		}
		
		//--------------------------------------------------------------------------------------
		//  Save Data
		//--------------------------------------------------------------------------------------

		// Apply Change of Asset
		EditorUtility.SetDirty(_dataSet);		

		// Save Excel File
		string file = "Assets/Editor/Data/" + _dataSet.name + ".xls";
		Debug.Log("file:"+file);
        using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
        {
            book.Write(fs);
        }	
	}
	
	static CharaData AddToAsset(CharaDataSet _dataSet, Transform _chara)
	{
		CharaData data = new CharaData();
		
		data.m_name = _chara.name;
		data.m_hp = Random.Range(0, 100);
		data.m_position = new Vector3(_chara.position.x, _chara.position.y, _chara.position.z);
		
		_dataSet.list.Add(data);
		
		return data;
	}
	
	static void AddToExcel(ISheet _sheet, CharaData _data, int _rowIdx)
	{
		IRow row = _sheet.CreateRow(_rowIdx);

		int cellIdx = 1;
		
		// name
		ICell nameCell = row.CreateCell(cellIdx++);
		nameCell.SetCellValue(_data.m_name);

		// hp
		ICell hpCell = row.CreateCell(cellIdx++);
		hpCell.SetCellValue(_data.m_hp);

		// x
		ICell posxCell = row.CreateCell(cellIdx++);
		posxCell.SetCellValue(_data.m_position.x);

		// y
		ICell posyCell = row.CreateCell(cellIdx++);
		posyCell.SetCellValue(_data.m_position.y);

		// z
		ICell poszCell = row.CreateCell(cellIdx++);
		poszCell.SetCellValue(_data.m_position.z);
	}
	
	static void CreateHeaderRow(ISheet _sheet)
	{
		IRow header = _sheet.CreateRow(1);
		
		int cellIdx = 1;
		
		// name
		ICell nameCell = header.CreateCell(cellIdx++);
		nameCell.SetCellValue("name");

		// hp
		ICell hpCell = header.CreateCell(cellIdx++);
		hpCell.SetCellValue("hp");

		// x
		ICell posxCell = header.CreateCell(cellIdx++);
		posxCell.SetCellValue("x");

		// y
		ICell posyCell = header.CreateCell(cellIdx++);
		posyCell.SetCellValue("y");

		// z
		ICell poszCell = header.CreateCell(cellIdx++);
		poszCell.SetCellValue("z");
	}
}