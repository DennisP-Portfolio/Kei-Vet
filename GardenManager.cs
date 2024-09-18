using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    [Header("Available Objects")]
    [SerializeField] private List<GameObject> _SelectableObjects = new();

    [Header("Current Garden")]
    [SerializeField] private List<Vector3> _ObjectPositions = new();
    [SerializeField] private List<Quaternion> _ObjectRotations = new();
    [SerializeField] private List<int> _ObjectIDs = new();

    private void Start()
    {
        SetZenData();
    }

    public GameObject ReturnObject(int index)
    {
        return _SelectableObjects[index];
    }

    public void PlaceObject(Vector3 pos, Quaternion rot, GameObject obj)
    {
        _ObjectPositions.Add(pos);
        _ObjectRotations.Add(rot);
        _ObjectIDs.Add(_SelectableObjects.IndexOf(obj));
        SaveZenData();
    }

    private void SaveZenData()
    {
        foreach (var objectid in _ObjectIDs)
        {
            print(objectid);
        }
        SaveData<Vector3> dataPosition = new SaveData<Vector3>(_ObjectPositions);
        SaveData<Quaternion> dataRotation = new SaveData<Quaternion>(_ObjectRotations);
        SaveData<int> dataIDs = new SaveData<int>(_ObjectIDs);

        string positionJSON = JsonUtility.ToJson(dataPosition);
        string rotationJSON = JsonUtility.ToJson(dataRotation);
        string idsJSON = JsonUtility.ToJson(dataIDs);
        
        System.IO.File.WriteAllText(Application.persistentDataPath + "/zenPos.json", positionJSON);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/zenRot.json", rotationJSON);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/zenIds.json", idsJSON);
    }
    
    private void SetZenData()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/zenPos.json"))
        {
            string positionJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/zenPos.json");
            string rotationJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/zenRot.json");
            string idsJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/zenIds.json");

            SaveData<Vector3> savePos = JsonUtility.FromJson<SaveData<Vector3>>(positionJson);
            SaveData<Quaternion> saveRot = JsonUtility.FromJson<SaveData<Quaternion>>(rotationJson);
            SaveData<int> saveIds = JsonUtility.FromJson<SaveData<int>>(idsJson);
            
            SpawnSavedGarden(savePos.list, saveRot.list, saveIds.list);
        }
    }

    public void RemoveObject(Vector3 pos, Quaternion rot, GameObject obj)
    {
        int index = _ObjectPositions.IndexOf(pos);
        _ObjectPositions.Remove(pos);
        _ObjectRotations.RemoveAt(index);
        _ObjectIDs.RemoveAt(index);
        SaveZenData();
    }

    public void SpawnSavedGarden(List<Vector3> positions, List<Quaternion> rotations, List<int> IDs)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject obj = Instantiate(_SelectableObjects[IDs[i]], transform);
            obj.transform.position = positions[i];
            obj.transform.rotation = rotations[i];

            _ObjectPositions.Add(positions[i]);
            _ObjectRotations.Add(rotations[i]);
            _ObjectIDs.Add(IDs[i]);
        }
    }
}
