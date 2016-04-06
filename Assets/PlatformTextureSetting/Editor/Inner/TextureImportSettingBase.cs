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
	public abstract class TextureImportSettingBase : EditorWindow
	{
		#region INNER_CLASS
		public class TextureImportMaxSizeChangeRule
		{
			public enum EParameterFormat
			{
				Pixel,
				Division,
			}
			public static readonly string[] MaxPixelSizeList = { "16", "32", "64", "128", "256", "512", "1024", "2048" };
			public static readonly string[] DivParamSizeList = { "1.0", "0.5", "0.25", "0.125", "0.0625" };

			public string pathMatch;
			public EParameterFormat parameterFormat;
			public int parameter;

			public TextureImportMaxSizeChangeRule(string path, EParameterFormat format, int param)
			{
				this.pathMatch = path;
				this.parameterFormat = format;
				this.parameter = param;
			}

			public int GetRulePopupIndex()
			{
				switch (this.parameterFormat)
				{
					case EParameterFormat.Pixel:
						return 0;
					case EParameterFormat.Division:
						return 1;
				}
				return 1;
			}
			public void SetRulePopupIndex(int idx)
			{
				var oldParamFormat = this.parameterFormat;
				switch (idx)
				{
					case 0:
						this.parameterFormat = EParameterFormat.Pixel;
						break;
					case 1:
						this.parameterFormat = EParameterFormat.Division;
						break;
				}
				if (oldParamFormat != parameterFormat)
				{
					this.parameter = 0;
				}
			}

			public int GetMaxPixelPopupIndex()
			{
				int idx = 0;
				foreach (var pixel in MaxPixelSizeList)
				{
					int param = System.Int32.Parse(pixel);
					if (param == this.parameter)
					{
						return idx;
					}
					++idx;
				}
				return 4;
			}
			public void SetMaxPixelPopupIndex(int idx)
			{
				if (0 > idx || idx >= MaxPixelSizeList.Length)
				{
					this.parameter = 256;
					return;
				}
				this.parameter = System.Int32.Parse(MaxPixelSizeList[idx]);
			}

			public int GetDivParamPopupIndex()
			{
				for (int i = 0; i < DivParamSizeList.Length; ++i)
				{
					if (this.parameter == (1 << i))
					{
						return i;
					}
				}
				return 1;
			}
			public void SetDivParamPopupIndex(int idx)
			{
				this.parameter = (1 << idx);
			}
		}

		/// <summary>
		/// テクスチャフォーマット変換用のデータクラス
		/// </summary>
		public class TextureImportFormatChangeRule
		{
			public string pathMatch;
			public TextureImporterFormat textureFormat;
			public int quality;

			public TextureImportFormatChangeRule(string path, TextureImporterFormat format, int q)
			{
				this.pathMatch = path;
				this.textureFormat = format;
				this.quality = q;
			}
		}
		#endregion INNER_CLASS

		#region OVER_RIDE_LIST
		protected abstract string targetPlatform{get;}
		protected virtual TextureImporterFormat[] GetAllowFormatList()
		{
			return new TextureImporterFormat[]{
			TextureImporterFormat.AutomaticCompressed,
			TextureImporterFormat.DXT1,
			TextureImporterFormat.DXT5,
			TextureImporterFormat.DXT1Crunched,
			TextureImporterFormat.DXT5Crunched,
			TextureImporterFormat.RGB24,
			TextureImporterFormat.RGB24,
			TextureImporterFormat.Alpha8,
			TextureImporterFormat.ARGB16,
			TextureImporterFormat.ARGB32,
		};
		}
		protected virtual string RuleFileBasePath
		{
			get
			{
				return "Assets/PlatformTextureSetting/Editor/Data/" + this.targetPlatform;
			}
		}

		#endregion OVER_RIDE_LIST

		private string SizeRuleFilePath
		{
			get
			{
				return Path.Combine(System.IO.Directory.GetCurrentDirectory(), RuleFileBasePath + "Size.txt");
			}
		}
		private string FormatRuleFilePath
		{
			get
			{
				return Path.Combine(System.IO.Directory.GetCurrentDirectory(), RuleFileBasePath + "Format.txt");
			}
		}

		private static readonly byte[] PngHeaderData = new byte[] { (byte)0x89, (byte)0x50, (byte)0x4E, (byte)0x47, (byte)0x0D, (byte)0x0A, (byte)0x1A, (byte)0x0A };

		private TextureImporterFormat[] AllowFormatList
		{
			get
			{
				if (allowFormatList == null) { allowFormatList = this.GetAllowFormatList(); }
				return allowFormatList;
			}
		}

		private string[] AllowFormatStringList
		{
			get
			{
				if (allowFormatStrList == null)
				{
					var list = this.AllowFormatList;
					int length = list.Length;
					allowFormatStrList = new string[length];
					for (int i = 0; i < length; ++i)
					{
						this.allowFormatStrList[i] = list[i].ToString();
					}
				}
				return allowFormatStrList;
			}
		}
		private string[] allowFormatStrList;
		private string[] sizeRuleStrList;
		
		private TextureImporterFormat[] allowFormatList;

		private enum ERuleButtonAction
		{
			Nothing,
			Upper,
			Downer,
			Delete
		}

		private List<TextureImportMaxSizeChangeRule> sizeRuleList;
		private List<TextureImportFormatChangeRule> formatRuleList;

		private Vector2 scrollParam;

		private Vector2 scrollFormatParam;
		private string[] guiTabStr;
		private int guiTabSelect = 0;


		/// <summary>
		/// Execute for CUI interface
		/// </summary>
		public void ExecuteForCUI()
		{
//			TextureImportSettingBase window = new TextureImportSettingBase();
			this.LoadRuleList();
			this.ExecValidateDefaultTextureSize();
			this.ChangeImportSettings(this.targetPlatform, this.sizeRuleList, this.formatRuleList);
		}

		void OnEnable()
		{
			this.titleContent.text = this.targetPlatform;
			this.LoadRuleList();
			this.SetLanguageData();
		}

		void SetLanguageData()
		{
			this.guiTabStr = new string[] { LanguageData.GetString(ETags.TextureSizeRule), LanguageData.GetString(ETags.FormatRule) };
			this.sizeRuleStrList = new string[] { LanguageData.GetString(ETags.MaxPixel), LanguageData.GetString(ETags.RateFromDefault) };

		}

		void OnGUI()
		{

			{
				var oldLang = LanguageData.Current;
				LanguageData.Current = (ELanguage)EditorGUILayout.EnumPopup(LanguageData.Current, GUILayout.Width(200));
				if (oldLang != LanguageData.Current)
				{
					this.SetLanguageData();
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(string.Format(LanguageData.GetString( ETags.Title), targetPlatform));
				if (GUILayout.Button(LanguageData.GetString(ETags.LoadBtn), GUILayout.Width(90.0f)))
				{
					if (EditorUtility.DisplayDialog(LanguageData.GetString(ETags.LoadDialogTitle), LanguageData.GetString(ETags.LoadDialogMessage), 
						LanguageData.GetString(ETags.OkBtn), LanguageData.GetString(ETags.CancelBtn)))
					{
						this.LoadRuleList();
					}
				}
				if (GUILayout.Button(LanguageData.GetString(ETags.SaveBtn), GUILayout.Width(90.0f)))
				{
					if (EditorUtility.DisplayDialog(LanguageData.GetString(ETags.SaveDialogTitle), LanguageData.GetString(ETags.SaveDialogMessage),
						LanguageData.GetString(ETags.OkBtn), LanguageData.GetString(ETags.CancelBtn)))
					{
						this.SaveRuleList();
					}
				}

				if (GUILayout.Button(LanguageData.GetString(ETags.ExecBtn), GUILayout.Width(80.0f)))
				{
					if (EditorUtility.DisplayDialog( string.Format( LanguageData.GetString(ETags.ExecDialogMessage), targetPlatform ),
						LanguageData.GetString(ETags.ExecDialogMessage),
						LanguageData.GetString(ETags.OkBtn),
						LanguageData.GetString(ETags.CancelBtn)))
					{
						this.SaveRuleList();
						this.ChangeImportSettings(targetPlatform, this.sizeRuleList, this.formatRuleList);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			{
				GUILayout.Label(LanguageData.GetString(ETags.ValidateDefault));
				if (GUILayout.Button(LanguageData.GetString( ETags.ValidateBtn), GUILayout.Width(120)))
				{
					if (EditorUtility.DisplayDialog( LanguageData.GetString(ETags.ValidateDialogTitle) ,
						LanguageData.GetString(ETags.ValidateDialogMessage),
						LanguageData.GetString(ETags.OkBtn),
						LanguageData.GetString(ETags.CancelBtn)))
					{
						ExecValidateDefaultTextureSize();
					}
				}
			}
			GUILayout.Label("");
			GUILayout.Label(LanguageData.GetString(ETags.RuleList) );

			this.guiTabSelect = GUILayout.Toolbar(this.guiTabSelect, this.guiTabStr);
			EditorGUILayout.BeginHorizontal();
			switch (guiTabSelect)
			{
				case 0:
					this.OnGUISizeRuleList();
					break;
				case 1:
					this.OnGUIFormatRuleList();
					break;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void OnGUISizeRuleList()
		{
			scrollParam = EditorGUILayout.BeginScrollView(scrollParam);
			int actionIndex = -1;
			ERuleButtonAction action = ERuleButtonAction.Nothing;
			int idx = 0;
			foreach (var sizeRule in sizeRuleList)
			{
				ERuleButtonAction tmpAction = this.OnGUISizeRule(sizeRule, (idx == 0), (idx == (sizeRuleList.Count - 1)));
				if (tmpAction != ERuleButtonAction.Nothing)
				{
					action = tmpAction;
					actionIndex = idx;
				}
				++idx;
			}
			if (actionIndex >= 0)
			{
				switch (action)
				{
					case ERuleButtonAction.Delete:
						this.sizeRuleList.RemoveAt(actionIndex);
						break;
					case ERuleButtonAction.Downer:
						SwapSizeRule(this.sizeRuleList, actionIndex, actionIndex + 1);
						break;
					case ERuleButtonAction.Upper:
						SwapSizeRule(this.sizeRuleList, actionIndex, actionIndex - 1);
						break;
				}
			}
			if (GUILayout.Button(LanguageData.GetString(ETags.AddRuleBtn)))
			{
				this.sizeRuleList.Add(new TextureImportMaxSizeChangeRule("**", TextureImportMaxSizeChangeRule.EParameterFormat.Pixel, 256));
			}
			EditorGUILayout.EndScrollView();
		}

		private void OnGUIFormatRuleList()
		{
			scrollFormatParam = EditorGUILayout.BeginScrollView(scrollFormatParam);
			int actionIndex = -1;
			ERuleButtonAction action = ERuleButtonAction.Nothing;
			int idx = 0;
			foreach (var formatRule in this.formatRuleList)
			{
				ERuleButtonAction tmpAction = this.OnGUIFormatRule(formatRule, (idx == 0), (idx == (this.formatRuleList.Count - 1)));
				if (tmpAction != ERuleButtonAction.Nothing)
				{
					action = tmpAction;
					actionIndex = idx;
				}
				++idx;
			}
			if (GUILayout.Button(LanguageData.GetString(ETags.AddRuleBtn)))
			{
				this.formatRuleList.Add(new TextureImportFormatChangeRule("**", this.AllowFormatList[0], 50));
			}
			if (actionIndex >= 0)
			{
				switch (action)
				{
					case ERuleButtonAction.Delete:
						this.formatRuleList.RemoveAt(actionIndex);
						break;
					case ERuleButtonAction.Downer:
						SwapFormatRule(this.formatRuleList, actionIndex, actionIndex + 1);
						break;
					case ERuleButtonAction.Upper:
						SwapFormatRule(this.formatRuleList, actionIndex, actionIndex - 1);
						break;
				}
			}
			EditorGUILayout.EndScrollView();
		}

		private ERuleButtonAction OnGUISizeRule(TextureImportMaxSizeChangeRule sizeRule, bool isTop, bool isBottom)
		{
			ERuleButtonAction action = ERuleButtonAction.Nothing;
			EditorGUILayout.BeginHorizontal();
			if (isTop)
			{
				GUILayout.Label("", GUILayout.Width(20));
			}
			else
			{
				if (GUILayout.Button(LanguageData.GetString(ETags.UpperBtn), GUILayout.Width(20)))
				{
					action = ERuleButtonAction.Upper;
				}
			}
			if (isBottom)
			{
				GUILayout.Label("", GUILayout.Width(20));
			}
			else
			{
				if (GUILayout.Button(LanguageData.GetString(ETags.DownerBtn), GUILayout.Width(20)))
				{
					action = ERuleButtonAction.Downer;
				}
			}

			sizeRule.pathMatch = EditorGUILayout.TextField(sizeRule.pathMatch);
			int typeIdx = sizeRule.GetRulePopupIndex();
			typeIdx = EditorGUILayout.Popup(typeIdx, sizeRuleStrList, GUILayout.Width(120.0f));
			sizeRule.SetRulePopupIndex(typeIdx);

			if (sizeRule.parameterFormat == TextureImportMaxSizeChangeRule.EParameterFormat.Pixel)
			{
				int pixelIdx = sizeRule.GetMaxPixelPopupIndex();
				pixelIdx = EditorGUILayout.Popup(pixelIdx, TextureImportMaxSizeChangeRule.MaxPixelSizeList, GUILayout.Width(80.0f));
				sizeRule.SetMaxPixelPopupIndex(pixelIdx);
			}
			else
			{
				int divIdx = sizeRule.GetDivParamPopupIndex();
				divIdx = EditorGUILayout.Popup(divIdx, TextureImportMaxSizeChangeRule.DivParamSizeList, GUILayout.Width(80.0f));
				sizeRule.SetDivParamPopupIndex(divIdx);
			}
			if (GUILayout.Button(LanguageData.GetString(ETags.DeleteBtn), GUILayout.Width(50.0f)))
			{
				action = ERuleButtonAction.Delete;
			}
			EditorGUILayout.EndHorizontal();
			return action;
		}


		private ERuleButtonAction OnGUIFormatRule(TextureImportFormatChangeRule formatRule, bool isTop, bool isBottom)
		{
			ERuleButtonAction action = ERuleButtonAction.Nothing;
			EditorGUILayout.BeginHorizontal();
			if (isTop)
			{
				GUILayout.Label("", GUILayout.Width(20));
			}
			else
			{
				if (GUILayout.Button(LanguageData.GetString(ETags.UpperBtn), GUILayout.Width(20)))
				{
					action = ERuleButtonAction.Upper;
				}
			}
			if (isBottom)
			{
				GUILayout.Label("", GUILayout.Width(20));
			}
			else
			{
				if (GUILayout.Button(LanguageData.GetString(ETags.DownerBtn), GUILayout.Width(20)))
				{
					action = ERuleButtonAction.Downer;
				}
			}

			formatRule.pathMatch = EditorGUILayout.TextField(formatRule.pathMatch);

			int selectFormatIdx = GetPopFormatRuleUpIndex(this.AllowFormatList, formatRule.textureFormat);
			selectFormatIdx = EditorGUILayout.Popup(selectFormatIdx, this.AllowFormatStringList, GUILayout.Width(120.0f));
			formatRule.textureFormat = this.AllowFormatList[selectFormatIdx];

			if (IsNeedQualityFormat(formatRule.textureFormat))
			{
				formatRule.quality = EditorGUILayout.IntField(formatRule.quality, GUILayout.Width(40));
				formatRule.quality = Mathf.Clamp(formatRule.quality, 0, 100);
			}
			else
			{
				EditorGUILayout.LabelField("", GUILayout.Width(40));
			}

			if (GUILayout.Button(LanguageData.GetString(ETags.DeleteBtn), GUILayout.Width(50.0f)))
			{
				action = ERuleButtonAction.Delete;
			}
			EditorGUILayout.EndHorizontal();
			return action;
		}

		private int GetPopFormatRuleUpIndex(TextureImporterFormat[] allowList, TextureImporterFormat selectFormat)
		{
			int length = allowList.Length;
			for (int i = 0; i < length; ++i)
			{
				if (allowList[i] == selectFormat)
				{
					return i;
				}
			}
			return 0;
		}


		private void SwapSizeRule(List<TextureImportMaxSizeChangeRule> rules, int idx1, int idx2)
		{
			if (rules == null ||
				idx1 < 0 || idx2 < 0 ||
				idx1 >= rules.Count || idx2 >= rules.Count)
			{
				return;
			}
			TextureImportMaxSizeChangeRule tmp = rules[idx1];
			rules[idx1] = rules[idx2];
			rules[idx2] = tmp;
		}


		private void SwapFormatRule(List<TextureImportFormatChangeRule> rules, int idx1, int idx2)
		{
			if (rules == null ||
				idx1 < 0 || idx2 < 0 ||
				idx1 >= rules.Count || idx2 >= rules.Count)
			{
				return;
			}
			TextureImportFormatChangeRule tmp = rules[idx1];
			rules[idx1] = rules[idx2];
			rules[idx2] = tmp;
		}

		private void ChangeImportSettings(string platform, List<TextureImportMaxSizeChangeRule> sizeRules, List<TextureImportFormatChangeRule> formatRules)
		{
			try
			{
				var guids = AssetDatabase.FindAssets("t:texture2D", null);
				int idx = 0;
				foreach (var guid in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
					++idx;
					EditorUtility.DisplayProgressBar("Progress", platform + " " + path, (float)idx / (float)guids.Length);
					if (textureImporter != null)
					{
						var sizeRule = GetSizeRule(path, sizeRules);
						int size = GetTextureSizeFromRule(textureImporter, sizeRule);
						var formatRule = GetFormatRule(path, formatRules);
						int textureQuality = 50;
						TextureImporterFormat importFormat = TextureImporterFormat.AutomaticCompressed;
						if (formatRule != null)
						{
							importFormat = formatRule.textureFormat;
							textureQuality = formatRule.quality;
						}
						int currentSettingSize = 0;
						TextureImporterFormat currentSettingFormat = TextureImporterFormat.AutomaticCompressed;
						int currentTextureQuality = 50;
						textureImporter.GetPlatformTextureSettings(platform, out currentSettingSize, out currentSettingFormat, out currentTextureQuality);

						if (currentSettingSize != size || currentSettingFormat != importFormat ||
							(IsNeedQualityFormat(importFormat) && textureQuality != currentTextureQuality))
						{
							if (IsNeedQualityFormat(importFormat))
							{
								textureImporter.SetPlatformTextureSettings(
									platform,
									size,
									importFormat, textureQuality, false);
							}
							else
							{
								textureImporter.SetPlatformTextureSettings(
									platform,
									size,
									importFormat);
							}
							EditorUtility.SetDirty(textureImporter);
							textureImporter.SaveAndReimport();
						}
					}
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}


		private void LoadRuleList()
		{
			this.LoadSizeRuleList();
			this.LoadFormatRuleList();
		}

		private void LoadSizeRuleList()
		{
			this.sizeRuleList = new List<TextureImportMaxSizeChangeRule>();

			if (File.Exists(this.SizeRuleFilePath))
			{
				String[] lines = File.ReadAllLines(this.SizeRuleFilePath);
				if (lines != null)
				{
					foreach (var line in lines)
					{
						string[] p = line.Split('\t');
						if (p.Length < 3)
						{
							continue;
						}
						var rule = new TextureImportMaxSizeChangeRule(p[0], (TextureImportMaxSizeChangeRule.EParameterFormat)Int32.Parse(p[1]), Int32.Parse(p[2]));
						this.sizeRuleList.Add(rule);
					}
				}
			}
			if (sizeRuleList.Count == 0)
			{
				this.sizeRuleList.Add(new TextureImportMaxSizeChangeRule("**", TextureImportMaxSizeChangeRule.EParameterFormat.Pixel, 128));
			}
		}

		private void LoadFormatRuleList()
		{
			this.formatRuleList = new List<TextureImportFormatChangeRule>();
			if (File.Exists(this.FormatRuleFilePath))
			{
				String[] lines = File.ReadAllLines(this.FormatRuleFilePath);
				if (lines != null)
				{
					foreach (var line in lines)
					{
						string[] p = line.Split('\t');
						if (p.Length < 3)
						{
							continue;
						}
						var rule = new TextureImportFormatChangeRule(p[0], (TextureImporterFormat)(Int32.Parse(p[1])), Int32.Parse(p[2]));
						this.formatRuleList.Add(rule);
					}
				}
			}
			if (formatRuleList.Count == 0)
			{
				this.formatRuleList.Add(new TextureImportFormatChangeRule("**", TextureImporterFormat.AutomaticCompressed, 80));
			}
		}

		private void SaveRuleList()
		{
			this.SaveSizeRuleList();
			this.SaveFormatRuleList();
		}

		private void SaveSizeRuleList()
		{
			if (this.sizeRuleList == null)
			{
				return;
			}
			string[] lines = new string[sizeRuleList.Count];
			int idx = 0;
			foreach (var sizeRule in sizeRuleList)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(sizeRule.pathMatch).Append("\t");
				sb.Append((int)sizeRule.parameterFormat).Append("\t");
				sb.Append(sizeRule.parameter);
				lines[idx] = sb.ToString();
				++idx;
			}
			File.WriteAllLines(this.SizeRuleFilePath, lines);
		}

		private void SaveFormatRuleList()
		{
			if (this.formatRuleList == null)
			{
				return;
			}
			if (this.formatRuleList.Count == 0)
			{
				File.WriteAllText(this.FormatRuleFilePath, "no_rule");
			}
			string[] lines = new string[formatRuleList.Count];
			int idx = 0;
			foreach (var formatRule in this.formatRuleList)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(formatRule.pathMatch).Append("\t");
				sb.Append((int)formatRule.textureFormat).Append("\t");
				sb.Append(formatRule.quality).Append("\t");
				lines[idx] = sb.ToString();
				++idx;
			}
			File.WriteAllLines(this.FormatRuleFilePath, lines);
		}

		private static TextureImportMaxSizeChangeRule GetSizeRule(string path, List<TextureImportMaxSizeChangeRule> ruleList)
		{
			foreach (var rule in ruleList)
			{
				Regex reg = GlobToRegex(rule.pathMatch);
				if (reg.IsMatch(path))
				{
					return rule;
				}
			}
			return null;
		}

		private static TextureImportFormatChangeRule GetFormatRule(string path, List<TextureImportFormatChangeRule> ruleList)
		{
			if (ruleList == null) { return null; }
			foreach (var rule in ruleList)
			{
				if (rule == null) { continue; }
				Regex reg = GlobToRegex(rule.pathMatch);
				if (reg.IsMatch(path))
				{
					return rule;
				}
			}
			return null;
		}

		public static Regex GlobToRegex(string wildcard)
		{
			string pattern = string.Format("^{0}$", Regex.Escape(wildcard)
				.Replace(@"\*\*", @"(.*)")
				.Replace(@"\*", @"([^/]*)")
				.Replace(@"\?", "([^/].)"));
			return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		private static int GetTextureSizeFromRule(TextureImporter importer, TextureImportMaxSizeChangeRule sizeRule)
		{
			if (sizeRule == null || sizeRule.parameter == 0)
			{
				return 1024;
			}
			if (sizeRule.parameterFormat == TextureImportMaxSizeChangeRule.EParameterFormat.Pixel)
			{
				return sizeRule.parameter;
			}
			else
			{
				return importer.maxTextureSize / sizeRule.parameter;
			}
		}

		private void ExecValidateDefaultTextureSize()
		{
			var guids = AssetDatabase.FindAssets("t:texture2D", null);
			int idx = 0;
			try
			{
				foreach (var guid in guids)
				{
					ValidateDefaultTextureMaxSize(guid);
					EditorUtility.DisplayProgressBar("Progress", " Texture default setting progress", (float)idx / (float)guids.Length);
					++idx;
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			EditorUtility.ClearProgressBar();
		}

		private void ValidateDefaultTextureMaxSize(string guid)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			if (textureImporter == null)
			{
				return;
			}
			bool flag = false;
			int w = 0;
			int h = 0;
			int size = 0;

			var data = File.ReadAllBytes(path);
			if (IsPngData(data))
			{
				GetPngSize(data, out w, out h);
				flag = true;
			}
			else
			{
			}

			if (flag)
			{
				size = GetNearMaxSize(w, h);
				if (textureImporter.maxTextureSize != size)
				{
					textureImporter.maxTextureSize = size;
					EditorUtility.SetDirty(textureImporter);
					textureImporter.SaveAndReimport();
				}
			}
			else
			{
				Debug.LogWarning("Texture Default setting Not Process " + path);
			}
		}


		private int GetNearMaxSize(int w, int h)
		{
			int tmpSize = w;
			if (w < h)
			{
				tmpSize = h;
			}
			for (int i = 1; i < 12; ++i)
			{
				if (tmpSize <= (1 << i))
				{
					return (1 << i);
				}
			}
			return 4096;
		}

		private static bool IsPngData(byte[] data)
		{
			if (data == null || data.Length < PngHeaderData.Length)
			{
				return false;
			}
			int length = PngHeaderData.Length;
			for (int i = 0; i < length; ++i)
			{
				if (data[i] != PngHeaderData[i])
				{
					return false;
				}
			}
			return true;
		}

		private static void GetPngSize(byte[] data, out int width, out int height)
		{
			width = 0;
			height = 0;
			if (data == null || data.Length < 33)
			{
				return;
			}
			int idx = 16;
			width = (data[idx + 0] << 24) +
				(data[idx + 1] << 16) +
				(data[idx + 2] << 8) +
				(data[idx + 3] << 0);
			idx = 20;
			height = (data[idx + 0] << 24) +
				(data[idx + 1] << 16) +
				(data[idx + 2] << 8) +
				(data[idx + 3] << 0);
		}

		private static bool IsNeedQualityFormat(TextureImporterFormat f)
		{
			switch (f)
			{
				case TextureImporterFormat.DXT1Crunched:
				case TextureImporterFormat.DXT5Crunched:
				case TextureImporterFormat.ETC_RGB4:
				case TextureImporterFormat.ETC2_RGB4:
				case TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA:
				case TextureImporterFormat.ETC2_RGBA8:
				case TextureImporterFormat.PVRTC_RGB2:
				case TextureImporterFormat.PVRTC_RGB4:
				case TextureImporterFormat.PVRTC_RGBA2:
				case TextureImporterFormat.PVRTC_RGBA4:
					return true;
			}
			return false;
		}
	}
}