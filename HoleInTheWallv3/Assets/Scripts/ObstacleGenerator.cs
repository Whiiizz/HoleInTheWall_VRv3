using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    public int wall_col = 2;                                                        // the amount of columns for the matrix
    public int wall_row = 2;                                                        // the amount of rows for the matrix
    public int block_height = 1;                                                   // the block height size
    public int block_width = 1;                                                    // the block width size
    public int block_depth = 1;                                                    // the block thickness size
    public int[,] wall;                                                             // matrix representing the wall with holes. 0 = hole, 1 = wall

    // Start is called before the first frame update
    void Start()
    {
        wall = new int[wall_row, wall_col];

        int[,] test_wall =  {   {1, 0, 0, 1, 1},
                                {1, 0, 1, 1, 0},
                                {0, 0, 0, 1, 1}
                            };

        set_wall(test_wall);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void build_wall()
    {
        // destroy all children first
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // build the wall based on the vector matrix.
        for (int i = 0; i < wall_row; i++)
        {
            for (int j = 0; j < wall_col; j++)
            {
                // if not 0, then is a wall block
                if (wall[i, j] != 0)
                {
                    // calculate centered offset 
                    float x_offset = (j - (wall_col - 1) / 2f) * block_width;
                    float y_offset = (wall_row - 1 - i) * block_height;

                    // create the cube block
                    GameObject rect_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rect_obj.name = i + " , " + j;
                    rect_obj.transform.localScale = new Vector3(block_width, block_height, block_depth);

                    // set parent to this GameObject
                    rect_obj.transform.SetParent(transform);

                    // apply local offset relative to this transform
                    Vector3 localOffset = new Vector3(x_offset, y_offset, 0f);
                    rect_obj.transform.position = transform.TransformPoint(localOffset);
                }

            }
        }
    }

    public void set_wall(int row, int col)
    {
        if (row < wall_row && col < wall_col) wall[row, col] = 1;
    }

    public void set_hole(int row, int col)
    {
        if (row < wall_row && col < wall_col) wall[row, col] = 0;
    }

    public void set_wall(int[,] given_wall)
    {
        //set the new wall to the given wall matrix
        wall = given_wall;

        wall_row = given_wall.GetLength(0);
        wall_col = given_wall.GetLength(1);

        build_wall();
    }
}
