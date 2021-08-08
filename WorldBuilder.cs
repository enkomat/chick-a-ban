using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public GameObject grassBlock;
    public List<GameObject> gemBlocks = new List<GameObject>();

    public GameObject[,,] BuildWorld()
    {
        GameObject[,,] cubes = new GameObject[16,64,16];
        for(int x = 0; x < 16; x++)
        {
            for(int y = 0; y < 64; y++)
            {
                for(int z = 0; z < 16; z++)
                {
                    GameObject cubeType = grassBlock;
                    if(y > 0) cubeType = GetRandomGem();
                    GameObject newCube = Instantiate(cubeType, new Vector3(x, -y, z), Quaternion.identity);
                    newCube.transform.SetParent(transform);
                    newCube.SetActive(false);
                    cubes[x,y,z] = newCube;
                }
            }
        }
        return cubes;
    }

    private GameObject GetRandomGem()
    {
        int random_index = Random.Range(0, gemBlocks.Count);
        return gemBlocks[random_index];
    }
}
