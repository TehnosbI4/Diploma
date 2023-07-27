using System.Dynamic;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace MovementMonitoring.Utilities
{
	public static class Configurator
	{
		private static dynamic? _config;
		public static dynamic YamlConfig 
		{
			get
			{
				if (_config == null)
				{
                    IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                    _config = deserializer.Deserialize<ExpandoObject>(File.ReadAllText("Properties\\Config.yml"));
					return _config;
                }
				else
				{
                    return _config;
                }
			}
		}
	}
}
