using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrackData
{
    public Transform track;
    public CapsuleCollider collider;
    //public float rotationSpeed;
    //public float targetRotation;
    //public float currentRotation;
    public float targetSpeed;
    public float currentSpeed;
    public float realSpeed;
    public bool isGrounded;
}

public class TankController : MonoBehaviour
{
    public enum Mode
    {
        ApplyForce,
        ApplyForceOnTracks,
        ApplyVelocity,
    }

    public Rigidbody tankRigidbody;
    public float maxSpeed = 2.0f;
    public float minSpeed = 0.24f;
    public float maxSpeedReverse = 1.0f;
    public float turnSpeed = 1.0f;
    public float minTorque = 0.0f;
    public float trackAcceleration = 1.0f;
    public float speed2AngleRatio = 1.0f; 
    public float maxAngularVelocity = 1f;
    public float areoDownForce = 1.0f;

    public TrackData leftTrack;
    public TrackData rightTrack;
    public float trackDistance = 1.0f;
    public float curSpeed = 0.0f;
    public float forceMultiplier = 0.5f;
    [SerializeField,Range(-1, 1)]
    private float verticalInput;
    [SerializeField,Range(-1, 1)]
    private float horizontalInput;

    public float angleVelocity;

    public Mode mode = Mode.ApplyForce;

    private Average averageSpeed = new Average(10);
    private Average averageAngle = new Average(10);

    void Start()
    {
        tankRigidbody = GetComponent<Rigidbody>();
        trackDistance = Vector3.Distance(leftTrack.track.position, rightTrack.track.position);

    }

    private void Update()
    {
        var targetSpeedLeft = 0f;
        var targetSpeedRight = 0f;

        if (verticalInput > 0)
        {
            targetSpeedLeft = maxSpeed * verticalInput;
            targetSpeedRight = maxSpeed * verticalInput;
        }
        else if (verticalInput < 0)
        {
            targetSpeedLeft = maxSpeedReverse * verticalInput;
            targetSpeedRight = maxSpeedReverse * verticalInput;
        }

        targetSpeedLeft += turnSpeed * horizontalInput;
        targetSpeedRight -= turnSpeed * horizontalInput;

        //Clamp the speed from -maxSpeedReverse to maxSpeed
        targetSpeedLeft = Mathf.Clamp(targetSpeedLeft, -maxSpeedReverse, maxSpeed);
        targetSpeedRight = Mathf.Clamp(targetSpeedRight, -maxSpeedReverse, maxSpeed);

        leftTrack.targetSpeed = targetSpeedLeft;
        rightTrack.targetSpeed = targetSpeedRight;
    }

    private void FixedUpdate()
    {
        turnSpeed = trackDistance * maxAngularVelocity / 2;
        curSpeed = averageSpeed.Add(tankRigidbody.velocity.magnitude).GetAverage();
        angleVelocity = averageAngle.Add(tankRigidbody.angularVelocity.y).GetAverage();

        var angleSpeed = tankRigidbody.angularVelocity.y;
        UpdateTrack(leftTrack, angleSpeed);
        UpdateTrack(rightTrack, -angleSpeed);

        tankRigidbody.AddTorque(Vector3.up * (horizontalInput * minTorque * tankRigidbody.mass / Time.fixedDeltaTime));
        
        if (mode == Mode.ApplyForce)
        {
            ApplyForce();
        }
        else if (mode == Mode.ApplyVelocity)
        {
            ApplyVelocity();
        }

        ////gravity
        //tankRigidbody.AddForce(Physics.gravity * tankRigidbody.mass);

        ////apply down force
        //var sqrVelocity = tankRigidbody.velocity.sqrMagnitude;
        //tankRigidbody.AddForce(Vector3.down * areoDownForce * sqrVelocity);
    }

    private void ApplyForce()
    {
        Vector3 velocity = Vector3.zero;
        float turnAngle = 0.0f;
        //distance between two tracks
        //var distance = Vector3.Distance(leftTrack.track.position, rightTrack.track.position);
        var leftSpeed = leftTrack.currentSpeed;
        var rightSpeed = rightTrack.currentSpeed;

        //difference in speed between the two tracks
        var speedDiff = leftSpeed - rightSpeed;
        //calculate the turn angle
        turnAngle = speedDiff * speed2AngleRatio * Time.fixedDeltaTime;

        //calculate the average speed of the two tracks
        var avgSpeed = (leftSpeed + rightSpeed) * 0.5f;

        //calculate the velocity vector
        velocity = transform.forward * avgSpeed;
        var curVelocity = tankRigidbody.velocity;
        //calculate difference between the current velocity and the target velocity
        var velocityDiff = velocity - curVelocity;
        //calculate the force to apply to the tank
        var force = velocityDiff * tankRigidbody.mass / Time.fixedDeltaTime;
        //apply the force to the tank
        tankRigidbody.AddForce(force);
        //apply gravity
        transform.Rotate(Vector3.up, turnAngle);
        //apply the velocity to the rigidbody
        //tankRigidbody.velocity = velocity;
    }

