using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PowerShellLanguageTests
{
	public class SampleCode
	{
		static void TestConditionExpressions()
		{
			string b = null;

			string c = b ?? "foo";
			string d = b != null ? b : "bar";
		}

		static void start(string cmd)
		{
			Process.Start(cmd);
			string b = null;

			string c = b ?? "foo";
			string d = b != null ? b : "bar";

			Type t = typeof(string);
			if (t == typeof(int))
				Console.WriteLine("foo");
		}

		static void foo()
		{
			string b = null;
			string d = b != null ? b : "bar";
		}
	}
}
