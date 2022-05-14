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
        ComputeBuffer agentsCB = new ComputeBuffer((int)agentCount, 5 * sizeof(float));
        agentsCB.SetData(agents);
        dataCompute.SetBuffer(0, "agents", agentsCB);

        dataCompute.SetTexture(0, "trailMap", trailMap);

        dataCompute.SetInt("width", (int)width);
        dataCompute.SetInt("height", (int)height);

        dataCompute.SetFloat("speed", speed);
        dataCompute.SetFloat("turnSpeed", turnSpeed);
        dataCompute.SetFloat("deltaTime", Time.deltaTime);

        dataCompute.SetFloat("globalSensorAngle", sensorAngle);
        dataCompute.SetFloat("globalSensorOffset", sensorOffset);

        dataCompute.Dispatch(0, (int)agentCount / 1000, 1, 1);

        agentsCB.GetData(agents);

        agentsCB.Dispose();
    }
    private void DispatchProcessingCompute(){
        processingCompute.SetTexture(0, "trailMap", trailMap);
        processingCompute.SetTexture(0, "processingMap", processedTrailMap);
        processingCompute.SetFloat("decaySpeed", decaySpeed);
        processingCompute.SetFloat("deltaTime", Time.deltaTime);
        processingCompute.Dispatch(0, (int)width / 32, (int)height / 32, 1);
    }

    private void CreateAgents(uint count){
        float sqrtCount = Mathf.Sqrt(count);
        for (int i = 0; i < count; i++){
            float x = Random.Range(-sqrtCount, sqrtCount);
            float y = Random.Range(-sqrtCount, sqrtCount);
            agents[i] = new Agent(new Vector2(x + width / 2, y + height / 2),
             Random.Range(0, 2* Mathf.PI), sensorAngle, sensorOffset);
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(processedTrailMap, dest);
    }
}

public struct Agent{
    Vector2 pos;
    float angle;
    public float sensorAngle;
    public float sensorOffset;
    public Agent(Vector2 pos, float angle, float sensorAngle, float sensorOffset){
        this.pos = pos;
        this.angle = angle;
        this.sensorAngle = sensorAngle;
        this.sensorOffset = sensorOffset;
    }
}