    private void ApplyVelocity()
    {
        //var distance = Vector3.Distance(leftTrack.track.position, rightTrack.track.position);
        var leftSpeed = leftTrack.currentSpeed;
        var rightSpeed = rightTrack.currentSpeed;
        //caculate radius by the distance between two tracks and the speed difference
        var radius = 0f;
        if(leftSpeed != rightSpeed)
        {
            radius = trackDistance * (leftSpeed + rightSpeed) / (2 * (leftSpeed - rightSpeed));
        }
        var avgSpeed = (leftSpeed + rightSpeed) * 0.5f;
        //calculate the angular velocity in degrees per second
        var angularVelocity = (leftSpeed - rightSpeed) / trackDistance;

        //calculate the velocity vector
        var velocity = transform.forward * avgSpeed;
        //Debug.Log("velocity: " + velocity + "  speed:" + avgSpeed + "  magnitude" + velocity.magnitude);
        ////calculate the velocity vector at the center of the tank
        //var centerVelocity = transform.forward * angularVelocity * radius;
        ////calculate the final velocity vector
        //velocity += centerVelocity;
        ////apply the velocity to the rigidbody
        tankRigidbody.velocity = velocity;
        //rotate the tank
        transform.Rotate(Vector3.up, angularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg);
    }

    private void UpdateTrack(TrackData data,float angleSpeed)
    {
        data.currentSpeed = Mathf.MoveTowards(data.currentSpeed, data.targetSpeed, Time.fixedDeltaTime * trackAcceleration);
        //check if the track is grounded
        if (Physics.Raycast(data.collider.transform.position, -data.track.up * 0.6f, out RaycastHit hit, 1.0f))
        {
            //calculate the rotation of the track
            //var rotation = Quaternion.LookRotation(Vector3.Cross(hit.normal, track.track.forward), hit.normal);
            //rotate the track
            //track.track.rotation = Quaternion.RotateTowards(track.track.rotation, rotation, track.rotationSpeed * Time.fixedDeltaTime);
        
            data.isGrounded = true;

            if (mode == Mode.ApplyForceOnTracks)
            {
                var rig = tankRigidbody;
                //var rig = track.track.GetComponentInChildren<Rigidbody>();
                //calculate the velocity vector
                var curVelocity = rig.velocity + angleSpeed * trackDistance * data.track.forward * 0.5f;
                data.realSpeed = Vector3.Dot(data.track.forward, curVelocity);
                //curVelocity = Vector3.Project(curVelocity, data.track.forward);
                //ust use the track's forward vector to calculate the velocity
                var trackVelocity = transform.forward * data.currentSpeed;
                //caculate the velocity difference by trackDistance , anglarVelocity and trackVelocity

                //var dot = Vector3.Dot(curVelocity, data.track.forward);
                //if(dot > minSpeed)
                //{
                //    curVelocity -= dot * data.track.forward * minSpeed;
                //}
                //else if(dot < -minSpeed)
                //{
                //    curVelocity += dot * data.track.forward * minSpeed;
                //}
                //curVelocity -= (dot > 0 ? 1 : -1) * data.track.forward * minSpeed;

                var velocityDiff = trackVelocity - curVelocity;
                velocityDiff = Vector3.Project(velocityDiff, data.track.forward);

                //var dot = Vector3.Dot(velocityDiff, data.track.forward);
                //velocityDiff += dot * data.track.forward * minSpeed;
                velocityDiff += velocityDiff.normalized * minSpeed; //(dot > 0 ? 1 : -1) * data.track.forward * minSpeed;

                //计算volocityDiff在track上的投影
                //velocityDiff = Vector3.Project(velocityDiff, track.track.forward);
                var force = velocityDiff * tankRigidbody.mass / Time.fixedDeltaTime;
                tankRigidbody.AddForceAtPosition(force * forceMultiplier, data.track.position);
                //tankRigidbody.AddForceAtPosition(force * 0.5f, data.track.position);

            }

        }
        else
        {
            data.isGrounded = false;
            //apply gravity on the track
            tankRigidbody.AddForceAtPosition(Physics.gravity * tankRigidbody.mass * 0.5f, data.track.position);
        }

        //var currentSpeed = track.currentSpeed;
        //var delta = currentSpeed - this.curSpeed;
        ////Calculate force to apply to the track
        //var force = delta * trackAcceleration;
        ////Apply the force to the track
        //tankRigidbody.AddForceAtPosition(track.track.forward * force, track.track.position);
    }

    public void SetVerticleInput(float value)
    {
        verticalInput = value;
    }

    public void SetHorizontalInput(float value)
    {
        horizontalInput = value;
    }

    public void SetLinerInput(float value)
    {
        if(value == 0)
        {
            verticalInput = 0;
        }
        else if(value > 0)
        {
            verticalInput = 1;
            maxSpeed = value;
        }
        else
        {
            verticalInput = -1;
            maxSpeedReverse = value;
        }
    }

    public void SetAngularInput(float value)
    {
        if (value == 0)
        {
            horizontalInput = 0;
        }
        else if (value > 0)
        {
            horizontalInput = 1;
            maxAngularVelocity = value;
        }
        else
        {
            horizontalInput = -1;
            maxAngularVelocity = value;
        }
    }

    private void OnDrawGizmos()
    {
        if (leftTrack != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(leftTrack.track.position, leftTrack.track.position + leftTrack.track.forward * leftTrack.currentSpeed);
        }
        if (rightTrack != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rightTrack.track.position, rightTrack.track.position + rightTrack.track.forward * rightTrack.currentSpeed);
        }
    }
}


public class Average
{
    private Queue<float> queue = new Queue<float>();

    public int Size { get; private set; }

    public Average(int size)
    {
        Size = size;
    }

    public Average Add(float value)
    {
        queue.Enqueue(value);
        if (queue.Count > Size)
        {
            queue.Dequeue();
        }
        return this;
    }

    public float GetAverage()
    {
        float sum = 0;
        foreach (var item in queue)
        {
            sum += item;
        }
        return sum / queue.Count;
    }
}
