namespace Reflector
{
	using System;

	// This example shows how to remote control a Reflector window 
	// from another application by sending messages.

	internal class Example
	{
		public static void Main()
		{
			// IsAvailable
			Console.Write("Available = ");
			Console.WriteLine(Reflector.RemoteController.Available);
					
			// LoadAssembly
			Console.Write("Load Assembly [Press Enter]");
			Console.ReadLine();
			Reflector.RemoteController.LoadAssembly(typeof(Example).Module.FullyQualifiedName);
	
			// UnloadAssembly
			Console.Write("Unload Assembly [Press Enter]");
			Console.ReadLine();
			Reflector.RemoteController.UnloadAssembly(typeof(Example).Module.FullyQualifiedName);
	
			// SelectTypeDeclaration
			Console.Write("Select Type [Press Enter]");
			Console.ReadLine();
			Reflector.RemoteController.Select("code://mscorlib/System.Object");
	
			// SelectMethodDeclaration
			Console.Write("Select Method [Press Enter]");
			Console.ReadLine();
			Reflector.RemoteController.Select("M:System.Text.StringBuilder.Append(System.Int16)");
		}
	}
}
