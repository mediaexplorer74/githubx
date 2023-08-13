//using Refit;
namespace GithubX.Shared.Services.Pocket
{
	internal class QueryAttribute : Attribute
	{
		private string v;

		public QueryAttribute(string v)
		{
			this.v = v;
		}
	}
}