using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField]
    private GameObject grassBlock;
    [SerializeField]
    private List<GameObject> gemBlocks = new List<GameObject>();

    public GameObject[,,] BuildWorld()
    {
        GameObject[,,] cubes = new GameObject[16,64,16];
        for(int y = 0; y < 64; y++)
        {
            GameObject layerHolder = new GameObject("Layer Holder");
            layerHolder.transform.SetParent(this.transform);
            for(int x = 0; x < 16; x++)
            {
                for(int z = 0; z < 16; z++)
                {
                    if(y == 0 && (x > 5 && x < 11 && z > 5 && z < 11)) continue; //no cubes in middle on ground level
                    GameObject cubeType = grassBlock;
                    if(y > 0) 
                    {
                        if(x == 0 || x == 15 || z == 0 || z == 15) cubeType = gemBlocks[8]; //create dirt cube
                        else cubeType = GetRandomGem();
                    }
                    GameObject newCube = Instantiate(cubeType, new Vector3(x, -y, z), Quaternion.identity);
                    newCube.transform.SetParent(layerHolder.transform);
                    cubes[x,y,z] = newCube;
                }
            }
        }
        return cubes;
    }

    private GameObject GetRandomGem()
    {
        int randomIndex = Random.Range(0, gemBlocks.Count);
        return gemBlocks[randomIndex];
    }
}
