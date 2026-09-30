using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LocalAIAssistant.Infrastructure
{
	// 簡體轉繁體工具，直接呼叫 Windows 內建的 LCMapStringEx（kernel32），不需要額外套件。
	// 只做字對字的轉換，不處理詞彙差異（例如「软件」會變「軟件」而不是「軟體」）。
	// 因為用了 Win32 API，只能在 Windows 上執行。
	internal class ChineseConverter
	{
		// LCMapStringEx 的旗標：把字串對應成繁體中文
		private const uint LCMAP_TRADITIONAL_CHINESE = 0x04000000;

		// P/Invoke 宣告。CharSet.Unicode 讓 string 以 UTF-16 傳給 W 版 API；
		// SetLastError = true 才能在失敗後用 Marshal.GetLastWin32Error() 取得錯誤碼。
		// lpDestStr 標 [Out] 且允許 null，配合下面「先問大小再配置」的兩段式呼叫。
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int LCMapStringEx(
			string lpLocaleName, uint dwMapFlags,
			string lpSrcStr, int cchSrc,
			[Out] char[]? lpDestStr, int cchDest,
			IntPtr lpVersionInformation, IntPtr lpReserved, IntPtr sortHandle);

		// 把字串中的簡體字轉成繁體字；非中文字元原樣保留。
		public static string ToTraditional(string text)
		{
			// 空字串不用轉換，直接回傳。
			// 而且 LCMapStringEx 收到長度 0 時會回傳 0，會被下面當成「失敗」
			if (string.IsNullOrEmpty(text))
				return text;

			// 第一次呼叫：緩衝區傳 null、大小傳 0，問「需要多大的緩衝區？」
			int size = LCMapStringEx("zh-TW", LCMAP_TRADITIONAL_CHINESE,
				text, text.Length,
				null, 0,
				IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			// 回傳 0 代表失敗，用 GetLastError 的錯誤碼丟出例外（fail fast）
			if (size == 0)
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

			// 配置剛好那麼大的緩衝區
			char[] buffer = new char[size];

			// 第二次呼叫：把結果寫進緩衝區
			int written = LCMapStringEx("zh-TW", LCMAP_TRADITIONAL_CHINESE,
				text, text.Length,
				buffer, buffer.Length,
				IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (written == 0)
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

			// 只取實際寫入的長度，轉成 C# 字串
			return new string(buffer, 0, written);
		}
	}
}
