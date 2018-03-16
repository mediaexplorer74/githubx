﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GithubX.UWP.Models;
using GithubX.UWP.Services.Cache;
using Newtonsoft.Json;

namespace GithubX.UWP.Services.Api
{
	static class ApiHandler
	{
		static WindowsCacheHandler wCache = new WindowsCacheHandler();
		static LocalCacheHandler lCache = new LocalCacheHandler();

		#region Category
		internal static async Task SaveCategoriesAsync(string userLoginAccountName, List<CategoryModel> categories)
		{
			await lCache.SaveAsync(CacheKeys.CategoriesKey(userLoginAccountName), JsonConvert.SerializeObject(categories)).ConfigureAwait(false);
		}

		internal static async Task<List<CategoryModel>> GetCategoriesAsync(string userId)
		{
			var cats = new List<CategoryModel>();
			var keys = CacheKeys.CategoriesKey(userId);
			try
			{
				var json = await lCache.ReadAsync(keys); // if does not exist will return null and 
				cats = JsonConvert.DeserializeObject<List<CategoryModel>>(json); //then throw exception
				return cats.OrderBy(o => o.OrderId).ToList();
			}
			catch (Exception)
			{
				cats.Add(new CategoryModel { Id = 0, Text = "All" });
				await SaveCategoriesAsync(userId, cats);
				return cats;
			}
		}
		#endregion

		#region Repositories
		internal static List<RepoModel> AllRepos { get; set; }

		internal static List<RepoModel> GetRepoOfCategory(int catId) => AllRepos.FindAll(obj => obj.CategoriesId.Contains(catId));

		internal static async Task<List<RepoModel>> GetNextPageReposAsync(string userAcc, int page)
		{
			if (!HttpHandler.CheckConnection) throw new Exception("No internet, no candy for you!🤬");
			var json = await HttpHandler.Get(Api.AccountStarsUrl(userAcc, page));
			if (json == null) throw new Exception("Oops!🤨🤔");
			var freshList = JsonConvert.DeserializeObject<List<RepoModel>>(json);
			if (page != 0) foreach (var r in freshList) r.CategoriesId = new int[] { };
			return freshList;
		}

		internal static async Task PrepareAllRepos(string userAcc, bool cacheEnable = true)
		{
			if (!HttpHandler.CheckConnection && !cacheEnable) throw new Exception("No internet, no candy for you!🤬");
			if (HttpHandler.CheckConnection)
			{
				try
				{
					var freshList = await GetNextPageReposAsync(userAcc, 0);
					try
					{
						var cacheList = await LoadFromCache();
						AllRepos = MergeOldAndNew(cacheList, freshList);
					}
					catch { AllRepos = freshList; }
					await SaveCategoryReposAsync(userAcc, AllRepos);
					return;
				}
				catch { }
			}
			else AllRepos = await LoadFromCache();// only cache - if cache=ON or online failes

			List<RepoModel> MergeOldAndNew(List<RepoModel> old, List<RepoModel> fresh)
			{
				//TODO: dirty dirty code :( dont like it
				var temp = new List<RepoModel>();
				foreach (var item in old)
				{
					var ls = new List<int>(item.CategoriesId);
					//if [] goodbye cache
					if (ls.Count == 1) if (ls[0] == 0) continue;
					// cache.del only [0]
					if (ls.Contains(0)) ls.Remove(0);

					// cache.update [0....] if contain.fresh ?nothing else replace with [...]
					var newItem = fresh.Find(obj => obj.id == item.id);//>change
					if (newItem != null)
					{
						ls.Add(0);
						newItem.CategoriesId = ls.ToArray();
						continue;
					}
					temp.Add(item);
				}
				fresh.AddRange(temp);
				return fresh;
			}
			async Task<List<RepoModel>> LoadFromCache()
			{
				var json = await lCache.ReadAsync(CacheKeys.RepositoriesKey(userAcc)).ConfigureAwait(false);
				if (json == null) throw new Exception("Oops!🤨🤔");
				return JsonConvert.DeserializeObject<List<RepoModel>>(json);
			}
		}

		internal static async Task SaveCategoryReposAsync(string user, List<RepoModel> cat)
		{
			var temp = cat.FindAll(x => x.CategoriesId != new int[] { });
			await lCache.SaveAsync(CacheKeys.RepositoriesKey(user), JsonConvert.SerializeObject(temp)).ConfigureAwait(false);
		}
		#endregion

		#region Login
		internal static OwnerModel LoginFromCache()
		{
			var key = CacheKeys.UserKey;
			if (wCache.Exists(key))
			{
				var json = wCache.Read(key);
				if (json.Length < 4) return null;
				return JsonConvert.DeserializeObject<OwnerModel>(json);
			}
			else return null;
		}

		internal static async Task<OwnerModel> LoginAsync(string account)
		{
			var key = CacheKeys.UserKey;
			var json = await HttpHandler.Get(Api.AccountInfoUrl(account));
			if (json == null || json.Length < 4) return null;
			var user = JsonConvert.DeserializeObject<OwnerModel>(json);
			wCache.Save(key, json);
			return user;
		}

		internal static void LogOut()
		{
			wCache.Remove(CacheKeys.UserKey);
		}
		#endregion

		#region ReadMe
		public static async Task<(bool, string)> GetReadMeMdAsync(int repoId, string url, bool fromCache = true)
		{
			string md = null;
			var key = CacheKeys.Readme(repoId);
			if (fromCache)
			{
				try
				{
					md = await lCache.ReadAsync(key);
					if (md != null)
					{
						removeUnSupported();
						await lCache.SaveAsync(key, md).ConfigureAwait(false);
						return (true, md);
					}
				}
				catch { }
			}
			return await LoadOnline();

			async Task<(bool, string)> LoadOnline()
			{
				try
				{
					md = await HttpHandler.GetString(url);
					if (md == null) return (false, "> Nothing");
					md = new Html2Markdown.Converter().Convert(md);
					removeUnSupported();
					await lCache.SaveAsync(key, md).ConfigureAwait(false);
				}
				catch { }
				return (false, md);
			}
			void removeUnSupported()
			{
				//md = md.Replace("- [ ]", "*");
				//md = md.Replace("- [*]", "*");
				md = md.Replace("[`", "[");
				md = md.Replace("`]", "]");
				//md = md.Replace("[x]", "*");
				//md = md.Replace("[ ]", "*");
			}
		}
		#endregion
	}
}
