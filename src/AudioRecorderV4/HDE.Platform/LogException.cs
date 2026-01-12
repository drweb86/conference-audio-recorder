using System;

namespace HDE.Platform.Logging
{
	[Serializable]
	public sealed class LogException: Exception
	{
		public LogException()
		{ 
		
		}


		public LogException(string message)
			: base(message)
		{

		}

		public LogException(string message, Exception innerException)
			: base(message, innerException)
		{ 
		
		}
	}
}
