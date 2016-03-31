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
	public class TextureImportSettingAndroid : TextureImportSettingBase
	{
		protected override string targetPlatform
		{
			get { return "Android"; }
		}
		[MenuItem("Tools/PlatformTextureSetting/Android")]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow(typeof(TextureImportSettingAndroid));
		}
	}
}