using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_DrawDebugShape : MonoBehaviour
{
    public bool showAlways;
    public UserDefined_Color color;
    public enum UserDefined_Color
    {
        green,
        red,
        blue,
        yellow,
        white,
        black
    }
    public Shape shape;
    public enum Shape
    {
        cube,
        wireCube,
        sphere,
        wireSphere,
        line,
        ray
    }

    [Header("Cube")]
    public Vector3 scale;

    [Header("Sphere")]
    public float radius;

    [Header("Line")]
    public Transform lineStartPos;
    public Transform lineEndPos;

    [Header("Ray")]
    public Transform rayStartPos;
    public float distance;
    public Direction direction;
    public enum Direction
    {
        front,
        back,
        left,
        right,
        up,
        down
    }

    //private variables
    private Vector3 dir;

    private void OnDrawGizmos()
    {
        if (showAlways)
        {
            switch (color)
            {
                case UserDefined_Color.green:
                    Gizmos.color = Color.green;
                    break;
                case UserDefined_Color.red:
                    Gizmos.color = Color.red;
                    break;
                case UserDefined_Color.blue:
                    Gizmos.color = Color.blue;
                    break;
                case UserDefined_Color.yellow:
                    Gizmos.color = Color.yellow;
                    break;
                case UserDefined_Color.white:
                    Gizmos.color = Color.white;
                    break;
                case UserDefined_Color.black:
                    Gizmos.color = Color.black;
                    break;
            }
            switch (shape)
            {
                case Shape.cube:
                    Gizmos.DrawCube(transform.position, scale);
                    break;
                case Shape.wireCube:
                    Gizmos.DrawWireCube(transform.position, scale);
                    break;
                case Shape.sphere:
                    Gizmos.DrawSphere(transform.position, radius);
                    break;
                case Shape.wireSphere:
                    Gizmos.DrawWireSphere(transform.position, radius);
                    break;
                case Shape.line:
                    Gizmos.DrawLine(lineStartPos.position, lineEndPos.position);
                    break;
                case Shape.ray:
                    switch (direction)
                    {
                        case Direction.front:
                            dir = transform.TransformDirection(Vector3.forward) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.back:
                            dir = transform.TransformDirection(Vector3.back) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.left:
                            dir = transform.TransformDirection(Vector3.left) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.right:
                            dir = transform.TransformDirection(Vector3.right) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.up:
                            dir = transform.TransformDirection(Vector3.up) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.down:
                            dir = transform.TransformDirection(Vector3.down) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                    }
                    break;
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (!showAlways)
        {
            switch (color)
            {
                case UserDefined_Color.green:
                    Gizmos.color = Color.green;
                    break;
                case UserDefined_Color.red:
                    Gizmos.color = Color.red;
                    break;
                case UserDefined_Color.blue:
                    Gizmos.color = Color.blue;
                    break;
                case UserDefined_Color.yellow:
                    Gizmos.color = Color.yellow;
                    break;
                case UserDefined_Color.white:
                    Gizmos.color = Color.white;
                    break;
                case UserDefined_Color.black:
                    Gizmos.color = Color.black;
                    break;
            }
            switch (shape)
            {
                case Shape.cube:
                    Gizmos.DrawCube(transform.position, scale);
                    break;
                case Shape.wireCube:
                    Gizmos.DrawWireCube(transform.position, scale);
                    break;
                case Shape.sphere:
                    Gizmos.DrawSphere(transform.position, radius);
                    break;
                case Shape.wireSphere:
                    Gizmos.DrawWireSphere(transform.position, radius);
                    break;
                case Shape.line:
                    Gizmos.DrawLine(lineStartPos.position, lineEndPos.position);
                    break;
                case Shape.ray:
                    switch (direction)
                    {
                        case Direction.front:
                            dir = transform.TransformDirection(Vector3.forward) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.back:
                            dir = transform.TransformDirection(Vector3.back) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.left:
                            dir = transform.TransformDirection(Vector3.left) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.right:
                            dir = transform.TransformDirection(Vector3.right) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.up:
                            dir = transform.TransformDirection(Vector3.up) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                        case Direction.down:
                            dir = transform.TransformDirection(Vector3.down) * distance;
                            Gizmos.DrawRay(rayStartPos.localPosition, dir);
                            break;
                    }
                    break;
            }
        }
    }
}