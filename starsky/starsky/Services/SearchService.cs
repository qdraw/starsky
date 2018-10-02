﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public class SearchService : ISearch
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public SearchViewModel Search(string query = "", int pageNumber = 0)
        {

            var stopWatch = Stopwatch.StartNew();

            // Create an view model
            var model = new SearchViewModel
            {
                PageNumber = pageNumber,
                SearchQuery = query ?? string.Empty,
                Breadcrumb = new List<string> {"/", query ?? string.Empty  }
                // Null check will safe you from error 500 with Empty request
            };

            if (query == null) return model;

            _orginalSearchQuery = model.SearchQuery;

            model.SearchQuery = QuerySafe(model.SearchQuery);
            model.SearchQuery = QueryShortcuts(model.SearchQuery);
            model = MatchSearch(model);

            WideSearch(_context.FileIndex.AsNoTracking(),model);
            NarrowSearch(model);

            // Remove duplicates from list
            model.FileIndexItems = model.FileIndexItems.GroupBy(s => s.FilePath)
                .Select(grp => grp.FirstOrDefault())
                .OrderBy(s => s.FilePath)
                .ToList();
            
            model.SearchCount = model.FileIndexItems.Count();

            model.FileIndexItems = model.FileIndexItems
                .OrderByDescending(p => p.DateTime)
                .Skip( pageNumber * NumberOfResultsInView )
                .SkipLast( model.SearchCount - (pageNumber * NumberOfResultsInView ) - NumberOfResultsInView ).ToHashSet();

            model.LastPageNumber = GetLastPageNumber(model.SearchCount);

            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
            return model;
        }

        private void WideSearch(IEnumerable<FileIndexItem> sourceList, SearchViewModel model)
        {
            // .AsNoTracking() => never change data to update
            for (var i = 0; i < model.SearchIn.Count; i++)
            {
                var  searchInType = (SearchViewModel.SearchInTypes)
                    Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLower());

                model.SearchFor[i] = model.SearchFor[i].ToLower();

                switch (searchInType)
                {
                    case SearchViewModel.SearchInTypes.description:
                        model.FileIndexItems = model.FileIndexItems.Concat(
                            sourceList.Where(
                                p => p.Description.ToLower().Contains(model.SearchFor[i])
                            ).ToHashSet()
                        );
                        break;
                    case SearchViewModel.SearchInTypes.filename:
                        model.FileIndexItems = model.FileIndexItems.Concat(
                            sourceList.Where(
                                p => p.FileName.ToLower().Contains(model.SearchFor[i])
                            ).ToHashSet()
                        );
                        break;
                    case SearchViewModel.SearchInTypes.filepath:
                        model.FileIndexItems = model.FileIndexItems.Concat(
                            sourceList.Where(
                                p => p.FilePath.ToLower().Contains(model.SearchFor[i])
                            ).ToHashSet()
                        );
                        break;
                    case SearchViewModel.SearchInTypes.parentdirectory:
                        model.FileIndexItems = model.FileIndexItems.Concat(
                            sourceList.Where(
                                p => p.ParentDirectory.ToLower().Contains(model.SearchFor[i])
                            ).ToHashSet()
                        );
                        break;
                    case SearchViewModel.SearchInTypes.title:
                        model.FileIndexItems = model.FileIndexItems.Concat(
                            sourceList.Where(
                                p => p.Title.ToLower().Contains(model.SearchFor[i])
                            ).ToHashSet()
                        );
                        break;
                    
                    case SearchViewModel.SearchInTypes.datetime:

                        DateTime.TryParse(model.SearchFor[i], out var dateTime);
                        if(dateTime.Year < 2) dateTime = DateTime.Now;
                        model.SearchFor[i] = dateTime.ToString(CultureInfo.InvariantCulture);
                        
                        switch (model.SearchForOptions[i])
                        {
                            case "<":
                                model.FileIndexItems = model.FileIndexItems.Concat(
                                    sourceList.Where(
                                        p => p.DateTime <= dateTime
                                    ).ToHashSet()
                                );
                                break;
                            case ">":
                                model.FileIndexItems = model.FileIndexItems.Concat(
                                    sourceList.Where(
                                        p => p.DateTime >= dateTime
                                    ).ToHashSet()
                                );
                                break;
                            default:
                                model.FileIndexItems = model.FileIndexItems.Concat(
                                    sourceList.Where(
                                        p => p.DateTime == dateTime
                                    ).ToHashSet()
                                );
                                break;
                        }

                        break;
                    default:
                        var splitSearchFor = Split(model.SearchFor[i]);

                        foreach (var itemSearchFor in splitSearchFor)
                        {
                            model.FileIndexItems = model.FileIndexItems.Concat(
                                sourceList.Where(
                                    p => p.Tags.ToLower().Contains(itemSearchFor)
                                ).ToHashSet()
                            );
                        }
                    break;
                }
            }
        }

        private void NarrowSearch(SearchViewModel model)
        {
            // Narrow Search
            for (var i = 0; i < model.SearchIn.Count; i++)
            {
                var searchInType = (SearchViewModel.SearchInTypes)
                    Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLower());

                switch (searchInType)
                {
                    case SearchViewModel.SearchInTypes.description:
                        model.FileIndexItems = model.FileIndexItems.Where(
                            p => p.Description.ToLower().Contains(model.SearchFor[i].ToLower())
                        ).ToHashSet();
                        break;

                    case SearchViewModel.SearchInTypes.filename:
                        model.FileIndexItems = model.FileIndexItems.Where(
                            p => p.FileName.ToLower().Contains(model.SearchFor[i].ToLower())
                        ).ToHashSet();
                        break;

                    case SearchViewModel.SearchInTypes.filepath:
                        model.FileIndexItems = model.FileIndexItems.Where(
                            p => p.FilePath.ToLower().Contains(model.SearchFor[i].ToLower())
                        ).ToHashSet();
                        break;

                    case SearchViewModel.SearchInTypes.parentdirectory:
                        model.FileIndexItems = model.FileIndexItems.Where(
                            p => p.ParentDirectory.ToLower().Contains(model.SearchFor[i].ToLower())
                        ).ToHashSet();
                        break;

                    case SearchViewModel.SearchInTypes.tags:
                        // Tags are searched by multiple words

                        var splitSearchFor = Split(model.SearchFor[i]);
                        foreach (var itemSearchFor in splitSearchFor)
                        {
                            model.FileIndexItems = model.FileIndexItems.Where(
                                p => p.Tags.ToLower().Contains(itemSearchFor)
                            ).ToHashSet();
                        }
                        break;

                    case SearchViewModel.SearchInTypes.title:
                        model.FileIndexItems = model.FileIndexItems.Where(
                            p => p.Title.ToLower().Contains(model.SearchFor[i].ToLower())
                        ).ToHashSet();
                        break;
                }
            }
        }

        private List<string> Split(string input)
        {
            return input.ToLower().Split(" ").ToList();
        }



        private string _defaultQuery = string.Empty;
        private string _orginalSearchQuery = string.Empty;

        public SearchViewModel MatchSearch(SearchViewModel model)
        {
            _defaultQuery = model.SearchQuery;

            foreach (var itemName in new FileIndexItem().FileIndexPropList())
            {
                SearchItemName(model, itemName);
            }

            model.SearchQuery = "-Tags:" + "\"" + _defaultQuery.Trim()+ "\""; // escape values for !delete! query
            SearchItemName(model, "Tags");
            model.SearchQuery = _orginalSearchQuery;
            return model;
        }

        private void SearchItemName(SearchViewModel model, string itemName)
        {
            // Without double escapes:
            // (:|=|;|>|<)(([\w\!\~\-_\.\/:]+)|(\"|').+(\"|'))
            Regex inurlRegex = new Regex(
                "-" + itemName +
                "(:|=|;|>|<)(([\\w\\!\\~\\-_\\.\\/:]+)|(\"|').+(\"|'))",
                RegexOptions.IgnoreCase);
            _defaultQuery = inurlRegex.Replace(_defaultQuery,"");
            if (inurlRegex.Match(model.SearchQuery).Success)
            {
                var item = inurlRegex.Match(model.SearchQuery).Value;
                Regex rgx = new Regex("-"+ itemName +"(:|=|;|>|<)", RegexOptions.IgnoreCase);

                // To Search Type
                var itemNameSearch = rgx.Match(item).Value;
                if (!itemNameSearch.Any()) return;

                // replace
                item = rgx.Replace(item, string.Empty);
                

                var searchForOption = itemNameSearch[itemNameSearch.Length - 1].ToString();
                model.SetAddSearchForOptions(searchForOption);                    
                
                // Remove parenthesis
                item = item.Replace("\"", string.Empty);
                item = item.Replace("'", string.Empty);
                model.SetAddSearchFor(item.Trim());
                model.SetAddSearchInStringType(itemName);
            }
        }

        public string QuerySafe(string query)
        {
            query = query.Trim();
            return query;
        }

        public string QueryShortcuts(string query)
        {
            query = query.Replace("-inurl", "-FilePath");
            return query;
        }

        // The amount of search results on one single page
        // Including the trash page
        private const int NumberOfResultsInView = 20;

        private int GetLastPageNumber(int fileIndexQueryCount)
        {
            var searchLastPageNumbers = (RoundUp(fileIndexQueryCount) / NumberOfResultsInView) - 1;

            if (fileIndexQueryCount <= NumberOfResultsInView)
            {
                searchLastPageNumbers = 0;
            }
            return searchLastPageNumbers;
       }

        // Round features:
        public int RoundUp(int toRound)
        {
            // 10 => ResultsInView
            if (toRound % NumberOfResultsInView == 0) return toRound;
            return (NumberOfResultsInView - toRound % NumberOfResultsInView) + toRound;
        }

        public int RoundDown(int toRound)
        {
            return toRound - toRound % 10;
        }


    }
}
