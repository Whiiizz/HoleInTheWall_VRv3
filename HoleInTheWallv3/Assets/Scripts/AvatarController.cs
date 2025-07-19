using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class AvatarController : MonoBehaviour
{
    [SerializeField] private Transform right_hand_target;
    [SerializeField] private Transform right_hand_forearm;
    [SerializeField] private Transform left_hand_target;
    [SerializeField] private Transform left_hand_forearm;
    [SerializeField] private SphereCollider right_arm_span;
    [SerializeField] private SphereCollider left_arm_span;
    [SerializeField] private BoxCollider movement_boundary;

    // variables to track if given position or rotation exceeds avatar movement (to reduce sparsity)
    public bool has_over_moved = false;
    public bool has_over_rotated = false;

    //file to read from
    readonly string controller_path = Application.dataPath + "/Scripts/Reinforcement_Learning";
    string rotate_hand_file;
    string move_body_file;
    string move_hand_file;

    //file to output towards
    string body_position_file;

    // Start is called before the first frame update    
    void Start()
    {
        //check for serialized fields
        if (right_hand_target == null || left_hand_target == null || right_hand_forearm == null || left_hand_forearm == null || left_arm_span == null || right_arm_span == null)
            Debug.Log("Please drag the right/left hand targets/forearm and limits to AvatarController script");
        if (movement_boundary == null)
            Debug.Log("Please drag the movement boundary to AvatarController script");

        //initialize the file names
        rotate_hand_file = controller_path + "/rotate_hand.csv";
        move_body_file = controller_path + "/move_body.csv";
        move_hand_file = controller_path + "/move_hand.csv";
        body_position_file = controller_path + "/body_position.csv";

        //Rotate_hand(100, 100, 350, true);
        //Move_hand(-.2f, -1, -.2f, true);
        //Move_body(.3f, -.2f, 90f);
        //Read_movement_file(10);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Read_movement_file(int wall_id)
    {
        string position_record;

        //find the rotation of the hand
        (float, float, float) right_hand_rotation = (0, 0, 0);
        (float, float, float) left_hand_rotation = (0, 0, 0);

        using (StreamReader reader = new(rotate_hand_file))
        {
            while (!reader.EndOfStream)
            {
                //read the line
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line)) continue;

                //param separated by comma
                string[] parameters = line.Split(',');

                if (parameters.Length != 4) continue;

                // try parsing the first 4 as floats
                if (!float.TryParse(parameters[0], out float x) ||
                    !float.TryParse(parameters[1], out float y) ||
                    !float.TryParse(parameters[2], out float z) ||
                    !float.TryParse(parameters[3], out float is_right))
                {
                    continue;
                }


                if (is_right > 0) right_hand_rotation = Rotate_hand(x, y, z, true);
                else left_hand_rotation = Rotate_hand(x, y, z, false);

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_rotated)
                    Debug.LogWarning($"Hand over-rotated at ({x}, {y}, {z}), is_right: {is_right}");
            }
        }

        //save the position of the hand rotations
        position_record = string.Format("\"{0}\",\"{1}\"", right_hand_rotation, left_hand_rotation);

        //find the position of the hand
        (float, float, float) right_hand_position = (0, 0, 0);
        (float, float, float) left_hand_position = (0, 0, 0);

        using (StreamReader reader = new(move_hand_file))
        {
            while (!reader.EndOfStream)
            {
                //read the line
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line)) continue;

                //param separated by comma
                string[] parameters = line.Split(',');

                if (parameters.Length != 4) continue;

                // try parsing the first 4 as floats
                if (!float.TryParse(parameters[0], out float x) ||
                    !float.TryParse(parameters[1], out float y) ||
                    !float.TryParse(parameters[2], out float z) ||
                    !float.TryParse(parameters[3], out float is_right))
                {
                    continue;
                }


                if (is_right > 0) right_hand_position = Move_hand(x, y, z, true);
                else left_hand_position = Move_hand(x, y, z, false);

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Hand over-moved at ({x}, {y}, {z}), is_right: {is_right}");
            }
        }

        //record the hand position
        position_record = position_record + string.Format(",\"{0}\",\"{1}\"", right_hand_position, left_hand_position);

        //find the position of the hand
        (float, float, float) body_position = (0, 0, 0);

        using (StreamReader reader = new(move_body_file))
        {
            while (!reader.EndOfStream)
            {
                //read the line
                string line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line)) continue;

                //param separated by comma
                string[] parameters = line.Split(',');

                if (parameters.Length != 3) continue;

                // try parsing the first 3 as floats
                if (!float.TryParse(parameters[0], out float x) ||
                    !float.TryParse(parameters[1], out float z) ||
                    !float.TryParse(parameters[2], out float r))
                {
                    continue;
                }

                body_position = Move_body(x, z, r);

                //warn that the parameters are out of bounds, resulting in sparsity issues
                if (has_over_moved)
                    Debug.LogWarning($"Body over-moved at ({x}, {z}, {r})");
            }
        }

        //record the hand position
        position_record = position_record + string.Format(",\"{0}\",\"{1}\"\n", body_position, wall_id);

        //record the position of everything
        File.AppendAllText(body_position_file, position_record);
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


        UnityEngine.Vector3 r_current_rotation = right_hand_target.transform.eulerAngles;
        UnityEngine.Vector3 l_current_rotation = left_hand_target.transform.eulerAngles;

        //ASSUMING T POSE
        if (is_right_hand)
        {
            //rotate the hand based on current position
            r_current_rotation.x += x_angle;
            r_current_rotation.y += y_angle;
            r_current_rotation.z += z_angle;
            right_hand_target.transform.eulerAngles = r_current_rotation;
            return (r_current_rotation.x, r_current_rotation.y, r_current_rotation.z);
        }
        else
        {
            //rotate hand based on current position
            l_current_rotation.x += x_angle;
            l_current_rotation.y += y_angle;
            l_current_rotation.z += z_angle;
            left_hand_target.transform.eulerAngles = l_current_rotation;
            return (l_current_rotation.x, l_current_rotation.y, l_current_rotation.z);
        }
    }

    //move hand from T-pose to the local position of the given transform
    public (float, float, float) Move_hand(float x_pos, float y_pos, float z_pos, bool is_right_hand)
    {
        has_over_moved = false;

        //find which one is the arm span limitation
        SphereCollider arm_span = is_right_hand ? right_arm_span : left_arm_span;
        Transform target = is_right_hand ? right_hand_target : left_hand_target;

        //calculate the center
        Transform center_transform = arm_span.transform;
        UnityEngine.Vector3 center = center_transform.TransformPoint(arm_span.center);

        //move the hand based on local values; transformation based off the parents
        UnityEngine.Vector3 target_position = new(x_pos, y_pos, z_pos);
        target.localPosition = target_position;

        //check if within radius
        float radius = arm_span.radius * center_transform.lossyScale.x;

        //check if target is within the limitation sphere
        UnityEngine.Vector3 offset = target.position - center;

        if (offset.magnitude > radius)
        {
            //clamp position to surface of sphere
            UnityEngine.Vector3 revised_position = center + offset.normalized * radius;
            target.localPosition = target.parent.InverseTransformPoint(revised_position);
            has_over_moved = true;
        }

        return (target.localPosition.x, target.localPosition.y, target.localPosition.z);
    }

    //no y_pos because we assume avatar can't jump/fly. y_rotation needs to be constrained to 0-360
    public (float, float, float) Move_body(float x_pos, float z_pos, float y_rotation)
    {
        has_over_moved = false;

        //store the original transformation
        UnityEngine.Vector3 start_pos = transform.position;

        //boundary of the movement in world space rather than local
        UnityEngine.Vector3 boundary_scaled = UnityEngine.Vector3.Scale(movement_boundary.size, movement_boundary.transform.lossyScale);
        UnityEngine.Vector3 half_size = boundary_scaled * .5f;
        UnityEngine.Vector3 center = movement_boundary.transform.TransformPoint(movement_boundary.center);

        UnityEngine.Vector3 min = center - half_size;
        UnityEngine.Vector3 max = center + half_size;

        //track the amount of change
        float x_movement = x_pos;
        float z_movement = z_pos;

        //predict the final destination
        float final_x = transform.position.x + x_pos;
        float final_z = transform.position.z + z_pos;

        //check if the predicted transformation is within bounds. if not, snap to max or min position
        if (final_x > max.x)
        {
            final_x = max.x;
            x_movement = final_x - start_pos.x;
        }
        else if (final_x < min.x)
        {
            final_x = min.x;
            x_movement = final_x - start_pos.x;
        }

        if (final_z > max.z)
        {
            final_z = max.z;
            z_movement = final_z - start_pos.z;
        }
        else if (final_z < min.z)
        {
            final_z = min.z;
            z_movement = final_z - start_pos.z;
        }

        if (x_movement != x_pos || z_movement != z_pos) has_over_moved = true;

        //move avatar
        transform.position = new(final_x, transform.position.y, final_z);
        //rotate avatar
        transform.eulerAngles = new(transform.eulerAngles.x, transform.eulerAngles.y + (y_rotation % 360), transform.eulerAngles.z);

        return (x_movement, z_movement, transform.eulerAngles.y);
    }
}
