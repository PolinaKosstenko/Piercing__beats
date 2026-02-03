using UnityEngine;

public class RobotSpawner : MonoBehaviour
{
    public GameObject[] robots;
    private int currentRobot = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 1; i < robots.Length; ++i)
        {
            robots[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SpawnNext()
    {
        Destroy(robots[currentRobot]);
        robots[++currentRobot].SetActive(true);
    }
}
