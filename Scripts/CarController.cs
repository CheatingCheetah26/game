using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using UnityEngine.Audio;

public class CarController : MonoBehaviour
{
    [System.Serializable]
    public class EngineSoundPart{
        public AudioSource audioSource;
        public float pitchOffset;
        public float speedPitchMultiplier;
        public float minRange;
        public float maxRange;
        public float blend;

        public EngineSoundPart(float pitchOffsetC,
                              float speedPitchMultiplierC, float minRangeC,
                               float maxRangeC, float blendC, AudioSource audioSourceC = null){

            audioSource = audioSourceC;
            pitchOffset = pitchOffsetC;
            speedPitchMultiplier = speedPitchMultiplierC;
            minRange = minRangeC;
            maxRange = maxRangeC;
            blend = blendC;

        }

        public float GetVolume(float rpm){
            if(rpm>=minRange && rpm<=maxRange){
                return 1;
            }else{
                float attenuated;
                if(rpm<minRange){
                    attenuated = 1 + (((rpm - minRange) * blend));
                }else{
                    attenuated = 1 + (((maxRange - rpm) * blend));
                }
                if(attenuated < 0){
                    attenuated = 0;
                }
                return attenuated;
            }
        }
    }

    

    public Texture2D newText;

    public GameObject[] wheelGameObjects = new GameObject[4];

    RaycastHit hit;
    public Tilemap gripTilemap;


    public float gasMaxPercentage;
    public float gasMinPercentage;

    public float brakeMaxPercentage;
    public float brakeTransitionMultiplier;

    public float steeringDeadzonePercentage;
    public float steeringSaturationPercentage;

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


    public EngineSoundPart lowRPM;
    public EngineSoundPart medRPM;
    public EngineSoundPart decLowRPM;
    public EngineSoundPart decMidRPM;

    public AudioSource gearChange;
    public AudioSource gearChangeDec;


    public float changeThreshold;

    public float accelInertia;
    public float accelInertiaAdder;

    public List<Gear> gears;
    public int currentGear;

    public AudioMixer audioMixer;

    public AnimationCurve curve;

    public Rigidbody2D rigidbody;

    public bool isInPit;

    public TrackDataHolder trackDataHolder;

    public UnityEngine.UI.Slider GI;
    public UnityEngine.UI.Slider BI;

    public CinemachineVirtualCamera camera;

    bool onGas = false;
    bool onBrake = false;
    bool onLeft = false;
    bool onRight = false;
    float gasVelocity;
    float steerVelocity;
    public float digitalSensitivity;

