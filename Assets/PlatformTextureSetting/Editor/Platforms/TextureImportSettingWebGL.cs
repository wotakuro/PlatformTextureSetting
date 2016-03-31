using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PlatformTextureSetting
{
	public class TextureImportSettingWebGL : TextureImportSettingBase
	{
		protected override string targetPlatform
		{
			get { return "WebGL"; }
		}
		[MenuItem("Tools/PlatformTextureSetting/WebGL")]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow(typeof(TextureImportSettingWebGL));
		}
	}
}