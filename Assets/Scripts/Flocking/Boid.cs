using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody rb;

    public Thing host;

    Vector3 finalForce;
    float maxforce;    // Maximum steering force
    float maxSpeed;    // Maximum speed

    void Start()
    {

        rb = gameObject.AddComponent<Rigidbody>();



        //setup flocking parameters
        finalForce = new Vector3(0, 0);
        rb.velocity = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1));

        maxSpeed = 5;
        maxforce = 0.5f;

        rb.mass = host.settings.mass;
        rb.drag = host.settings.drag;
    }



    // We accumulate a new acceleration each time based on three rules
    void flock(List<Boid> boids)
    {
        Vector3 sep = Separate(boids);   // Separation
        Vector3 ali = Align(boids);      // Alignment
        Vector3 coh = Cohesion(boids);   // Cohesion

        // Arbitrarily weight these forces
        sep *= 1.5f;
        ali *= 1.0f;
        coh *= 1.0f;

        // Add the force vectors to acceleration
        finalForce += sep;
        finalForce += ali;
        finalForce += coh;

    }

    // Method to update transform.position
    void Update()
    {
        flock(ThingManager.main.boids);
        rb.AddForce(finalForce);

        //limit max speed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        float rotationSmoothSpeed = 3.14f / 2f;
        Quaternion targetRotation = Quaternion.LookRotation(rb.velocity.normalized);
        Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothSpeed);
        rb.rotation = newRotation;
    }

    // A method that calculates and applies a steering force towards a target
    // STEER = DESIRED MINUS VELOCITY
    Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - transform.position;  // A vector pointing from the transform.position to the target
                                                        // Normalize desired and scale to maximum speed
        desired.Normalize();
        desired *= maxSpeed;
        // Steering = Desired minus Velocity
        Vector3 steer = desired - rb.velocity;
        while (steer.magnitude > maxforce)
        {
            steer *= 0.9f;
        }
        return steer;
    }

    // Separation
    // Method checks for nearby boids and steers away
    Vector3 Separate(List<Boid> boids)
    {
        float desiredseparation = 25.0f;
        Vector3 steer = new Vector3(0, 0, 0);
        int count = 0;
        // For every boid in the system, check if it's too close
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(transform.position, other.transform.position);
            // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
            if ((d > 0) && (d < desiredseparation))
            {
                // Calculate vector pointing away from neighbor
                Vector3 diff = transform.position - other.transform.position;
                diff.Normalize();
                diff /= d;        // Weight by distance
                steer += diff;
                count++;            // Keep track of how many
            }
        }

        // Average -- divide by how many
        if (count > 0)
        {
            steer /= (float)count;
        }

        // As long as the vector is greater than 0
        if (steer.magnitude > 0)
        {
            // Implement Reynolds: Steering = Desired - Velocity
            steer.Normalize();
            steer *= maxSpeed;
            steer -= rb.velocity;
            while (steer.magnitude > maxforce)
            {
                steer *= 0.9f;
            }
        }
        return steer;
    }

    // Alignment
    // For every nearby boid in the system, calculate the average velocity
    Vector3 Align(List<Boid> boids)
    {
        float neighbordist = 50;
        Vector3 sum = new Vector3(0, 0);
        int count = 0;

        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(transform.position, other.transform.position);
            if ((d > 0) && (d < neighbordist))
            {
                sum += other.rb.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= (float)count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector3 steer = sum - rb.velocity;
            while (steer.magnitude > maxforce)
            {
                steer *= 0.9f;
            }
            return steer;
        }
        else
        {
            return new Vector3(0, 0);
        }
    }

    // Cohesion
    // For the average transform.position (i.e. center) of all nearby boids, calculate steering vector towards that transform.position
    Vector3 Cohesion(List<Boid> boids)
    {
        float neighbordist = 50;
        Vector3 sum = new Vector3(0, 0);   // Start with empty vector to accumulate all transform.positions
        int count = 0;
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(transform.position, other.transform.position);
            if ((d > 0) && (d < neighbordist))
            {
                sum += other.transform.position; // Add transform.position
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            return Seek(sum);  // Steer towards the transform.position
        }
        else
        {
            return new Vector3(0, 0);
        }
    }


}
