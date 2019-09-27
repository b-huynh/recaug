using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class RoomManager : MonoBehaviour
{
    public string roomName = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("Attempting to save current room mesh");
            SaveRoom();
        }       
    }

    public void SaveRoom()
    {
        if (roomName == null)
        {
            Debug.Log("Cannot save as room name not specified.");
        }

        List<MeshFilter> roomMeshFilters = 
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.GetMeshFilters() as List<MeshFilter>;
    
        string savedPath = MeshSaver.Save(roomName, roomMeshFilters);
        Debug.LogFormat("Saved mesh to: {0}", savedPath);
    }

    public void LoadRoom(string roomName)
    {
        MeshSaver.Load(roomName);   
    }
}
