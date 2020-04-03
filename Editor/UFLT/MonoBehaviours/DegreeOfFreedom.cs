using UnityEngine;
using System.Collections;
using UFLT.Records;

namespace UFLT.MonoBehaviours
{
	/// <summary>
	/// Info extracted from a Degree of freedom(DOF).
	/// </summary>
	public class DegreeOfFreedom : MonoBehaviour
	{
		#region Properties

		/// <summary>
		/// Origin of DOF's local coordinate system.(x, y, z).
		/// </summary>
		public Vector3 origin;

		/// <summary>
		/// Point on x axis of DOF's local coordinate system (x, y, z).
		/// </summary>
		public Vector3 pointOnXAxis;

		/// <summary>
		/// Point in xy plane of DOF's local coordinate system (x, y, z)
		/// </summary>
		public Vector3 pointInXYPlane;

		/// <summary>
		/// Min, Max, Current & Increment of x with respect to local coordinate system.
		/// </summary>
		public Vector4 minMaxCurrentIncrementX;

		/// <summary>
		/// Min, Max, Current & Increment of y with respect to local coordinate system.
		/// </summary>
		public Vector4 minMaxCurrentIncrementY;

		/// <summary>
		/// Min, Max, Current & Increment of z with respect to local coordinate system.
		/// </summary>
		public Vector4 minMaxCurrentIncrementZ;

		/// <summary>
		/// Min, Max, Current & Increment of pitch 
		/// </summary>
		public Vector4 minMaxCurrentIncrementPitch;

		/// <summary>
		/// Min, Max, Current & Increment of roll 
		/// </summary>
		public Vector4 minMaxCurrentIncrementRoll;

		/// <summary>
		/// Min, Max, Current & Increment of yaw 
		/// </summary>
		public Vector4 minMaxCurrentIncrementYaw;

		/// <summary>
		/// Min, Max, Current & Increment of scale z
		/// </summary>
		public Vector4 minMaxCurrentIncrementScaleZ;

		/// <summary>
		/// Min, Max, Current & Increment of scale y.
		/// </summary>
		public Vector4 minMaxCurrentIncrementScaleY;

		/// <summary>
		/// Min, Max, Current & Increment of scale x.
		/// </summary>
		public Vector4 minMaxCurrentIncrementScaleX;

		/// <summary>
		/// If true then the X translation should be limited using the relevant Min,Max values.
		/// </summary>		
		public bool xTranslationLimited;

		/// <summary>
		/// If true then the Y translation should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool yTranslationLimited;

		/// <summary>
		/// If true then the Z translation should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool zTranslationLimited;

		/// <summary>
		/// If true then the pitch should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool pitchLimited;

		/// <summary>
		/// If true then the roll should be limited using the relevant Min,Max values.
		/// </summary>					
		public bool rollLimited;

		/// <summary>
		/// If true then the yaw should be limited using the relevant Min,Max values.
		/// </summary>			
		public bool yawLimited;

		/// <summary>
		/// If true then the X scale should be limited using the relevant Min,Max values.
		/// </summary>					
		public bool scaleXLimited;

		/// <summary>
		/// If true then the Y scale should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool scaleYLimited;

		/// <summary>
		/// If true then the Z scale should be limited using the relevant Min,Max values.
		/// </summary>						
		public bool scaleZLimited;

		#endregion Properties

		/// <summary>
		/// Called by the DOF class when creating an OpenFlight DOF node from file.
		/// </summary>
		/// <param name="switchData"></param>
		public virtual void OnDOFNode(DOF dofData)
		{
			origin = new Vector3((float)dofData.Origin[0], (float)dofData.Origin[1], (float)dofData.Origin[2]);
			pointOnXAxis = new Vector3((float)dofData.PointOnXAxis[0], (float)dofData.PointOnXAxis[1], (float)dofData.PointOnXAxis[2]);
			pointInXYPlane = new Vector3((float)dofData.PointInXYPlane[0], (float)dofData.PointInXYPlane[1], (float)dofData.PointInXYPlane[2]);
			minMaxCurrentIncrementX = new Vector4((float)dofData.MinMaxCurrentIncrementX[0], (float)dofData.MinMaxCurrentIncrementX[1], (float)dofData.MinMaxCurrentIncrementX[2], (float)dofData.MinMaxCurrentIncrementX[3]);
			minMaxCurrentIncrementY = new Vector4((float)dofData.MinMaxCurrentIncrementY[0], (float)dofData.MinMaxCurrentIncrementY[1], (float)dofData.MinMaxCurrentIncrementY[2], (float)dofData.MinMaxCurrentIncrementY[3]);
			minMaxCurrentIncrementZ = new Vector4((float)dofData.MinMaxCurrentIncrementZ[0], (float)dofData.MinMaxCurrentIncrementZ[1], (float)dofData.MinMaxCurrentIncrementZ[2], (float)dofData.MinMaxCurrentIncrementZ[3]);
			minMaxCurrentIncrementPitch = new Vector4((float)dofData.MinMaxCurrentIncrementPitch[0], (float)dofData.MinMaxCurrentIncrementPitch[1], (float)dofData.MinMaxCurrentIncrementPitch[2], (float)dofData.MinMaxCurrentIncrementPitch[3]);
			minMaxCurrentIncrementRoll = new Vector4((float)dofData.MinMaxCurrentIncrementRoll[0], (float)dofData.MinMaxCurrentIncrementRoll[1], (float)dofData.MinMaxCurrentIncrementRoll[2], (float)dofData.MinMaxCurrentIncrementRoll[3]);
			minMaxCurrentIncrementYaw = new Vector4((float)dofData.MinMaxCurrentIncrementYaw[0], (float)dofData.MinMaxCurrentIncrementYaw[1], (float)dofData.MinMaxCurrentIncrementYaw[2], (float)dofData.MinMaxCurrentIncrementYaw[3]);
			minMaxCurrentIncrementScaleZ = new Vector4((float)dofData.MinMaxCurrentIncrementScaleZ[0], (float)dofData.MinMaxCurrentIncrementScaleZ[1], (float)dofData.MinMaxCurrentIncrementScaleZ[2], (float)dofData.MinMaxCurrentIncrementScaleZ[3]);
			minMaxCurrentIncrementScaleY = new Vector4((float)dofData.MinMaxCurrentIncrementScaleY[0], (float)dofData.MinMaxCurrentIncrementScaleY[1], (float)dofData.MinMaxCurrentIncrementScaleY[2], (float)dofData.MinMaxCurrentIncrementScaleY[3]);
			minMaxCurrentIncrementScaleX = new Vector4((float)dofData.MinMaxCurrentIncrementScaleX[0], (float)dofData.MinMaxCurrentIncrementScaleX[1], (float)dofData.MinMaxCurrentIncrementScaleX[2], (float)dofData.MinMaxCurrentIncrementScaleX[3]);
			xTranslationLimited = dofData.FlagsXTranslationLimited;
			yTranslationLimited = dofData.FlagsYTranslationLimited;
			zTranslationLimited = dofData.FlagsYTranslationLimited;
			pitchLimited = dofData.FlagsPitchLimited;
			rollLimited = dofData.FlagsRollLimited;
			yawLimited = dofData.FlagsYawLimited;
			scaleXLimited = dofData.FlagsScaleXLimited;
			scaleYLimited = dofData.FlagsScaleYLimited;
			scaleZLimited = dofData.FlagsScaleZLimited;
		}
	}
}