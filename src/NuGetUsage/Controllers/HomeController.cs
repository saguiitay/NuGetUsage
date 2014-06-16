using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using NuGetUsage.Models;
using NuGetUsage.RavenDb;
using NuGetUsage.Service;
using NuGetUsage.ViewModels;
using Raven.Client;
using Raven.Client.Linq;

namespace NuGetUsage.Controllers
{
	public class HomeController : BaseController
	{
		public const int MinDaysBetweenImports = 5;
		public const int SuggestImportAfterDays = 5;

		public async Task<ActionResult> Index(RepoViewModel model)
		{
			IEnumerable<PackageRef> packages = null;
			IEnumerable<string> suggestionsQueries = null;
			IQueryable<Repo> reposQuery;

			Repo repo = GetRepo(model.Query, out reposQuery);
			var query = model.Query;
			if (repo != null)
			{
				var packagesByUsage = DocumentSession.Query<PackageRefUsage, Repos_PackagesUsage>()
					.Include<PackageRefUsage>(x => x.Repos)
					.Where(x => x.Repos.Contains(repo.FullName));

				Expression<Func<PackageRefUsage, object>> fieldExpression = null;
				switch (model.SortField)
				{
					case SortField.Name:
						fieldExpression = x => x.PackageName;
						break;
					case SortField.Popularity:
						fieldExpression = x => x.Count;
						break;
				}

				if (model.SortOrder == SortOrder.Asc)
					packagesByUsage = packagesByUsage.OrderBy(fieldExpression);
				else
					packagesByUsage = packagesByUsage.OrderByDescending(fieldExpression);

				var packagesNames = packagesByUsage.ToList().Select(p => "Packages/" + p.PackageName).ToList();

				packages = DocumentSession.Load<Package>(packagesNames).ToList();

				if (repo.UpdatedFromGitHub == null || repo.UpdatedFromGitHub.Value.AddDays(5) < DateTime.UtcNow)
				{
					Task.Factory.StartNew(async () =>
					    {
					        var dataService = new DataService();
							await dataService.AddRepositoryInfo(query);
						});
				}

			}
			else
			{
				var repoByNameQuery = DocumentSession.Query<Repo>()
					.Include<Repo>(x => x.Packages.Select(p => p.Id))
					.Where(x => x.Name == query);

				if (repoByNameQuery.Any())
				{
					suggestionsQueries = repoByNameQuery.Take(5).Select(r => r.FullName);
				}
				else if (reposQuery != null)
				{
					var suggestions = reposQuery.Suggest();
					suggestionsQueries = suggestions.Suggestions;
				}
			}

			var viewModel = new RepoViewModel
				{
					Repo = repo,
					Packages = packages,

					Query = query,
					SortField = model.SortField,
					SortOrder = model.SortOrder,

					AllowImport = IsAdmin || (repo == null && await new GitHubService().IsValidRepositoryName(query)),
					Suggestions = suggestionsQueries,

                    AdminKey = model.AdminKey
				};

			return View(viewModel);
		}

		public async Task<ActionResult> Import(RepoViewModel model)
		{
			if (!IsAdmin)
			{
				IQueryable<Repo> x;
				var repo = GetRepo(model.Query, out x);
				
				if (repo != null && repo.UpdatedFromGitHub != null)
				{
					if (repo.UpdatedFromGitHub.Value.AddDays(MinDaysBetweenImports) < DateTime.UtcNow)
					{
						return RedirectToAction("Index", model);
					}
				}
			}
		    var dataService = new DataService();
			await dataService.AddRepositoryInfo(model.Query);

			return RedirectToAction("Index", model);
		}

		[ChildActionOnly]
		[OutputCache(Duration = 60 * 5)]
		public async Task<ActionResult> Stats()
		{
			RavenQueryStatistics reposStatistics;
			RavenQueryStatistics packagesStatistics;
			var x = DocumentSession.Query<Repo>().Statistics(out reposStatistics).Take(1).Lazily();
			DocumentSession.Query<Package>().Statistics(out packagesStatistics).Take(1).Lazily();

			x.Value.ToList();

			var model = new StatsViewModel
				{
					ReposCount = reposStatistics.TotalResults,
					PackagesCount = packagesStatistics.TotalResults
				};
			return PartialView(model);
		}


		private Repo GetRepo(string query, out IQueryable<Repo> repoByFullNameQuery)
		{
			repoByFullNameQuery = null;

			if (string.IsNullOrWhiteSpace(query))
				return null;

			query = query.Trim();

			repoByFullNameQuery = DocumentSession.Query<Repo>()
				.Include<Repo>(x => x.Packages.Select(p => p.Id))
				.Where(x => x.FullName == query);

			return repoByFullNameQuery.FirstOrDefault();
		}
	}
}