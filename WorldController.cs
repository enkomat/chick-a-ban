using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject water;
    public GameObject rain;
    public GameObject shadowCube;
    public ParticleSystem miningEffect;
    public WorldBuilder worldBuilder;
    public List<GameObject> numberCubes = new List<GameObject>();
    private GameObject[,,] cubes = new GameObject[16,64,16];
    private GameObject[] layers = new GameObject[64];

    private void Awake()
    {
        cubes = worldBuilder.BuildWorld();
        SetLayers();
        ActivateNextTwoLayersBelow(0);
    }

    public bool CanMoveInDirection(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {
        if(DirectionNotBlocked(x, y, z, x_offset, y_offset, z_offset))
        {
            if(CubeExistsInPosition(x + x_offset, y + y_offset, z + z_offset))
            {
                if(CubeIsMineable(x + x_offset, y + y_offset, z + z_offset))
                {
                    OnCubeMined(x + x_offset, y + y_offset, z + z_offset);
                    ActivateNeededLayers(y_offset, y);
                    MoveShadowCube(y_offset, y);
                    return true;
                }
                else if(y_offset == 0 && CubeCanMoveInDirection(x, y, z, x_offset, y_offset, z_offset))
                {
                    MoveCubeToPosition(x + x_offset, y + y_offset, z + z_offset, x_offset, y_offset, z_offset);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                ActivateNeededLayers(y_offset, y);
                MoveShadowCube(y_offset, y);
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private void SetLayers()
    {
        for(int y = 0; y < 64; y++)
        {
            layers[y] = cubes[0, y, 0].transform.parent.gameObject;
            layers[y].SetActive(false);
        }
    }

    private void ActivateAllLayersAfterIndex(int layerIndex)
    {
        if(layerIndex < 0) return;

        for(int y = 0; y < 64; y++)
        {
            if(y >= layerIndex) layers[y].SetActive(true);
            else layers[y].SetActive(false);
        }
    }

    private void ActivateNeededLayers(int y_offset, int y)
    {
        if(y_offset == 0) return;
        else if(y_offset == 1) ActivateNextTwoLayersBelow(y+1);
        else if(y_offset == -1) ActivateNextTwoLayersBelow(y-1);

        if(y == 0 && y_offset == 1)
        {
            ActivateUndergroundSkybox();
            DisableWaterAndRain();
        } 
        else if(y == 1 && y_offset == -1) 
        {
            ActivateOvergroundSkybox();
            ActivateWaterAndRain();
        }
    }

    private void ActivateNextTwoLayersBelow(int layerIndex)
    {
        if(layerIndex < 0) layerIndex = 0;

        DisableAllLayers();
        layers[layerIndex].SetActive(true);
        layers[layerIndex+1].SetActive(true);
    }

    private void DisableAllLayers()
    {
        for(int y = 0; y < 64; y++)
        {
            layers[y].SetActive(false);
        }
    }

    private void OnCubeMined(int x, int y, int z)
    {
        Destroy(cubes[x,y,z]);
        cubes[x,y,z] = null;
        miningEffect.transform.position = new Vector3(x, -y, z);
        miningEffect.Play();
    }

    private bool CubeExistsInPosition(int x, int y, int z)
    {
        if(x < 0 || x > 15 || y < 0 || y > 63 || z < 0 || z > 15 || cubes[x,y,z] == null) return false;
        return cubes[x,y,z] != null;
    }

    private bool CubeIsMineable(int x, int y, int z)
    {
        if(x < 0 || x > 15 || y < 0 || y > 63 || z < 0 || z > 15 || cubes[x,y,z] == null) return false;
        bool isMineable = cubes[x,y,z].name.Contains("dirt") || cubes[x,y,z].name.Contains("grass");
        return isMineable;
    }

    private void MoveCubeToPosition(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {
        
        if(x + x_offset < 0 || x + x_offset > 15 || z + z_offset < 0 || z + z_offset > 15 || cubes[x,y,z] == null) return;
        if(OverSimilarCube(x + x_offset, y + y_offset, z + z_offset)) 
        {
            CombineCubes(x, y, z, x_offset, y_offset, z_offset);
        }
        else
        {
            cubes[x,y,z].transform.position = cubes[x,y,z].transform.position + new Vector3(x_offset, y_offset, z_offset);
            cubes[x + x_offset, y + y_offset, z + z_offset] = cubes[x,y,z];
            cubes[x,y,z] = null;
        }
    }

    private void CombineCubes(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {   
        if(cubes[x,y,z].name.Contains("stone") || cubes[x + x_offset, y + y_offset, z + z_offset].name.Contains("stone")) return;
        GameObject newCube = Instantiate(GetCorrectCreatableCube(cubes[x,y,z]), cubes[x + x_offset, y + y_offset, z + z_offset].transform.position, Quaternion.identity);
        newCube.transform.parent = layers[y].transform;
        Destroy(cubes[x + x_offset, y + y_offset, z + z_offset]);
        Destroy(cubes[x,y,z]);
        cubes[x + x_offset, y + y_offset, z + z_offset] = newCube;
        cubes[x,y,z] = null;
    }

    private void MoveShadowCube(int y_offset, int y)
    {
        if((y_offset == 1 && y < 0) || (y_offset == -1 && y == 0)) return; 

        Vector3 originalPosition = shadowCube.transform.position;
        if(y_offset == -1) shadowCube.transform.position = new Vector3(originalPosition.x, originalPosition.y + 1, originalPosition.z);
        else if(y_offset == 1) shadowCube.transform.position = new Vector3(originalPosition.x, originalPosition.y - 1, originalPosition.z);
        else return;
    }

    private GameObject GetCorrectCreatableCube(GameObject combinable)
    {
        if(combinable.name.Contains("1")) return numberCubes[1];
        else if(combinable.name.Contains("2")) return numberCubes[2];
        else if(combinable.name.Contains("4")) return numberCubes[3];
        else if(combinable.name.Contains("8")) return numberCubes[4];
        return numberCubes[4];
    }

    private bool CubesAreSimilar(GameObject cube_1, GameObject cube_2)
    {
        if(cube_1 == null || cube_2 == null) return false;
        return cube_1.name == cube_2.name;
    }

    private bool OverSimilarCube(int x, int y, int z)
    {
        if(x < 15 && CubesAreSimilar(cubes[x,y,z], cubes[x+1, y, z])) return true;
        else if(x > 0 && CubesAreSimilar(cubes[x,y,z], cubes[x-1, y, z])) return true;
        else if(z < 15 && CubesAreSimilar(cubes[x,y,z], cubes[x, y, z+1])) return true;
        else if(z > 0 && CubesAreSimilar(cubes[x,y,z], cubes[x, y, z-1])) return true;
        else return false;
    }

    private GameObject GetSimilarAdjacentCube(int x, int y, int z)
    {
        if(x < 15 && CubesAreSimilar(cubes[x,y,z], cubes[x+1, y, z])) return cubes[x+1, y, z];
        else if(x > 0 && CubesAreSimilar(cubes[x,y,z], cubes[x-1, y, z])) return cubes[x-1, y, z];
        else if(z < 15 && CubesAreSimilar(cubes[x,y,z], cubes[x, y, z+1])) return cubes[x, y, z+1];
        else if(z > 0 && CubesAreSimilar(cubes[x,y,z], cubes[x, y, z-1])) return cubes[x, y, z-1];
        else return null;
    }

    private bool SimilarCubeInDirectionFound(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {
        if(CubesAreSimilar(cubes[x+x_offset, y+y_offset, z+z_offset], cubes[x+(x_offset*2), y+(y_offset*2), z+(z_offset*2)])) return true;
        else return false;
    }

    private bool NewCubePositionIsInsideBounds(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {
        if(x <= 1 && x_offset == -1) return false;
        else if(x >= 14 && x_offset == 1) return false;
        else if(z <= 1 && z_offset == -1) return false;
        else if(z >= 14 && z_offset == 1) return false;
        else return true;
    }

    private bool DirectionNotBlocked(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {
        return 
        // clamp movement to bounds of the world, this could be done with mathf.clamp
        ((x_offset == 1 && x < 15) || (x_offset == -1 && x > 0) || (y_offset == 1 && y < 63) || (y_offset == -1 && y > -1) || (z_offset == 1 && z < 15) || (z_offset == -1 && z > 0));
    }

    private bool CubeCanMoveInDirection(int x, int y, int z, int x_offset, int y_offset, int z_offset)
    {
        return NewCubePositionIsInsideBounds(x, y, z, x_offset, y_offset, z_offset) && 
        (!CubeIsMineable(x + x_offset, y + y_offset, z + z_offset) && CubeExistsInPosition(x + x_offset, y + y_offset, z + z_offset) && (!CubeExistsInPosition(x + (x_offset * 2), y + (y_offset * 2), z + (z_offset * 2)) || SimilarCubeInDirectionFound(x, y, z, x_offset, y_offset, z_offset)));
    }

    private void DisableWaterAndRain()
    {
        water.SetActive(false);
        rain.SetActive(false);
    }

    private void ActivateWaterAndRain()
    {
        water.SetActive(true);
        rain.SetActive(true);
    }

    private void ActivateUndergroundSkybox()
    {
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void ActivateOvergroundSkybox()
    {
        mainCamera.clearFlags = CameraClearFlags.Skybox;
    }
}
