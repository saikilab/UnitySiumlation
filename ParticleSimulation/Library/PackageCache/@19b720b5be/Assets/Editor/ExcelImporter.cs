using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class ExcelImporter : AssetPostprocessor {
	
	static void OnPostprocessAllAssets (
			string[] importedAssets, 
			string[] deletedAssets, 
			string[] movedAssets, 
			string[] movedFromAssetPaths)
	{
		foreach (string file in importedAssets) 
		{			
			if (file.EndsWith(".xls"))
			{
				Debug.Log("FilePath:"+file);
				
				string asset_path = "Assets/Data/" + Path.GetFileNameWithoutExtension(file) + ".asset";
				Debug.Log("AssetPath:"+asset_path);
				
				using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					// Load or Create Asset
					CharaDataSet charaDataSet = AssetDatabase.LoadAssetAtPath(asset_path, typeof(CharaDataSet)) as CharaDataSet;					
					if(charaDataSet == null)
					{
						charaDataSet = ScriptableObject.CreateInstance<CharaDataSet>();
						AssetDatabase.CreateAsset(charaDataSet, asset_path);
					}
					charaDataSet.Clear();
					
					// Open Excel
					IWorkbook book = new HSSFWorkbook(fs);
					int sheetNum = book.NumberOfSheets;
					for(int i=0; i<sheetNum; ++i)
					{
						ISheet sheet = book.GetSheetAt(i);
						Debug.Log("Sheet:" + sheet.SheetName);
						
						int firstRow = sheet.FirstRowNum;
						int lastRow = sheet.LastRowNum;
						
						for(int rowIdx=firstRow+1; rowIdx<=lastRow; ++rowIdx)
						{
							IRow row = sheet.GetRow(rowIdx);
							if(row == null){ continue; }
							
							int cellIdx = row.FirstCellNum;
							
							ICell nameCell 	= row.GetCell(cellIdx++);
							ICell hpCell 	= row.GetCell(cellIdx++);
							ICell posxCell	= row.GetCell(cellIdx++);
							ICell posyCell	= row.GetCell(cellIdx++);
							ICell poszCell	= row.GetCell(cellIdx++);
							
							CharaData charaData = new CharaData();
							charaData.m_name = nameCell.StringCellValue;
							charaData.m_hp	= (int)hpCell.NumericCellValue;
							charaData.m_position = new Vector3((float)posxCell.NumericCellValue, 
																(float)posyCell.NumericCellValue, 
																(float)poszCell.NumericCellValue);
							
							// Add Data
							Debug.Log("Add:"+charaData.m_name+","+charaData.m_hp);
							charaDataSet.Add(charaData);
							
							CreateOrUpdateObject(charaData);
						}
					}

					// Apply Data
					ScriptableObject obj = AssetDatabase.LoadAssetAtPath(asset_path, typeof(ScriptableObject)) as ScriptableObject;
					EditorUtility.SetDirty(obj);
				}
			}
			
			if (file.EndsWith(".xlsx"))
			{
				Debug.Log("FilePath:"+file);
				Debug.LogWarning(".xlsx file is not spported now.");
			}

		}
	}
	
	static private void CreateOrUpdateObject(CharaData _data)
	{
		if(_data == null){ return; }
		
		GameObject objRoot = GameObject.Find("ObjRoot");
		if(objRoot == null){ return; }
		
		GameObject objChara = GameObject.Find(_data.m_name);
		if(objChara == null)
		{
			//objChara = new GameObject(_data.m_name); 
			objChara = GameObject.CreatePrimitive(PrimitiveType.Cube);
			objChara.name = _data.m_name;
		}
		objChara.transform.parent = objRoot.transform;
		objChara.transform.position = new Vector3(_data.m_position.x, _data.m_position.y, _data.m_position.z);
	}
}
