using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

public class AxisMovementConstraint : TransformConstraint
{
    #region Properties

    [SerializeField] 
    [Tooltip("Allow movement along an X-Axis")]
    private bool movementOnXAxis;


    [SerializeField]
    [Tooltip("Allow movement along an Y-Axis")]
    private bool movementOnYAxis;


    [SerializeField]
    [Tooltip("Allow movement along an Z-Axis")]
    private bool movementOnZAxis;

    [SerializeField]
    [Tooltip("Set the size of the steps for fixed movement")]
    private float fixedStepSize = 1.0f;

    [SerializeField]
    [Tooltip("Enable to allow re-positioning only in fixed steps.")]
    private bool bFixedStep = false;

    [SerializeField]
    [Tooltip("Relative to rotation at manipulation start or world")]
    private bool useLocalSpaceForConstraint = false;

    /// <summary>
    /// Relative to rotation at manipulation start or world
    /// </summary>
    public bool UseLocalSpaceForConstraint
    {
        get => useLocalSpaceForConstraint;
        set => useLocalSpaceForConstraint = value;
    }

    public override TransformFlags ConstraintType => TransformFlags.Move;

    #endregion Properties


    private void Start()
    {
        movementOnXAxis = true;
        movementOnYAxis = true;
        movementOnZAxis = true;
    }

    #region Public Methods

    /// <summary>
    /// Removes movement along a given axis if its flag is found
    /// in ConstraintOnMovement
    /// </summary>
    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Quaternion inverseRotation = Quaternion.Inverse(WorldPoseOnManipulationStart.Rotation);
        Vector3 position = transform.Position;

        if (bFixedStep)
        {
            position.x = WorldPoseOnManipulationStart.Position.x + ((int)((position.x - WorldPoseOnManipulationStart.Position.x) / fixedStepSize)) * fixedStepSize;
            position.y = WorldPoseOnManipulationStart.Position.y + ((int)((position.y - WorldPoseOnManipulationStart.Position.y) / fixedStepSize)) * fixedStepSize;
            position.z = WorldPoseOnManipulationStart.Position.z + ((int)((position.z - WorldPoseOnManipulationStart.Position.z) / fixedStepSize)) * fixedStepSize;
        }

        if (useLocalSpaceForConstraint)
        {
            //movement considering local space
            position = inverseRotation * position;

            if (!movementOnXAxis)
                position.x = (inverseRotation * WorldPoseOnManipulationStart.Position).x;

            if (!movementOnYAxis)
                position.y = (inverseRotation * WorldPoseOnManipulationStart.Position).y;

            if (!movementOnZAxis)
                position.z = (inverseRotation * WorldPoseOnManipulationStart.Position).z;


            position = WorldPoseOnManipulationStart.Rotation * position;

        }
        else
        {
            //world manipulation
            if (!movementOnXAxis)
                position.x = WorldPoseOnManipulationStart.Position.x;
            if (!movementOnYAxis)
                position.y = WorldPoseOnManipulationStart.Position.y;
            if (!movementOnZAxis)
                position.z = WorldPoseOnManipulationStart.Position.z;

        }
        
        transform.Position = position;
    }


    /// <summary>
    /// checks, if user can move on given axis or not
    /// </summary>
    /// <param name="axis">Axis to check for</param>
    /// <returns>true, if movement is possible, otherwise false</returns>
    public bool CanMoveOnAxis(AxisFlags axis)
    {
        switch (axis)
        {
            case AxisFlags.XAxis: return movementOnXAxis;
            case AxisFlags.YAxis: return movementOnYAxis;
            case AxisFlags.ZAxis: return movementOnZAxis;
        }

        //function-complete return statement but will never occur
        return false;
    }

    /// <summary>
    /// sets movement on given axis
    /// </summary>
    /// <param name="axis">Axis to allow/disallow movement</param>
    /// <param name="canMove">true, if movement is possible (standard) or not</param>
    public void SetMoveOneAxis(AxisFlags axis, bool canMove)
    {
        switch (axis)
        {
            case AxisFlags.XAxis: movementOnXAxis = canMove; break;
            case AxisFlags.YAxis: movementOnYAxis = canMove; break;
            case AxisFlags.ZAxis: movementOnZAxis = canMove; break;
        }
    }


    public bool AllowStepSize
    {
        get => bFixedStep;
        set => bFixedStep = value;
    }

    public float StepSize
    {
        get => fixedStepSize;
        set => fixedStepSize = value;

    }

    #endregion Public Methods
}