    // Start is called before the first frame update
    void Start()
    {
        trackDataHolder = GameObject.FindWithTag("TrackData").GetComponent<TrackDataHolder>();

        GI = GameObject.FindWithTag("UI_GI").GetComponent<UnityEngine.UI.Slider>();
        BI = GameObject.FindWithTag("UI_BI").GetComponent<UnityEngine.UI.Slider>();

        currentGear = 0;

        TilemapData tmd = TilemapData.Convert(gripTilemap);
        gripTilemap = tmd.Create(gripTilemap);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInPit)
        {
            //getting the player input
            switch (GamePreferences.controlScheme)
            {
                case 0:
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        //calculating the pressure on the gas pedal
                        if (touch.position.y < gasMaxPercentage * Screen.height)
                        {
                            if (touch.position.y < gasMinPercentage * Screen.height)
                            {
                                gasInput = 0;
                            }
                            else
                            {
                                float gasTouchPercentage = touch.position.y / Screen.height;
                                float oldMin = gasMinPercentage;
                                float oldMax = gasMaxPercentage;
                                float newMin = 0;
                                float newMax = 1;
                                float oldRange = oldMax - oldMin;
                                float newRange = newMax - newMin;
                                float correctedPercentage = (((gasTouchPercentage - oldMin) * newRange) / oldRange) + newMin;
                                gasInput = correctedPercentage;
                            }
                        }
                        else
                        {
                            gasInput = 1;
                        }

                        //calculating the pressure on the brake pedal
                        if (touch.position.y < brakeMaxPercentage * Screen.height)
                        {
                            float brakeTouchPercentage = touch.position.y / Screen.height;
                            float oldMin = brakeMaxPercentage;
                            float oldMax = 0f;
                            float newMin = 0;
                            float newMax = 1;
                            float oldRange = oldMax - oldMin;
                            float newRange = newMax - newMin;
                            float correctedPercentage = (((brakeTouchPercentage - oldMin) * newRange) / oldRange) + newMin;
                            brakeInput = correctedPercentage;
                        }
                        else
                        {
                            brakeInput = 0;
                        }


                        //calculating the steering angle
                        float touchPercentage = touch.position.x / Screen.width;
                        if (touchPercentage < steeringSaturationPercentage)
                        {
                            steeringInput = -1;
                        }
                        else
                        {
                            if (touchPercentage > 1 - steeringSaturationPercentage)
                            {
                                steeringInput = 1;
                            }
                            else
                            {
                                if (touchPercentage > 0.5 - steeringDeadzonePercentage && touchPercentage < 0.5 + steeringDeadzonePercentage)
                                {
                                    steeringInput = 0;
                                }
                                else
                                {
                                    if (touchPercentage > 0.5)
                                    {
                                        float oldMin = 0.5f + steeringDeadzonePercentage;
                                        float oldMax = 1 - steeringSaturationPercentage;
                                        float newMin = 0.5f;
                                        float newMax = 1;
                                        float oldRange = oldMax - oldMin;
                                        float newRange = newMax - newMin;
                                        float correctedPercentage = (((touchPercentage - oldMin) * newRange) / oldRange) + newMin;
                                        steeringInput = (correctedPercentage - 0.5f) * 2;
                                    }
                                    else
                                    {
                                        float oldMin = 0 + steeringSaturationPercentage;
                                        float oldMax = 0.5f - steeringDeadzonePercentage;
                                        float newMin = 0;
                                        float newMax = 0.5f;
                                        float oldRange = oldMax - oldMin;
                                        float newRange = newMax - newMin;
                                        float correctedPercentage = (((touchPercentage - oldMin) * newRange) / oldRange) + newMin;
                                        steeringInput = (correctedPercentage - 0.5f) * 2;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        gasInput = 0;
                        brakeInput = 0;
                        steeringInput = 0;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    if (!onGas)
                    {
                        gasInput = 0f;
                    }
                    else
                    {
                        gasInput = Mathf.SmoothDamp(gasInput, 1f, ref gasVelocity, digitalSensitivity);
                    }
                    if (!onBrake)
                    {
                        brakeInput = 0f;
                    }
                    else
                    {
                        brakeInput = Mathf.SmoothDamp(brakeInput, 1f, ref gasVelocity, digitalSensitivity);
                    }
                    if (!onLeft && !onRight)
                    {
                        steeringInput = 0f;
                    }
                    else
                    {
                        if (onLeft)
                        {
                            steeringInput = -1f;
                        }
                        else
                        {
                            steeringInput = 1f;
                        }
                    }
                    if (GamePreferences.autoBrake && !onGas)
                    {
                        brakeInput = Mathf.Min(1,0.1f+(speed/maxSpeed)/2);
                    }

                    break;
            }

            if (GamePreferences.usingController)
            {
                gasInput = (Input.GetAxis("Throttle") + 1) / 2;
                brakeInput = (Input.GetAxis("Brake") + 1) / 2;

                steeringInput = Input.GetAxis("Horizontal");
            }

            //getting the surface type under each wheel
            for (int i = 0; i < wheelGameObjects.Length; i++)
            {
                Vector3 wheelPosition = wheelGameObjects[i].transform.position;
                Sprite tileSprite = gripTilemap.GetSprite(Vector3Int.CeilToInt(wheelPosition - new Vector3(1, 1, 0)));
                Texture2D tileTex = TextureFromSprite(tileSprite);
                float xcoord = wheelPosition.x - (int)wheelPosition.x;
                float ycoord = wheelPosition.y - (int)wheelPosition.y;
                Color pointColor = tileTex.GetPixel((int)(32 - xcoord * -32), (int)(32 - ycoord * -32));
                Debug.DrawRay(wheelPosition, Vector3.forward, pointColor);
                Destroy(tileTex);
                if(pointColor == Color.green)
                {
                    
                    
                }
            }

            //old
            //calculating the translations and rotations to apply to the transform and applying them
            //if (gasInput > 0)
            //{
            //    float addedSpeed = (gasInput * accelerationMultiplier) / ((1 + maxSpeed) / (1 + speed));
            //    speed += (maxSpeed - speed) * gasInput * accelerationMultiplier;
            //}
            //else
            //{
            //    speed = ((speed / decelerationMultiplier) - brakeInput * brakeMultiplier);
            //}



            //rotation = -steeringInput * rotationMultiplier * speed;
            //if (rotation > maxGrip)
            //{
            //    rotation = maxGrip;
            //}
            //if (rotation < -maxGrip)
            //{
            //    rotation = -maxGrip;
            //}

            //if (rotation > 0)
            //{
            //    rotation = rotation * (1 - brakeInput) * brakeRotationMultiplier;
            //}
            //else
            //{
            //    rotation = rotation * (1 - brakeInput) * brakeRotationMultiplier;
            //}

            //rotation = rotation / (1 + gasInput * gasRotationMultiplier);

            //new

            speed = speed + ((gasInput*accelerationMultiplier) * (1 - (speed / maxSpeed)) * Time.deltaTime);
            speed = speed - (brakeInput * brakeMultiplier * Time.deltaTime);
            if(gasInput == 0f && brakeInput == 0f)
            {
                speed = speed * decelerationMultiplier;

            }
            rotation = (1 - (speed / maxSpeed)) * steeringInput * rotationMultiplier * -1f;
            rotation = rotation - (rotation * brakeInput * brakeRotationMultiplier * Time.deltaTime);


            //adding downforce
            if(speed/maxSpeed > 0.6f)
            {
                rotation = rotation + (rotation * ((speed / maxSpeed) - 0.6f))*5;
            }
            if (speed < 0f)
            {
                speed = 0f;
            }

            //UI used for debugging
            GI.value = gasInput;
            BI.value = brakeInput;

            //sound stuff

            float speedPercentage = speed / maxSpeed;

            float rpm = gears[currentGear].GetRpm(speedPercentage);
            if(rpm<0.4 && currentGear != 0)
            {
                currentGear--;
                gearChangeDec.Play();
            }
            else
            {
                if(rpm>0.8 && currentGear != gears.Count - 1)
                {
                    currentGear++;
                    gearChange.Play();
                }
            }


            lowRPM.audioSource.volume = lowRPM.GetVolume(rpm) * gasInput;
            lowRPM.audioSource.pitch = ((rpm * lowRPM.speedPitchMultiplier) + lowRPM.pitchOffset)- ((1 - gasInput) / 10)*(1-speed/maxSpeed);

            medRPM.audioSource.volume = medRPM.GetVolume(rpm) * gasInput;
            medRPM.audioSource.pitch = ((rpm * medRPM.speedPitchMultiplier) + medRPM.pitchOffset)-((1 - gasInput)/10) * (1 - speed / maxSpeed);

            
            decLowRPM.audioSource.volume = (1 - Mathf.Min(gasInput * 2f, 1f)) * decLowRPM.GetVolume(rpm) * (1 - gasInput);
            decLowRPM.audioSource.pitch = (rpm * decLowRPM.speedPitchMultiplier) + decLowRPM.pitchOffset;

            decMidRPM.audioSource.volume = (1 - Mathf.Min(gasInput * 2f, 1f)) * decMidRPM.GetVolume(rpm) * (1 - gasInput);
            decMidRPM.audioSource.pitch = (rpm * decMidRPM.speedPitchMultiplier) + decMidRPM.pitchOffset;
            

        }

    }

    private void FixedUpdate()
    {
        if (!isInPit)
        {
            rigidbody.MovePosition(rigidbody.position + new Vector2((transform.right * speed * Time.deltaTime).x, (transform.right * speed * Time.deltaTime).y));

            rigidbody.MoveRotation(rigidbody.rotation + rotation * Time.deltaTime);
        }

        audioMixer.SetFloat("AIVolume", -5+(speed / maxSpeed)*-20f);
    }

    private void LateUpdate()
    {
        camera.m_Lens.OrthographicSize = Mathf.Abs(speed / maxSpeed) + 0.5f;
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
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
        {
            return sprite.texture;
        }
    }

    public void StartPitStop(float stopTime, float laneSpeed)
    {
        StartCoroutine(PitStop(stopTime, laneSpeed));
    }

    IEnumerator PitStop(float stopTime, float laneSpeed)
    {
        isInPit = true;
        Debug.Log("I'm in pit stop mode");
        Pit pit = trackDataHolder.trackData.pit;
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
        while (isInPit)
        {
            yield return new WaitForFixedUpdate();
            transform.Translate(Vector3.right * Time.deltaTime * laneSpeed);
            Debug.Log("moving at " + Time.deltaTime * laneSpeed);
        }
        speed = laneSpeed;
    }

    public void OnGasDown(bool trueOrFalse)
    {
        onGas = trueOrFalse;
    }

    public void OnBrakeDown(bool trueOrFalse)
    {
        onBrake = trueOrFalse;
    }

    public void OnLeftDown(bool trueOrFalse)
    {
        onLeft = trueOrFalse;
    }

    public void OnRightDown(bool trueOrFalse)
    {
        onRight = trueOrFalse;
    }


    public void GetDigitalInput(string input,bool trueOrFalse)
    {
        switch (input)
        {
            case "gas":
                OnGasDown(trueOrFalse);
                break;

            case "brake":
                OnBrakeDown(trueOrFalse);
                break;

            case "left":
                OnLeftDown(trueOrFalse);
                break;

            case "right":
                OnRightDown(trueOrFalse);
                break;
        }
    }
}

[System.Serializable]
public class Gear
{
    public float demultiplication;

    public float GetRpm(float speed)
    {
        return speed * demultiplication;
    }
}