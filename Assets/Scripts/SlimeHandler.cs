using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlimeHandler : MonoBehaviour
{
    private bool running;
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
    [SerializeField] private ComputeShader clearCompute;
    [Space]
    [SerializeField] private TMP_InputField agentField;
    [SerializeField] private TMP_InputField speedField;
    [SerializeField] private TMP_InputField decaySpeedField;
    [SerializeField] private TMP_InputField turnSpeedField;
    [SerializeField] private TMP_InputField sensorAngleField;
    [SerializeField] private TMP_InputField sensorOffsetField;
    [SerializeField] private TMP_InputField boundField;
    [SerializeField] private TMP_Dropdown spawnPosDrop;
    [SerializeField] private TMP_Dropdown spawnDirDrop;
    [SerializeField] private TMP_Dropdown trailModeDrop;
    [SerializeField] private TMP_Text startButtonText;
    [Space]
    [SerializeField] private uint width;
    [SerializeField] private uint height;
    private void Awake() {
        running = false;
        trailMap = new RenderTexture((int)width, (int)height, 0);
        trailMap.enableRandomWrite = true;
        trailMap.filterMode = FilterMode.Point;

        processedTrailMap = new RenderTexture((int)width, (int)height, 0);
        processedTrailMap.enableRandomWrite = true;
    }

    // Update is called once per frame
    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (running){
            DispatchProcessingCompute();       
            DispatchDataCompute(); 
        }
    }

    private void DispatchDataCompute(){
        ComputeBuffer agentsCB = new ComputeBuffer((int)agents.Length, 3 * sizeof(float));
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

        dataCompute.Dispatch(0, (int)agents.Length / 1000, 1, 1);

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
    private void DispatchClearCompute(){
        clearCompute.SetTexture(0, "Result", trailMap);
        clearCompute.Dispatch(0, (int)width / 32, (int)height / 32, 1);
        clearCompute.SetTexture(0, "Result", processedTrailMap);
        clearCompute.Dispatch(0, (int)width / 32, (int)height / 32, 1);
    }

    private void CreateAgents(uint count){
        agents = new Agent[agentCount];

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
    private float ParseFloat(string s, float failSafe){
        float value;
        if (float.TryParse(s, out value)){
            return value;
        }
        return failSafe;
    }
    public void StartStop(){
        if (!running){
            CreateAgents(agentCount);
            startButtonText.text = "Stop";
        }
        else{
            DispatchClearCompute();
            startButtonText.text = "Start";
        }
        running = !running;
    }  
    public void SetAgentCount(){
        agentCount = (uint)ParseFloat(agentField.text, agentCount);
    }
    public void SetSpeed(){
        speed = ParseFloat(speedField.text, speed);
    }
    public void SetDecaySpeed(){
        decaySpeed = ParseFloat(decaySpeedField.text, decaySpeed);
    }
    public void SetTurnSpeed(){
        turnSpeed = ParseFloat(turnSpeedField.text, turnSpeed);
    }
    public void SetSensorAngle(){
        sensorAngle = ParseFloat(sensorAngleField.text, sensorAngle);
    }
    public void SetSensorOffset(){
        sensorOffset = ParseFloat(sensorOffsetField.text, sensorOffset);
    }
    public void SetBound(){
        bound = ParseFloat(boundField.text, bound);
    }
    public void SetSpawnPos(){
        spawnPos = (SpawnPosition)spawnPosDrop.value;
    }
    public void SetSpawnDir(){
        spawnDir = (SpawnDirection)spawnDirDrop.value;
    }
    public void SetTrailMode(){
        trailMode = (trailMode)trailModeDrop.value;
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
    Circle,
    Square,
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