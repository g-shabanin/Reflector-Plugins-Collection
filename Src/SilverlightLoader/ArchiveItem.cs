namespace Reflector.SilverlightLoader
{
	internal class ArchiveItem
	{
		private string name;
		private byte[] value;

		public string Name 
		{
			get 
			{
				return this.name;
			}
			
			set
			{
				this.name = value;	
			}
		}
		
		public byte[] Value
		{
			get
			{
				return this.value;	
			}	
			
			set
			{
				this.value = value;	
			}
		}
	}
}
