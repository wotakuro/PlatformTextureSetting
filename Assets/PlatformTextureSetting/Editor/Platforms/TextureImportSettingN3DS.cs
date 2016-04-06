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
	public class TextureImportSettingN3DS : TextureImportSettingBase
	{
		protected override string targetPlatform
		{
			get { return "Nintendo 3DS"; }
		}
		[MenuItem("Tools/PlatformTextureSetting/Nintendo3DS")]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow(typeof(TextureImportSettingN3DS));
		}
	}
}