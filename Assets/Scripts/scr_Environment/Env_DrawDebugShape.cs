using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_DrawDebugShape : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Is this shape shown even if it is not selected?")]
    [SerializeField] private bool showAlways;
    [Tooltip("One of the 6 base colors for the shape.")]
    [SerializeField] private UserDefined_Color color;
    [SerializeField] private enum UserDefined_Color
    {
        green,
        red,
        blue,
        yellow,
        white,
        black
    }
    [Tooltip("One of the 6 available shapes.")]
    [SerializeField] private Shape shape;
    [SerializeField] private enum Shape
    {
        cube,
        wireCube,
        sphere,
        wireSphere,
        line,
        ray
    }

    [Header("Connect to cell")]
    [SerializeField] private bool connectedToCellScript;
    [SerializeField] private Trigger_Location CellScript;

    [Header("Cube")]
    [SerializeField] private Vector3 scale;

    [Header("Sphere")]
    [SerializeField] private float radius;

    [Header("Line")]
    [SerializeField] private Transform lineStartPos;
    [SerializeField] private Transform lineEndPos;

    [Header("Ray")]
    [SerializeField] private Transform rayStartPos;
    [SerializeField] private float distance;
    [SerializeField] private Direction direction;
    [SerializeField] private enum Direction
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

    private void Awake()
    {
        if (connectedToCellScript)
        {
            CellScript = GetComponent<Trigger_Location>();
        }
    }

    //always shows gizmos
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
                    if (connectedToCellScript)
                    {
                        Gizmos.DrawSphere(transform.position, CellScript.maxDistanceToDiscover);
                    }
                    else
                    {
                        Gizmos.DrawSphere(transform.position, radius);
                    }
                    break;
                case Shape.wireSphere:
                    if (connectedToCellScript)
                    {
                        Gizmos.DrawWireSphere(transform.position, CellScript.maxDistanceToDiscover);
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(transform.position, radius);
                    }
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

    //only shows gizmos if this item is selected
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
                    if (connectedToCellScript)
                    {
                        Gizmos.DrawSphere(transform.position, CellScript.maxDistanceToDiscover);
                    }
                    else
                    {
                        Gizmos.DrawSphere(transform.position, radius);
                    }
                    break;
                case Shape.wireSphere:
                    if (connectedToCellScript)
                    {
                        Gizmos.DrawWireSphere(transform.position, CellScript.maxDistanceToDiscover);
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(transform.position, radius);
                    }
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