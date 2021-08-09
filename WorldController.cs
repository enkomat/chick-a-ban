using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Light mainLight;
    [SerializeField]
    private GameObject water;
    [SerializeField]
    private GameObject rain;
    [SerializeField]
    private GameObject shadowCube;
    [SerializeField]
    private ParticleSystem miningEffect;
    [SerializeField]
    private ParticleSystem combineEffect;
    [SerializeField]
    private WorldBuilder worldBuilder;
    [SerializeField]
    private List<GameObject> numberCubes = new List<GameObject>();

    private GameObject[,,] cubes = new GameObject[16,64,16];
    private GameObject[] layers = new GameObject[64];

    private void Awake()
    {
        InitializeGame();
    }

    public bool CanMoveInDirection(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        if(DirectionNotBlocked(x, y, z, xOffset, yOffset, zOffset))
        {
            if(CubeExistsInPosition(x + xOffset, y + yOffset, z + zOffset))
            {
                if(CubeIsMineable(x + xOffset, y + yOffset, z + zOffset))
                {
                    OnCubeMined(x + xOffset, y + yOffset, z + zOffset);
                    ActivateNeededLayers(yOffset, y);
                    MoveShadowCube(yOffset, y);
                    return true;
                }
                else if(CubeCanMoveInDirection(x, y, z, xOffset, yOffset, zOffset))
                {
                    MoveCubeToPosition(x + xOffset, y + yOffset, z + zOffset, xOffset, yOffset, zOffset);
                    ActivateNeededLayers(yOffset, y);
                    MoveShadowCube(yOffset, y);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                ActivateNeededLayers(yOffset, y);
                MoveShadowCube(yOffset, y);
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private void InitializeGame()
    {
        cubes = worldBuilder.BuildWorld();
        SetLayers();
        ActivateNextTwoLayersBelow(0);
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

    private void ActivateNeededLayers(int yOffset, int y)
    {
        if(yOffset == 0) return;
        else if(yOffset == 1) ActivateNextTwoLayersBelow(y+1);
        else if(yOffset == -1) ActivateNextTwoLayersBelow(y-1);

        if(y == 0 && yOffset == 1)
        {
            ChangeLightIntensity(0.75f);
            DisableWaterAndRain();
        } 
        else if(y == 1 && yOffset == -1) 
        {
            ChangeLightIntensity(1.4f);
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
        PlayMiningEffect(x, y, z);
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

    private void MoveCubeToPosition(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        
        if(x + xOffset < 0 || x + xOffset > 15 || z + zOffset < 0 || z + zOffset > 15 || cubes[x,y,z] == null) return;
        
        if(yOffset != 0) cubes[x,y,z].transform.parent = layers[y + yOffset].transform;
        if(OverSimilarCube(x + xOffset, y + yOffset, z + zOffset)) 
        {
            CombineCubes(x, y, z, xOffset, yOffset, zOffset);
        }
        else
        {
            cubes[x,y,z].transform.position = cubes[x,y,z].transform.position + new Vector3(xOffset, -yOffset, zOffset);
            cubes[x + xOffset, y + yOffset, z + zOffset] = cubes[x,y,z];
            cubes[x,y,z] = null;
        }
    }

    private void CombineCubes(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {   
        if(cubes[x,y,z].name.Contains("stone") || cubes[x + xOffset, y + yOffset, z + zOffset].name.Contains("stone")) return;
        
        GameObject newCube = Instantiate(GetCorrectCreatableCube(cubes[x,y,z]), cubes[x + xOffset, y + yOffset, z + zOffset].transform.position, Quaternion.identity);
        newCube.transform.parent = layers[y].transform;
        Destroy(cubes[x + xOffset, y + yOffset, z + zOffset]);
        Destroy(cubes[x,y,z]);
        cubes[x + xOffset, y + yOffset, z + zOffset] = newCube;
        cubes[x,y,z] = null;
        PlayCombineEffect(x, y, z, xOffset, yOffset, zOffset);
    }

    private void PlayCombineEffect(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        combineEffect.transform.position = new Vector3(x + xOffset, -y + yOffset, z + zOffset);
        combineEffect.Play();
    }

    private void PlayMiningEffect(int x, int y, int z)
    {
        miningEffect.transform.position = new Vector3(x, -y, z);
        miningEffect.Play();
    }

    private void MoveShadowCube(int yOffset, int y)
    {
        if((yOffset == 1 && y < 0) || (yOffset == -1 && y == 0)) return; 

        Vector3 originalPosition = shadowCube.transform.position;
        if(yOffset == -1) shadowCube.transform.position = new Vector3(originalPosition.x, originalPosition.y + 1, originalPosition.z);
        else if(yOffset == 1) shadowCube.transform.position = new Vector3(originalPosition.x, originalPosition.y - 1, originalPosition.z);
        else return;
    }

    private GameObject GetCorrectCreatableCube(GameObject combinable)
    {
        if(combinable.name.Contains("1")) return numberCubes[1];
        else if(combinable.name.Contains("2")) return numberCubes[2];
        else if(combinable.name.Contains("4")) return numberCubes[3];
        else if(combinable.name.Contains("8")) return numberCubes[4];
        else if(combinable.name.Contains("16")) return numberCubes[5];
        else if(combinable.name.Contains("32")) return numberCubes[6];
        else if(combinable.name.Contains("64")) return numberCubes[7];
        else if(combinable.name.Contains("128")) return numberCubes[8];
        else if(combinable.name.Contains("256")) return numberCubes[9];
        else if(combinable.name.Contains("512")) return numberCubes[10];
        else if(combinable.name.Contains("1024")) return numberCubes[11];
        return numberCubes[0];
    }

    private bool CubesAreSimilar(GameObject cube1, GameObject cube2)
    {
        if(cube1 == null || cube2 == null) return false;
        return cube1.name == cube2.name;
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

    private bool SimilarCubeInDirectionFound(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        if(CubesAreSimilar(cubes[x+xOffset, y+yOffset, z+zOffset], cubes[x+(xOffset*2), y+(yOffset*2), z+(zOffset*2)])) return true;
        else return false;
    }

    private bool NewCubePositionIsInsideBounds(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        if(x <= 1 && xOffset == -1) return false;
        else if(x >= 14 && xOffset == 1) return false;
        else if(z <= 1 && zOffset == -1) return false;
        else if(z >= 14 && zOffset == 1) return false;
        else return true;
    }

    private bool DirectionNotBlocked(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        return 
        // clamp movement to bounds of the world, this could be done with mathf.clamp
        ((xOffset == 1 && x < 15) || (xOffset == -1 && x > 0) || (yOffset == 1 && y < 63) || (yOffset == -1 && y > -1) || (zOffset == 1 && z < 15) || (zOffset == -1 && z > 0));
    }

    private bool CubeCanMoveInDirection(int x, int y, int z, int xOffset, int yOffset, int zOffset)
    {
        return NewCubePositionIsInsideBounds(x, y, z, xOffset, yOffset, zOffset) && 
        (!CubeIsMineable(x + xOffset, y + yOffset, z + zOffset) && CubeExistsInPosition(x + xOffset, y + yOffset, z + zOffset) && (!CubeExistsInPosition(x + (xOffset * 2), y + (yOffset * 2), z + (zOffset * 2)) || SimilarCubeInDirectionFound(x, y, z, xOffset, yOffset, zOffset)));
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

    private void ChangeLightIntensity(float amt)
    {
        mainLight.intensity = amt;
    }
}
