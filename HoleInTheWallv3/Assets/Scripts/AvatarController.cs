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

    public float move_spd = 1f;                                                         // speed at which the hand moves toward destination
    public float rotate_spd = 180f;                                                     // speed at which the hand rotates toward destination
    public float ik_tolerance = 0.02f;                                                  // the distance of target from forearm allowed
    public float ik_stall_tolerance = 0.001f;                                           // the minimum movement required for target to move forearm. if no more movement, reached maximum transform/rotation

    // variables to track if given position or rotation exceeds avatar movement (to reduce sparsity)
    public bool has_over_moved = false;
    public bool has_over_rotated = false;

    // Start is called before the first frame update    
    void Start()
    {
        //check for serialized fields
        if (right_hand_target == null || left_hand_target == null || right_hand_forearm == null || left_hand_forearm == null)
            Debug.Log("Please drag the right/left hand targets to AvatarController script");

        // Rotate_hand(100, 100, 0, true);
        // Move_hand(100, 20, 10, true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public (float, float, float) Rotate_Hand(float x_angle, float y_angle, float z_angle, bool is_right_hand)
    {
        Quaternion desired_rotation = Quaternion.Euler(x_angle, y_angle, z_angle);

        //different variables with different arms
        if (is_right_hand)
        {
            while (true)
            {
                right_hand_target.rotation = Quaternion.RotateTowards(right_hand_target.rotation, desired_rotation, rotate_spd * Time.deltaTime);

                //stop movement once the rotation is maxed
                if (Quaternion.Angle(right_hand_target.rotation, desired_rotation) < 1f)
                {
                    // overextended if rotation != desired rotation
                    has_over_rotated = right_hand_target.rotation != desired_rotation;
                    return (right_hand_target.rotation.x, right_hand_target.rotation.y, right_hand_target.rotation.z);
                }
            }
        }
        else
        {
            while (true)
            {
                left_hand_target.rotation = Quaternion.RotateTowards(left_hand_target.rotation, desired_rotation, rotate_spd * Time.deltaTime);
                if (Quaternion.Angle(left_hand_target.rotation, desired_rotation) < 1f)
                {
                    has_over_rotated = left_hand_target.rotation != desired_rotation;
                    return (left_hand_target.rotation.x, left_hand_target.rotation.y, left_hand_target.rotation.z);
                }
            }
        }
    }

    public (float, float, float) Move_Hand(float x_pos, float y_pos, float z_pos, bool is_right_hand)
    {
        return (0f,0f,0f);

    }

    //move head
    //move body


    // //rotate the hand. If true, then rotate right hand. else rotate left hand.
    // public (float, float, float) Rotate_hand(float xAngle, float yAngle, float zAngle, bool isRight)
    // {
    //     //rotate the hands
    //     if (isRight) right_hand_target.Rotate(xAngle, yAngle, zAngle);
    //     else left_hand_target.transform.Rotate(xAngle, yAngle, zAngle);

    //     return (xAngle, yAngle, zAngle);
    // }

    // //move the hand. If true, then rotate the right hand. Else rotate the left hand
    // public (float, float, float) Move_hand(float xPos, float yPos, float zPos, bool isRight)
    // {
    //     //move the hands
    //     if (isRight) right_hand_target.position = new(xPos, yPos, zPos);
    //     else left_hand_target.position = new(xPos, yPos, zPos);

    //     return (xPos, yPos, zPos);
    // }
}
