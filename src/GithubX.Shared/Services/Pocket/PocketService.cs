using System.Reactive.Linq;
using Akavache;
//using Refit;
using System;
using System.Threading.Tasks;

namespace GithubX.Shared.Services.Pocket
{
	public class PocketService
	{
		private readonly IPocketApi Api;//= RestService.For<IPocketApi>("https://getpocket.com/v3/");
		private string ApiKey { get; } //consumer_key
		private string AccessToken { get; set; }
		public string FallBackUri { get; } = "githubx://pocket";

		/// <summary>
		/// Initializes a new instance of the <see cref="PocketService"/> class.
		/// </summary>
		/// <param name="apiKey"></param>
		/// <param name="accessToken"></param>
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
		public PocketService(string apiKey, string? accessToken)
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
		{
			ApiKey = apiKey;
			if (accessToken != null) AccessToken = accessToken;
		}

		public async Task<(string? requestCode, Uri uri)> GenerateAuthUri()
		{
			RequestCode? requestCode = await Api.GetRequestToken(FallBackUri, ApiKey);
			if (requestCode == null) throw new NullReferenceException("Null request_code");
			const string authentificationUri = "https://getpocket.com/auth/authorize?request_token={0}&redirect_uri={1}&mobile={2}&force={3}&webauthenticationbroker={4}";
			return (requestCode.Code, new Uri(string.Format(authentificationUri, requestCode.Code, FallBackUri, "1", "login", "1")));
		}

		public async Task<string?> GetUserToken(string requestCode)
		{
			if (requestCode == null) throw new NullReferenceException("Call GetRequestCode() first to receive a request_code");
			var user = await Api.GetUserToken(requestCode, ApiKey);
			return AccessToken = user.Access_token;
		}

		public async Task Add(Uri uri)
		{
			await Api.PostArticle(new AddParameters(uri.AbsoluteUri, ApiKey, AccessToken));
		}

		public async void LoadFromCache()
		{
			AccessToken = await BlobCache.UserAccount.GetObject<string>("pocket");
		}

		public async void SaveInCache()
		{
			await BlobCache.UserAccount.InsertObject("pocket", AccessToken);
		}

		public bool IsLoggedIn()
		{
			return AccessToken.Length > 1;
		}
	}
}
