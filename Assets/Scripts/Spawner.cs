using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject boil;
    [SerializeField] private int number = 100;
    [SerializeField] private float spacing = 10.0f;

    void Awake()
    {
        int gridSize = Mathf.CeilToInt(Mathf.Pow(number, 1f / 3f));
        int spawned = 0;

        for (int x = 0; x < gridSize && spawned < number; x++)
        {
            for (int y = 0; y < gridSize && spawned < number; y++)
            {
                for (int z = 0; z < gridSize && spawned < number; z++)
                {
                    Vector3 position = new Vector3(x * spacing, y * spacing, z * spacing);
                    Instantiate(boil, position, Quaternion.identity);
                    spawned++;
                }
            }
        }
    }
}
