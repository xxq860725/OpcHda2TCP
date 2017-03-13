using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpcHda2Tcp.Common
{
	public static class NativeMethods
	{
		[DllImport("User32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

		[DllImport("user32.dll", EntryPoint = "RemoveMenu")]
		public static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
	}
}
