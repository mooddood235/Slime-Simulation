using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlimeHandler : MonoBehaviour
{
    private Agent[] agents;
    [SerializeField] private uint agentCount;
    [SerializeField] private float speed;
    [SerializeField] private float decaySpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float sensorAngle;
    [SerializeField] private float sensorOffset;
    [SerializeField] private float bound;
    [SerializeField] private SpawnPosition spawnPos;
    [SerializeField] private SpawnDirection spawnDir;
    [SerializeField] private trailMode trailMode;
    [Space]
    [SerializeField] private RenderTexture trailMap;
    [SerializeField] private RenderTexture processedTrailMap;
    [SerializeField] private ComputeShader dataCompute;
    [SerializeField] private ComputeShader processingCompute;
    [Space]
    [SerializeField] private uint width;
    [SerializeField] private uint height;
    private void Awake() {
        agents = new Agent[agentCount];

        trailMap = new RenderTexture((int)width, (int)height, 0);
        trailMap.enableRandomWrite = true;
        trailMap.filterMode = FilterMode.Point;

        processedTrailMap = new RenderTexture((int)width, (int)height, 0);
        processedTrailMap.enableRandomWrite = true;
    }
    void Start()
    {
        CreateAgents(agentCount);
    }

    // Update is called once per frame
    void Update()
    {
        DispatchProcessingCompute();       
        DispatchDataCompute(); 
    }

    private void DispatchDataCompute(){
        ComputeBuffer agentsCB = new ComputeBuffer((int)agentCount, 3 * sizeof(float));
        agentsCB.SetData(agents);
        dataCompute.SetBuffer(0, "agents", agentsCB);

        dataCompute.SetTexture(0, "trailMap", trailMap);

        dataCompute.SetInt("width", (int)width);
        dataCompute.SetInt("height", (int)height);
        dataCompute.SetFloat("deltaTime", Time.deltaTime);
        dataCompute.SetFloat("speed", speed);
        dataCompute.SetFloat("turnSpeed", turnSpeed);
        dataCompute.SetFloat("sensorAngle", sensorAngle);
        dataCompute.SetFloat("sensorOffset", sensorOffset);

        dataCompute.Dispatch(0, (int)agentCount / 1000, 1, 1);

        agentsCB.GetData(agents);

        agentsCB.Dispose();
    }
    private void DispatchProcessingCompute(){
        processingCompute.SetTexture(0, "trailMap", trailMap);
        processingCompute.SetTexture(0, "processingMap", processedTrailMap);
        processingCompute.SetFloat("decaySpeed", decaySpeed);
        processingCompute.SetFloat("deltaTime", Time.deltaTime);
        processingCompute.SetInt("trailMode", (int)trailMode);
        processingCompute.Dispatch(0, (int)width / 32, (int)height / 32, 1);
    }

    private void CreateAgents(uint count){
        Vector2 pos = new Vector2();
        float angle = 0;

        float centerX = width / 2f;
        float centerY = height / 2f;

        for (int i = 0; i < count; i++){
            if (spawnPos == SpawnPosition.Square){
                pos.x = Random.Range(-bound, bound) + centerX;
                pos.y = Random.Range(-bound, bound) + centerY;
            }
            else if (spawnPos == SpawnPosition.Circle){
                float radius = bound * Mathf.Sqrt(Random.Range(0f, 1f));
                float theta = Random.Range(0f, 1f) * 2 * Mathf.PI;
                pos.x = centerX + radius * Mathf.Cos(theta);
                pos.y = centerY + radius * Mathf.Sin(theta);  
            }
            else if (spawnPos == SpawnPosition.Point){
                pos.x = centerX;
                pos.y = centerY;
            }

            if (spawnDir == SpawnDirection.Random){
                angle = Random.Range(0, 2 * Mathf.PI);
            }
            else if (spawnDir == SpawnDirection.Inwards){
                float xMag = Mathf.Abs(pos.x - centerX);
                float yMag = Mathf.Abs(pos.y - centerY);

                if (pos.x > centerX) xMag *= -1;
                if (pos.y > centerY) yMag *= -1;
                angle = Mathf.Atan2(yMag, xMag);
            }
            else if (spawnDir == SpawnDirection.Outwards){
                float xMag = -Mathf.Abs(pos.x - centerX);
                float yMag = -Mathf.Abs(pos.y - centerY);

                if (pos.x > centerX) xMag *= -1;
                if (pos.y > centerY) yMag *= -1;
                angle = Mathf.Atan2(yMag, xMag);
            }
            agents[i] = new Agent(pos, angle);
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(processedTrailMap, dest);
    }
}

public struct Agent{
    Vector2 pos;
    float angle;
    public Agent(Vector2 pos, float angle){
        this.pos = pos;
        this.angle = angle;
    }
}
enum SpawnPosition{
    Square,
    Circle,
    Point
}
enum SpawnDirection{
    Random,
    Inwards,
    Outwards
}
enum trailMode{
    simple,
    mean
}