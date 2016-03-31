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
	public class TextureImportSettingiOS : TextureImportSettingBase
	{
		protected override string targetPlatform
		{
			get { return "Nintendo 3DS"; }
		}
		[MenuItem("Tools/PlatformTextureSetting/iOS")]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow(typeof(TextureImportSettingiOS)) as TextureImportSettingiOS;
		}
	}
}