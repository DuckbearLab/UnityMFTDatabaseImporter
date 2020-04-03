using UnityEngine;
using System.Collections.Generic;
using UFLT.MonoBehaviours;
using System;

namespace UFLT
{
	/// <summary>
	/// Openflight Import settings.    
	/// </summary>
	[Serializable]
	public class ImportSettings
	{
		/// <summary>
		/// Additional directories to search for textures and external references.
		/// </summary>
		public List<string> additionalSearchDirectories = new List<string>();

		/// <summary>
		/// Class that will be added to DOF GameObjects. The OnDOFNode( DOF ) message will 
		/// be sent to this GameObject with the relevant data when it is first created. Leave this field
		/// blank to use the default DOF component.
		/// </summary>		
		//[Tooltip( "Class that will be added to DOF GameObjects. The \"OnDOFNode( DOF )\" " +
		//          "message will be sent to this GameObject with the relevant data when it is first created." +
		//          "Leave this field blank to use the default DOF component. " )]
		//public string degreeOfFreedomComponent;

		/// <summary>
		/// Class that will be added to LOD GameObjects. The OnLODNode( LOD ) message will 
		/// be sent to this GameObject with the relevant data when it is first created. Leave this field
		/// blank to use the default LOD component.
		/// </summary>		
		//[Tooltip( "Class that will be added to LOD GameObjects. The \"OnLODNode( LOD )\" " +
		//          "message will be sent to this GameObject with the relevant data when it is first created." +
		//          "Leave this field blank to use the default LOD component. " )]
		//public string levelOfDetailComponent;

		/// <summary>
		/// Class that will be added to SwitchNode GameObjects. The OnSwitchNode( SwitchNode ) message will 
		/// be sent to this GameObject with the relevant data when it is first created. Leave this field
		/// blank to use the default Switch component.
		/// </summary>
		//[Tooltip( "Class that will be added to SwitchNode GameObjects. The \"OnSwitchNode( SwitchNode )\" " +
		//          "message will be sent to this GameObject with the relevant data when it is first created." +
		//          "Leave this field blank to use the default Switch component. " )]
		//public string switchComponent;

		/// <summary>
		/// Adds mesh colliders to all meshes if true.
		/// </summary>
		[Tooltip("Adds mesh colliders to all meshes if true.")]
		public bool generateColliders = false;
	}
}