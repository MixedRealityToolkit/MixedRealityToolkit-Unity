using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

public class AxisRotationConstraint : TransformConstraint
{
    #region Properties

    [SerializeField]
    [Tooltip("Set to true, if rotation around X-Axis is possible.")]
    private bool allowRotationOnXAxis;

    [SerializeField]
    [Tooltip("Set to true, if rotation around Y-Axis is possible.")]
    private bool allowRotationOnYAxis;

    [SerializeField]
    [Tooltip("Set to true, if rotation around Z-Axis is possible.")]
    private bool allowRotationOnZAxis;

    [SerializeField]
    [Tooltip("Set the size of the steps for fixed rotation")]
    private float fixedStepSize = 1.0f;

    [SerializeField]
    [Tooltip("Enable to allow rotation only in fixed steps.")]
    private bool bFixedStep = false;

    [SerializeField]
    [Tooltip("Check if object rotation should be in local space of object being manipulated instead of world space.")]
    private bool useLocalSpaceForConstraint = false;

    /// <summary>
    /// Gets or sets whether the constraints should be applied in local space of the object being manipulated or world space.
    /// </summary>
    public bool UseLocalSpaceForConstraint
    {
        get => useLocalSpaceForConstraint;
        set => useLocalSpaceForConstraint = value;
    }

    public override TransformFlags ConstraintType => TransformFlags.Rotate;

    #endregion Properties


    private void Awake()
    {
        allowRotationOnXAxis = true;
        allowRotationOnYAxis = true;
        allowRotationOnZAxis = true;
    }


    #region Public Methods

    /// <summary>
    /// Removes rotation about given axis if its flag is found
    /// in ConstraintOnRotation
    /// </summary>
    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Quaternion rotation = transform.Rotation * Quaternion.Inverse(WorldPoseOnManipulationStart.Rotation);
        Vector3 eulers = rotation.eulerAngles;

        if (bFixedStep)
        {
            Vector3 eulerStart = Quaternion.Inverse(WorldPoseOnManipulationStart.Rotation).eulerAngles;

            eulers.x = ((int)(eulers.x / fixedStepSize)) * fixedStepSize;
            eulers.y = ((int)(eulers.y / fixedStepSize)) * fixedStepSize;
            eulers.z = ((int)(eulers.z / fixedStepSize)) * fixedStepSize;
        }


        if (!allowRotationOnXAxis)
            eulers.x = 0;
        
        if (!allowRotationOnYAxis)
            eulers.y = 0;
        
        if (!allowRotationOnZAxis)
            eulers.z = 0;
        

        transform.Rotation = useLocalSpaceForConstraint
            ? WorldPoseOnManipulationStart.Rotation * Quaternion.Euler(eulers)
            : Quaternion.Euler(eulers) * WorldPoseOnManipulationStart.Rotation;
    }

    /// <summary>
    /// checks, if user can rotate around given axis or not
    /// </summary>
    /// <param name="axis">Axis to check for</param>
    /// <returns>true, if rotation is possible, otherwise false</returns>
    public bool CanRotateOnAxis(AxisFlags axis)
    {
        switch (axis)
        {
            case AxisFlags.XAxis: return allowRotationOnXAxis;
            case AxisFlags.YAxis: return allowRotationOnYAxis;
            case AxisFlags.ZAxis: return allowRotationOnZAxis;
        }

        //function-complete return statement but will never occur
        return false;
    }

    /// <summary>
    /// sets rotation on given axis
    /// </summary>
    /// <param name="axis">Axis to allow/disallow rotation</param>
    /// <param name="canMove">true, if rotation is possible (standard) or not</param>
    public void SetRotateOneAxis(AxisFlags axis, bool canMove)
    {
        switch (axis)
        {
            case AxisFlags.XAxis: allowRotationOnXAxis = canMove; break;
            case AxisFlags.YAxis: allowRotationOnYAxis = canMove; break;
            case AxisFlags.ZAxis: allowRotationOnZAxis = canMove; break;
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
