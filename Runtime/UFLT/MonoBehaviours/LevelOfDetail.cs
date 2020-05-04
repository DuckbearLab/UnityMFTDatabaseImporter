using UnityEngine;
using System.Collections;
using LOD = UFLT.Records.LOD;

namespace UFLT.MonoBehaviours
{
	[ExecuteInEditMode]
	public class LevelOfDetail : MonoBehaviour
	{
		#region Properties

		/// <summary>
		/// The distance to switch the model into view.
		/// </summary>
		public float SwitchInDistance
		{
			get
			{
				return switchInDistance;
			}
			set
			{
				switchInDistance = value;
				if (Application.isPlaying) switchInDistanceSqr = value * value;
			}
		}
		[SerializeField]
		private float switchInDistance;
		private float switchInDistanceSqr;

		/// <summary>
		/// The distance to switch the model out of view.
		/// </summary>
		public float SwitchOutDistance
		{
			get
			{
				return switchOutDistance;
			}
			set
			{
				switchOutDistance = value;
				if (Application.isPlaying) switchOutDistanceSqr = value * value;
			}
		}
		[SerializeField]
		private float switchOutDistance;
		public float switchOutDistanceSqr;
		
		/// <summary>
		/// Use previous slant range.
		/// </summary>
		public bool usePreviousSlantRange;

		/// <summary>
		/// false for replacement LOD, true for additive LOD.
		/// </summary>
		public bool additive;

		/// <summary>
		/// Flags value
		/// </summary>
		public bool freezeCenter;

		/// <summary>
		/// Center of LOD.
		/// </summary>
		public Vector3 center;

		/// <summary>
		/// The range over which real-time smoothing effects should be employed 
		/// while switching from one LOD to another. Smoothing effects include 
		/// geometric morphing and image blending. The smoothing effect is active
		/// between: switch-in distance minus transition range (near), and 
		/// switch-in distance(far).
		/// </summary>
		public float transitionRange;

		/// <summary>
		/// Used to calculate switch in and out distances based on viewing 
		/// parameters of your simulation display system
		/// </summary>
		public float significantSize;

		#endregion
		
		private bool _currentState = true;

		/// <summary>
		/// Called by the LOD class when creating an OpenFlight LOD node from file.
		/// </summary>
		/// <param name="switchData"></param>
		public virtual void OnLODNode(LOD switchData)
		{
			switchInDistance = (float)switchData.SwitchInDistance;
			switchOutDistance = (float)switchData.SwitchOutDistance;
			usePreviousSlantRange = switchData.FlagsUsePreviousSlantRange;
			additive = switchData.FlagsAdditiveLODsBelow;
			freezeCenter = switchData.FlagsFreezeCenter;
            // DuckbearLab: FIX! Inverted X
			center = new Vector3((float)switchData.Center[0], (float)switchData.Center[1], (float)switchData.Center[2]);
			transitionRange = (float)switchData.TransitionRange;
			significantSize = (float)switchData.SignificantSize;
			InitLOD();
		}

		private void Start()
		{
			InitLOD();
			switchInDistanceSqr = switchInDistance * switchInDistance;
			switchOutDistanceSqr = switchOutDistance * switchOutDistance;
		}

		private void InitLOD()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.SetActive(true);
			}
		}
		
		private void Update()
		{
			if (Camera.main == null)
				return;

			bool enable = _currentState;
			
			if(Application.isPlaying)
			{				
				float distSqr = Vector3.SqrMagnitude(transform.TransformPoint(center) - Camera.main.transform.position);
				enable = (distSqr >= switchOutDistanceSqr) && (distSqr < switchInDistanceSqr);
			}
			else
			{
				float dist = Vector3.Distance(transform.TransformPoint(center), Camera.main.transform.position);
				enable = (dist >= switchOutDistance) && (dist < switchInDistance);
			}					
			
			if (enable != _currentState)
			{
				_currentState = enable;
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(enable);
				}
			}
		}

		#region Editor

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Vector3 pos = transform.TransformPoint(center);
			Gizmos.DrawRay(pos, Vector3.up * 1000);						
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = _currentState ? Color.blue : Color.black;
			Vector3 pos = transform.TransformPoint(center);
			Gizmos.DrawRay(pos, Vector3.up * 100);							
        }

		#endregion Editor
	}
}