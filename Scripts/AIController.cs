using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIController : MonoBehaviour
{
    public Texture2D newText;

    public GameObject[] wheelGameObjects = new GameObject[4];

    RaycastHit hit;
    public Tilemap gripTilemap;


    float gasInput;
    float brakeInput;
    float steeringInput;

    public float maxSpeed;
    public float accelerationMultiplier;
    public float decelerationMultiplier;
    public float brakeMultiplier;
    public float steeringDecelerationMultiplier;

    public float maxGrip;
    public float gasRotationMultiplier;
    public float brakeRotationMultiplier;
    public float rotationMultiplier;

    float speed;
    float rotation;

    TrackDataHolder trackDataHolder;
    int wpObjective;
    public LayerMask AImask;
    public float sideDistance;
    public float frontDistance;
    public float rearDistance;
    bool leftOrRight;
    float randomCorneringSpeed;
    RaycastHit2D frontHit;
    RaycastHit2D leftHit;
    RaycastHit2D rightHit;
    RaycastHit2D rearHit;

    public AudioSource audioSource;
    public AudioSource audioClose;
    public AudioSource audioFar;
    public float engineSoundMultiplier;
    public float engineSoundOffset;
    public List<Gear> gears;
    public int currentGear;
    public float oldDistance;

    public int state;//0: racing  1: aiming pitlane  2: init pitlane  3: in pitlane coroutine

    public bool pitThisLap;

    public float optimizationDistance;
    public Rigidbody2D rigidbody;

    bool isFarFromPlayer = true;

    float distanceToPlayer;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        trackDataHolder = GameObject.FindWithTag("TrackData").GetComponent<TrackDataHolder>();
        gripTilemap = GameObject.FindWithTag("TilemapGrip").GetComponent<Tilemap>();
        StartCoroutine(LeftOrRightChanger());
        StartCoroutine(RandomCornering());
        engineSoundOffset += Random.Range(-0.05f,0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        //AI stuff

        if(distanceToPlayer > optimizationDistance){
            if(isFarFromPlayer == false){
                isFarFromPlayer = true;
                Debug.Log("I'm far from player. Optimizing...");
                rigidbody.isKinematic = true;

                audioFar.enabled = true;
                audioFar.time = Random.RandomRange(0f, 10f);
                audioFar.Play();
            }
        }else{
            if (isFarFromPlayer == true)
            {
                Debug.Log("I'm close to player");
                isFarFromPlayer = false;
                rigidbody.isKinematic = false;

                audioFar.enabled = false;
            }
        }

        WayPoint wayPoint = new WayPoint();
        Vector3 wpCoord = new Vector3();
        Vector3 relative = new Vector3();

        if (state == 0)
        {
            wayPoint = CommonReferences.trackData.wayPoints[wpObjective];
        }
        else
        {
            if(state == 1)
            {
                if (pitThisLap)
                {
                    wayPoint = CommonReferences.trackData.pit.entryWayPoint;
                }
                else
                {
                    state = 0;
                    wayPoint = CommonReferences.trackData.wayPoints[wpObjective];
                }
            }
            else
            {
                if(state == 2)
                {
                    StartCoroutine(PitStop(3,0.4f));
                }
            }
        }

        if(state != 3 && CommonReferences.sessionManager.hasRaceStarted)
        {
            wpCoord = wayPoint.pos;
            relative = transform.InverseTransformPoint(wpCoord);
            float angle = Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg;//angle is 90° when waypoint is on the left and -90° when waypoint is on the right
            if (angle > 1f)
            {
                steeringInput = -1f;
            }
            else
            {
                if (angle < -1f)
                {
                    steeringInput = 1f;
                }
                else
                {
                    steeringInput = -angle / 1f;
                }
            }
            float speedPercentage = speed / maxSpeed;
            float impreciseSpeed;
            if (wayPoint.idealSpeed == 1f)
            {
                impreciseSpeed = wayPoint.idealSpeed;
            }
            else
            {
                impreciseSpeed = wayPoint.idealSpeed * ((1+GamePreferences.difficulty)/2) * randomCorneringSpeed * 0.9f;
            }
            if (impreciseSpeed < speedPercentage)
            {
                gasInput = 0f;
                brakeInput = speedPercentage - impreciseSpeed + 0.2f;
            }
            else
            {
                gasInput = Mathf.Max(1 - (Mathf.Abs(angle) / 90f*1.5f), 0f);
                brakeInput = 0f;
            }

            frontHit = Physics2D.CircleCast(transform.position+transform.right*0.075f, 0.04f, transform.right, frontDistance, AImask);
            leftHit = Physics2D.CircleCast(transform.position + transform.right * 0.075f + transform.right * 0.02f, 0.05f, transform.up, sideDistance, AImask);
            rightHit = Physics2D.CircleCast(transform.position + transform.right * 0.075f + transform.right * 0.02f, 0.04f, -transform.up, sideDistance, AImask);

            RaycastHit2D colHit = Physics2D.Raycast(transform.position, transform.right, frontDistance / 3, AImask);

            bool fronthitBool = false;
            bool leftHitBool = false;
            bool rightHitBool = false;

            if (frontHit.collider != null)
            {
                fronthitBool = true;
            }
            if (leftHit.collider != null)
            {
                leftHitBool = true;
            }
            if (rightHit.collider != null)
            {
                rightHitBool = true;
            }

            if (fronthitBool == true && leftHitBool == false && rightHitBool == false)
            {
                if (Vector2.Distance(transform.position, wayPoint.pos) > 2 && wayPoint.canOvertake)
                {
                    if (leftOrRight)
                    {
                        if (steeringInput < 0.25f)
                        {
                            steeringInput = Mathf.Lerp(1f, -steeringInput, frontHit.distance / frontDistance);
                        }
                    }
                    else
                    {
                        if (steeringInput > -0.25f)
                        {
                            steeringInput = Mathf.Lerp(-1f, -steeringInput, frontHit.distance / frontDistance);
                        }
                    }
                }
            }
            else
            {
                if (leftHitBool && rightHitBool)
                {

                }
                else
                {
                    if (leftHitBool)
                    {
                        if (steeringInput != -1f || wayPoint.idealSpeed == 1)
                        {
                            steeringInput = Mathf.Lerp(1f, steeringInput, leftHit.distance / sideDistance * 2f);
                        }
                        else
                        {
                            gasInput = 0f;
                            steeringInput = Mathf.Lerp(-0.5f, steeringInput, leftHit.distance / sideDistance * 2f);
                        }
                    }
                    else
                    {
                        if (rightHitBool)
                        {
                            if (steeringInput != 1f || wayPoint.idealSpeed == 1)
                            {
                                steeringInput = Mathf.Lerp(-1f, steeringInput, rightHit.distance / sideDistance * 2f);
                            }
                            else
                            {
                                gasInput = 0f;
                                steeringInput = Mathf.Lerp(0.5f, steeringInput, rightHit.distance / sideDistance * 2f);
                            }
                        }
                    }
                }
            }

            if (colHit.collider != null)
            {
                if (leftOrRight)
                {
                    if (steeringInput > -0.25f)
                    {

                    }
                    steeringInput = Mathf.Lerp(1f, steeringInput, colHit.distance / frontDistance / 3);
                }
                else
                {
                    if (steeringInput < 0.25f)
                    {

                    }
                    steeringInput = Mathf.Lerp(-1f, steeringInput, colHit.distance / frontDistance / 3);
                }
                gasInput = 0f;
                brakeInput = 0.2f;
            }


            //getting the surface type under each wheel
            if (!isFarFromPlayer)
            {
                for (int i = 0; i < wheelGameObjects.Length; i++)
                {
                    Color color = GetGripColorAt(wheelGameObjects[i].transform.position);
                    if(color == Color.green)
                    {
                        //speed = speed / 1.005f;
                    }
                }
            }

            //calculating the translations and rotations to apply to the transform and applying them
            speed = speed + ((gasInput * accelerationMultiplier) * (1 - (speed / maxSpeed)) * Time.deltaTime);
            speed = speed - (brakeInput * brakeMultiplier * Time.deltaTime);
            
            rotation = Mathf.Max(1 - (speed / maxSpeed), 0.3f) * steeringInput * rotationMultiplier * -1f;
            rotation = rotation - (rotation * brakeInput * brakeRotationMultiplier * Time.deltaTime);

            if (gasInput == 0f && brakeInput == 0f)
            {
                speed = speed * decelerationMultiplier;
                rotation = rotation * 1.25f;
            }

            if (speed / maxSpeed > 0.6f)
            {
                //rotation = rotation + (rotation * ((speed / maxSpeed) - 0.6f)) * 5;
            }

            if(GamePreferences.difficulty < 1)
            {
                rotation = rotation * ((2 + GamePreferences.difficulty) / 3f);
            }
            else
            {
                rotation = rotation * ((1 + GamePreferences.difficulty) / 2f);
            }
        }
    }

    private void FixedUpdate()
    {
        if(state != 3)
        {
            rigidbody.MovePosition(rigidbody.position + new Vector2((transform.right * speed * Time.deltaTime).x, (transform.right * speed * Time.deltaTime).y));

            rigidbody.MoveRotation(rigidbody.rotation + rotation * Time.deltaTime);
        }

        distanceToPlayer = trackDataHolder.DistanceToPlayer(transform.position);

        float speedPercentage = speed / maxSpeed;

        float rpm = gears[currentGear].GetRpm(speedPercentage);
        if (rpm < 0.4 && currentGear != 0)
        {
            currentGear--;
        }
        else
        {
            if (rpm > 0.8 && currentGear != gears.Count - 1)
            {
                currentGear++;
            }
        }

        if (!isFarFromPlayer)
        {
            audioSource.pitch = (rpm * engineSoundMultiplier) + engineSoundOffset;
            audioSource.pitch = audioSource.pitch * (1 + (oldDistance - distanceToPlayer) * 10);

            audioClose.pitch = (rpm * engineSoundMultiplier) + engineSoundOffset;
            audioClose.pitch = audioClose.pitch * (1 + (oldDistance - distanceToPlayer) * 10);

            audioClose.volume = Mathf.Max(1 - ((oldDistance - distanceToPlayer) * 3), 0f) * Mathf.Max(1 - (distanceToPlayer / 2), 0f)+0.3f;
            audioSource.volume = 1 - audioClose.volume;
        }
        else
        {
            audioClose.volume = 0f;
            audioSource.volume = 0f;
        }

        oldDistance = distanceToPlayer;
    }


    public Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.name = "newText";
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
        {
            return sprite.texture;
        }
    }

    public Color GetGripColorAt(Vector3 position){
        Sprite tileSprite = gripTilemap.GetSprite(Vector3Int.CeilToInt(position - new Vector3(1, 1, 0)));
        Texture2D tileTex = TextureFromSprite(tileSprite);
        tileTex.name = "tileTex";
        float xcoord = position.x - (int)position.x;
        float ycoord = position.y - (int)position.y;
        Color pointColor = tileTex.GetPixel((int)(32 - xcoord * -32), (int)(32 - ycoord * -32));
        Debug.DrawRay(position, Vector3.forward, pointColor);
        Destroy(tileTex);
        return pointColor;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * gasInput);
        if(Mathf.Abs(steeringInput)!=1f){
            Gizmos.color = Color.blue;
        }
        Gizmos.DrawLine(transform.position, transform.position + transform.up * -steeringInput/2);

    }

    public void IncrementWayPoint(int index){
        int futureWayPoint = index + 1;
        if(futureWayPoint > CommonReferences.trackData.wayPoints.Count-1){
            futureWayPoint = 0;
        }
        wpObjective = futureWayPoint;
    }

    IEnumerator LeftOrRightChanger(){
        while(true){
            yield return new WaitForSeconds(Random.Range(1, 3));
            if(Random.value>0.5){
                leftOrRight = true;
            }else{
                leftOrRight = false;
            }
        }
    }

    IEnumerator RandomCornering()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 3));
            randomCorneringSpeed = Random.Range(0.85f, 1.05f);
        }
    }

    IEnumerator PitStop(float stopTime, float laneSpeed)
    {
        state = 3;
        Debug.Log("I'm in pit stop mode");
        Pit pit = CommonReferences.trackData.pit;
        transform.position = pit.entryWayPoint.pos;
        transform.rotation = Quaternion.Euler(0, 0, pit.angle);
        Debug.Log(Vector2.Distance(transform.position, trackDataHolder.GetPitPos(3)));
        while (Vector2.Distance(transform.position, trackDataHolder.GetPitPos(3)) > 0.1f)
        {
            yield return new WaitForFixedUpdate();
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            Debug.Log("moving at " + Time.deltaTime * laneSpeed);
        }
        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForFixedUpdate();
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            transform.Rotate(Vector3.forward * -1f);
        }
        for (int i = 0; i < 40; i++)
        {
            //yield return new WaitForEndOfFrame();
            yield return null;
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            transform.Rotate(Vector3.back * -1f);
        }
        Debug.Log("Stop !");
        yield return new WaitForSeconds(stopTime);
        Debug.Log("Gogogo !");
        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForFixedUpdate();
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            transform.Rotate(Vector3.back * -1f);
        }
        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForFixedUpdate();
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            transform.Rotate(Vector3.forward * -1f);
        }
        while (state == 3)
        {
            yield return new WaitForFixedUpdate();
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            Debug.Log("moving at " + Time.deltaTime * laneSpeed);
        }
        wpObjective = pit.indexAfterPit;
        speed = laneSpeed;
    }
}
