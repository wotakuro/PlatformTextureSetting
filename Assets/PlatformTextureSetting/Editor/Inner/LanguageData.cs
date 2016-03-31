using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatformTextureSetting
{
	public enum ELanguage
	{
		English,
		Japanese,
	}
	public enum ETags
	{
		Title,
		LoadBtn,
		SaveBtn,
		ExecBtn,
		ValidateDefault,
		ValidateBtn,
		RuleList,
		TextureSizeRule,
		FormatRule,
		MaxPixel,
		RateFromDefault,
		AddRuleBtn,
		DeleteBtn,
		UpperBtn,
		DownerBtn,
		OkBtn,
		CancelBtn,
		ExecDialogTitle,
		ExecDialogMessage,
		LoadDialogTitle,
		LoadDialogMessage,
		SaveDialogTitle,
		SaveDialogMessage,
		ValidateDialogTitle,
		ValidateDialogMessage,
	}

	public class LanguageData
	{
		private static Dictionary<ELanguage ,Dictionary<ETags,string > >dictionary;
		

		public static ELanguage Current = ELanguage.English;
		static LanguageData(){
			dictionary= new Dictionary<ELanguage,Dictionary<ETags,string>>();
			InitJapaneseDictionary();
			InitEnglishDictionary();
		}


		private static void InitEnglishDictionary()
		{
			var tmpDict = new Dictionary<ETags, string>();
			tmpDict.Add(ETags.Title, "{0} ");
			tmpDict.Add(ETags.LoadBtn, "Load rule");
			tmpDict.Add(ETags.SaveBtn, "Save rule");
			tmpDict.Add(ETags.ExecBtn, "Run");
			tmpDict.Add(ETags.ValidateBtn, "Validate");
			tmpDict.Add(ETags.TextureSizeRule, "Max size rule");
			tmpDict.Add(ETags.AddRuleBtn, "Add rule");
			tmpDict.Add(ETags.FormatRule, "Format rule");
			tmpDict.Add(ETags.DeleteBtn, "del");
			tmpDict.Add(ETags.UpperBtn, "↑");
			tmpDict.Add(ETags.DownerBtn, "↓");
			tmpDict.Add(ETags.OkBtn, "OK");
			tmpDict.Add(ETags.CancelBtn, "Cancel");
			tmpDict.Add(ETags.ValidateDefault, "Validate default max size of texture");
			tmpDict.Add(ETags.MaxPixel, "MaxPixel");
			tmpDict.Add(ETags.RateFromDefault, "Rate from default");
			tmpDict.Add(ETags.ExecDialogTitle, "{0} ");
			tmpDict.Add(ETags.ExecDialogMessage, "Is it ok to run？\nWhen this was run,texture platform size will be change.\nThe size is detectd by texture file path.");
			tmpDict.Add(ETags.LoadDialogTitle, "Load rule");
			tmpDict.Add(ETags.LoadDialogMessage, "Is it ok to descard changes?");
			tmpDict.Add(ETags.SaveDialogTitle, "Save rule");
			tmpDict.Add(ETags.SaveDialogMessage, "Is it ok to save rules?");
			tmpDict.Add(ETags.ValidateDialogTitle, "Validate default max size of texture");
			tmpDict.Add(ETags.ValidateDialogMessage, "Is it ok to change default max size of texture?");
			tmpDict.Add(ETags.RuleList, "Rule list");
			dictionary.Add(ELanguage.English, tmpDict);
		}
		private static void InitJapaneseDictionary()
		{
			var tmpDict = new Dictionary<ETags, string>();
			tmpDict.Add(ETags.Title, "{0} テクスチャルール");
			tmpDict.Add(ETags.LoadBtn, "設定のロード");
			tmpDict.Add(ETags.SaveBtn,"設定のセーブ");
			tmpDict.Add(ETags.ExecBtn,"実行する");
			tmpDict.Add(ETags.ValidateBtn,"整合性処理を実行する");
			tmpDict.Add(ETags.TextureSizeRule,"テクスチャサイズルール");
			tmpDict.Add(ETags.AddRuleBtn,"ルール追加");
			tmpDict.Add(ETags.FormatRule,"フォーマットルール");
			tmpDict.Add(ETags.DeleteBtn,"削除");
			tmpDict.Add(ETags.UpperBtn,"↑");
			tmpDict.Add(ETags.DownerBtn,"↓");
			tmpDict.Add(ETags.OkBtn,"OK");
			tmpDict.Add(ETags.CancelBtn,"Cancel");
			tmpDict.Add(ETags.ValidateDefault,"デフォルト設定のテクスチャ最大サイズの整合性を取る");
			tmpDict.Add(ETags.MaxPixel,"最大ピクセル指定");
			tmpDict.Add(ETags.RateFromDefault,"デフォ値の何分の1指定");
			tmpDict.Add(ETags.ExecDialogTitle,"{0} テクスチャ設定");
			tmpDict.Add(ETags.ExecDialogMessage,"下記ルールで設定を行いますが宜しいですか？\n上から順にファイルパスのパターンマッチを行います。\n\n※設定のセーブも行います");
			tmpDict.Add(ETags.LoadDialogTitle,"設定のロード");
			tmpDict.Add(ETags.LoadDialogMessage,"現在の編集内容を破棄して設定をロードしますか？");
			tmpDict.Add(ETags.SaveDialogTitle,"設定のセーブ");
			tmpDict.Add(ETags.SaveDialogMessage,"現在の編集内容をセーブしますか？");
			tmpDict.Add(ETags.ValidateDialogTitle,"デフォルト設定のテクスチャ最大サイズの整合処理");
			tmpDict.Add(ETags.ValidateDialogMessage,"デフォルト設定のテクスチャ最大サイズを、実際の画像ファイルから設定しなおしますか？");
			tmpDict.Add(ETags.RuleList, "ルール一覧");
			dictionary.Add(ELanguage.Japanese, tmpDict);
		}

		public static string GetString( ETags tag )
		{
			return GetString(Current, tag);
		}
		public static string GetString( ELanguage lang , ETags tag )
		{
			try
			{
				return dictionary[lang][tag];
			}
			catch
			{
			}
				return "";
		}
	}
}
