using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Compilyactor.Code
{
	public class SplashAggregator
	{
		private static Random _random = new Random();
		private string _metadata = "No metadata";
		private static SplashAggregator _instance = new SplashAggregator();
		private Dictionary<string, string[]> _splashes = new Dictionary<string, string[]>();

		public static SplashAggregator Instance { get => _instance; }

		private HttpClient? _http;

		public void Initialize(HttpClient http)
		{
			_http = http ?? throw new ArgumentNullException(nameof(http));
		}

		public string this[string key]
		{
			get
			{
				if(!_splashes.ContainsKey(key))
				{
					StringBuilder sb = new StringBuilder();
					foreach(string k in _splashes.Keys)
					{
						sb.AppendLine(k);
					}
					sb.AppendLine(_metadata);
					return sb.ToString();
				}

				int length = _splashes[key].Length;
				return _splashes[key][_random.Next(length)];
			}
		}

		private SplashAggregator()
		{ }

		public async Task CollectSplashBaseAsync()
		{
			if(_http == null) throw new InvalidOperationException("HttpClient не инициализирован!");

			string path = "texts/splashes/";

			// Используем Service Worker для получения списка файлов
			var response = await _http.GetStringAsync($"{path}?_=");
			var files = ExtractFilesFromHtml(response);

			foreach(var file in files)
			{
				var content = await _http.GetStringAsync($"{path}{file}");
				_splashes[file.Replace(".txt", "")] = content.Split('\n').Select(s => s.Trim()).ToArray();
			}
		}

		private List<string> ExtractFilesFromHtml(string html)
		{
			return html.Split(new[] { "href=\"" }, StringSplitOptions.None)
					   .Skip(1)
					   .Select(part => part.Split('"')[0])
					   .Where(name => name.EndsWith(".txt"))
					   .ToList();
		}
	}
}
