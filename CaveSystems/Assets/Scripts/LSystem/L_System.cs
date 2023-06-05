using System.Collections.Generic;
using UnityEngine;

public class L_System : MonoBehaviour
{
    [Header("L_System settings")]
    [Tooltip("Gameobject that is moved along the generated structure")]
    public Transform turtle;

    [Tooltip("The base length of a line")]
    public float baseCompression;

    [HideInInspector] public float compression;

    [Tooltip("The base angle the turtle is rotated by")]
    public float baseAngle;

    [HideInInspector] public float angle;

    [Tooltip("How many time the alphabet is iterrated through")]
    public int generations;

    [Tooltip("If the L_System should use a custom ruleset or generate one")]
    public bool predeterminedRules;

    [Tooltip("Custom ruleset")]
    public string rules;

    [Tooltip("If the L_system should use a custom Axiom or generate one")]
    public bool predeterminedAxiom;

    [Tooltip("Length of the generated axiom")]
    public int axiomLength;

    [Tooltip("Custom axiom")]
    public string axiom;

    // current word describing the order of actions
    protected List<char> word = new List<char>();

    // list of all possible actions
    protected List<L_Action> alphabet = new List<L_Action>();

    // list of all positions the turtle moved to
    [HideInInspector] public List<List<Vector3>> positions = new List<List<Vector3>>();

    [HideInInspector] public int timesLoaded;
    [HideInInspector] public Vector3 savePos;
    [HideInInspector] public Vector3 down;
    [HideInInspector] public Vector3 up;
    [HideInInspector] public Vector3 left;
    [HideInInspector] public Vector3 right;
    [HideInInspector] public Vector3 front;
    [HideInInspector] public Vector3 back;

    /// <summary>
    /// Foundation to generate the L_System
    /// </summary>
    public virtual void GenerateLSystem()
    {
        ClearData();
        compression = baseCompression;
        angle = baseAngle;
        savePos = turtle.transform.position;
        positions.Add(new List<Vector3>());
    }

    /// <summary>
    /// Iterates through the word a set amount of times and applies the rules to it
    /// </summary>
    protected virtual void Iterate()
    { }

    /// <summary>
    /// goes through the generated word and applies every action in order
    /// </summary>
    protected virtual void ExecuteWord()
    { }

    /// <summary>
    /// calculates the corner points of the cube surrounding it
    /// </summary>
    protected virtual void SetCornerPoints()
    { }

    /// <summary>
    /// Clears the data of the current L_System
    /// </summary>
    protected virtual void ClearData()
    {
        word.Clear();
        alphabet.Clear();
        positions.Clear();
        timesLoaded = 0;
        turtle.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}