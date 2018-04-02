﻿namespace GithubX.UWP.Models
{
	class ContentModel
	{

		public string name { get; set; }
		public string path { get; set; }
		public string sha { get; set; }
		public int size { get; set; }
		public string url { get; set; }
		public string html_url { get; set; }
		public string git_url { get; set; }
		public string download_url { get; set; }
		public string type { get; set; }
		public _Links _links { get; set; }
		public string content { get; set; }
		public string encoding { get; set; }

		public class _Links
		{
			public string self { get; set; }
			public string git { get; set; }
			public string html { get; set; }
		}

		[Newtonsoft.Json.JsonIgnore]
		public string Icon => type == "dir" ? "" : "";


	}
}
