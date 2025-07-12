using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class AvatarController : MonoBehaviour
{
    [SerializeField] private Transform right_hand_target;
    [SerializeField] private Transform right_hand_forearm;
    [SerializeField] private Transform left_hand_target;
    [SerializeField] private Transform left_hand_forearm;
    [SerializeField] private SphereCollider right_arm_span;
    [SerializeField] private SphereCollider left_arm_span;

    public float move_spd = 1f;
    // variables to track if given position or rotation exceeds avatar movement (to reduce sparsity)
    public bool has_over_moved = false;
    public bool has_over_rotated = false;

    // Start is called before the first frame update    
    void Start()
    {
        //check for serialized fields
        if (right_hand_target == null || left_hand_target == null || right_hand_forearm == null || left_hand_forearm == null)
            Debug.Log("Please drag the right/left hand targets to AvatarController script");

        //Rotate_hand(100, 100, 350, true);
        // Move_hand(100, 20, 10, true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public (float, float, float) Rotate_hand(float x_angle, float y_angle, float z_angle, bool is_right_hand)
    {
        has_over_rotated = false;

        //check if within human bounds
        if (Math.Abs(x_angle) > 90f)
        {
            has_over_rotated = true;
            //set as max rotation
            if (x_angle > 0) x_angle = 90f;
            else x_angle = -90f;
        }
        if (Math.Abs(y_angle) > 90f)
        {
            has_over_rotated = true;
            //set as max rotation
            if (y_angle > 0) y_angle = 90f;
            else y_angle = -90f;
        }
        if (Math.Abs(z_angle) > 90f)
        {
            has_over_rotated = true;
            //set as max rotation
            if (z_angle > 0) z_angle = 90f;
            else z_angle = -90f;
        }


        Vector3 r_current_rotation = right_hand_target.transform.eulerAngles;
        Vector3 l_current_rotation = left_hand_target.transform.eulerAngles;

        //ASSUMING T POSE
        if (is_right_hand)
        {
            //rotate the hand based on current position
            r_current_rotation.x += x_angle;
            r_current_rotation.y += y_angle;
            r_current_rotation.z += z_angle;
            right_hand_target.transform.eulerAngles = r_current_rotation;
        }
        else
        {
            //rotate hand based on current position
            l_current_rotation.x += x_angle;
            l_current_rotation.y += y_angle;
            l_current_rotation.z += z_angle;
            left_hand_target.transform.eulerAngles = l_current_rotation;
        }

        return (x_angle, y_angle, z_angle);
    }

    public (float, float, float) Move_hand(float x_pos, float y_pos, float z_pos, bool is_right_hand)
    {
        has_over_moved = false;

        //find which one is the arm span limitation
        SphereCollider arm_span = is_right_hand ? right_arm_span : left_arm_span;
        //CapsuleCollider forbidden_zone = is_right_hand ? right_forbidden_zone : left_forbidden_zone;
        Transform target = is_right_hand ? right_hand_target : left_hand_target;

        //calculate the center
        Transform center_transform = arm_span.transform;
        Vector3 center = center_transform.position + arm_span.center;

        //get the radius of the arm limit
        float radius = arm_span.radius * center_transform.lossyScale.x;

        Vector3 offset = target.position - center;

        //check if outside the sphere
        if (offset.magnitude > radius)
        {
            //clamp position to surface of sphere
            target.position = center + offset.normalized * radius;
            has_over_moved = true;
        }

        return (target.position.x, target.position.y, target.position.z);
    }

}
